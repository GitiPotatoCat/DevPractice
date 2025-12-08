

using System.Text;

namespace FileAPI.Utilities;


public static class FileValidation 
{
    public static readonly HashSet<string> PermittedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".pdf"
        };



    private static readonly Dictionary<string, List<byte[]>> _fileSignatures =
        new() 
        {
            { 
                ".png", new List<byte[]> 
                {
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
                }
            },
            {
                ".jpg", new List<byte[]> 
                {
                    new byte[] { 0xFF, 0xD8, 0xFF }
                }
            },
            {
                ".jpeg", new List<byte[]> 
                {
                    new byte[] { 0xFF, 0xD8, 0xFF }
                }
            },
            {
                ".pdf", new List<byte[]> 
                {
                    Encoding.ASCII.GetBytes("%PDF")
                }
            }
        };



    public static bool HasAllowedExtension(string fileName) =>
        PermittedExtensions.Contains(Path.GetExtension(fileName));


    public static bool CheckSignature(Stream stream, string fileName) 
    {
        var ext = Path.GetExtension(fileName);
        if (!_fileSignatures.TryGetValue(ext, out var signatures))
            return false;

        var maxLen = signatures.Max(s => s.Length);
        var header = new byte[maxLen];
        stream.Position = 0;
        var read = stream.Read(header, 0, maxLen);
        stream.Position = 0;
        foreach (var sig in signatures) 
        {
            if (read >= sig.Length && header.Take(sig.Length).SequenceEqual(sig))
                return true;
        }

        return false;
    }
}