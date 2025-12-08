

namespace FileAPI.Models;


public class FileRecord 
{
    public Guid Id { get; set; }
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Sha256 { get; set; } = default!;
    public DateTime CreatedUtc { get; set; }
}