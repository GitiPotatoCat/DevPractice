
using FileAPI.Data;
using FileAPI.Models;
using FileAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.WebUtilities;


namespace FileAPI.Controllers;


[ApiController]
[Route("files")]
public class FilesController : ControllerBase
{
    private readonly FileDbContext _db;

    public FilesController(FileDbContext db) 
    {
        _db = db;
    }


    [HttpPost]
    [RequestSizeLimit(5L * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 5L * 1024 * 1024)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file) 
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file.");

        var originalName = Path.GetFileName(file.FileName);
        if (!FileValidation.HasAllowedExtension(originalName))
            return BadRequest("Disallowed file type.");

        await using var signatureStream = file.OpenReadStream();
        if (!FileValidation.CheckSignature(signatureStream, originalName))
            return BadRequest("File signature mismatch.");

        string sha256;
        await using (var hashStream = file.OpenReadStream()) 
                                            using (var sha = SHA256.Create()) 
                                            {
                                                var hash = await sha.ComputeHashAsync(hashStream);
                                                sha256 = Convert.ToHexString(hash);
                                            }


        byte[] bytes;
        await using (var ms = new MemoryStream()) 
            {
                await file.CopyToAsync(ms);
                bytes = ms.ToArray();
            }

            var record = new FileRecord {
                Id = Guid.NewGuid(),
                OriginalName = originalName,
                ContentType = file.ContentType,
                Size = file.Length,
                Sha256 = sha256,
                CreatedUtc = DateTime.UtcNow
            };


        _db.Files.Add(record);
        _db.Entry(record).Property("Data").CurrentValue = bytes;
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = record.Id },
                                            new 
                                            {
                                                record.Id,
                                                record.OriginalName,
                                                record.ContentType,
                                                record.Size,
                                                record.CreatedUtc
                                            });            
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) 
    {
        var record = await _db.Files.FirstOrDefaultAsync(f => f.Id == id);
        if (record is null)
            return NotFound();

        var data = _db.Entry(record).Property<byte[]>("Data").CurrentValue;

        if (data is null || data.Length == 0)
            return NotFound("File data missing.");


        var stream = new MemoryStream(data); // stream out of DB
        
        return File(stream, record.ContentType, record.OriginalName);
    }



    [HttpPost("stream")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadStreamed() 
    {
        if (!Request.HasFormContentType 
            || MediaTypeHeaderValue.TryParse(Request.ContentType, out var mediaType) 
            || string.IsNullOrEmpty(mediaType.Boundary.Value)) { return new UnsupportedMediaTypeResult(); }



        var reader = new Microsoft.AspNetCore.WebUtilities.MultipartReader(mediaType.Boundary.Value, Request.Body);
        MultipartSection section;

        while ((section = await reader.ReadNextSectionAsync()) != null) 
        {
            var hasContentDisposition = ContentDispositionHeaderValue
                            .TryParse(section.ContentDisposition, out var disposition);



            if (hasContentDisposition &&
                            disposition!.DispositionType.Equals("form-data") &&
                            !string.IsNullOrEmpty(disposition.FileName.Value)) {

                var originalName = Path.GetFileName(disposition.FileName.Value);
                if (FileValidation.HasAllowedExtension(originalName))
                    return BadRequest("Disallowed file type.");


                using var ms = new MemoryStream();
                await section.Body.CopyToAsync(ms);
                var bytes = ms.ToArray();

                using var sigStream = new MemoryStream(bytes);
                if (!FileValidation.CheckSignature(sigStream, originalName))
                    return BadRequest("File signature mismatch.");


                string sha256;
                using (var sha = SHA256.Create())
                    sha256 = Convert.ToHexString(sha.ComputeHash(bytes));

                var record = new FileRecord 
                {
                    Id = Guid.NewGuid(),
                    OriginalName = originalName,
                    ContentType = disposition.Name.HasValue ? disposition.Name.Value! : "application/octet-stream",
                    Size = bytes.LongLength,
                    Sha256 = sha256,
                    CreatedUtc = DateTime.UtcNow
                };

                _db.Files.Add(record);
                _db.Entry(record).Property("Data").CurrentValue = bytes;
                await _db.SaveChangesAsync();

                return CreatedAtAction
                (
                    nameof(Get), 
                    new { id = record.Id },
                    new 
                    {
                        record.Id,
                        record.OriginalName,
                        record.ContentType,
                        record.Size,
                        record.CreatedUtc
                    }
                );
            }   // end of 'if' statement
        }  // end of while loop

        return BadRequest("No file section.");
    }
}