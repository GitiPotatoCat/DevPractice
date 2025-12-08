using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;

namespace FileWeb.Pages;

public class UploadModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public string? Message { get; set; }



    public UploadModel(IHttpClientFactory httpClientFactory) 
    {
        _httpClientFactory = httpClientFactory;
    }


    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync() 
    {
        var file = Request.Form.Files["file"];
        if (file is null  ||  file.Length == 0) 
        {
            Message = "No file selected.";
            return Page();
        }

        var client = _httpClientFactory.CreateClient("FileAPI");
        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        var streamContent = new StreamContent(stream);

        streamContent.Headers.ContentType =
            new MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.FileName);

        var res = await client.PostAsync("/files", content);
        Message = 
            res.IsSuccessStatusCode ? 
            $"Uploaded successfully: {await res.Content.ReadAsStringAsync()}" 
                                    : 
            $"Upload failed: {res.StatusCode} - { await res.Content.ReadAsStringAsync() }";

        return Page();
    }
}
