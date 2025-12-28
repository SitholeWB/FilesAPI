using Microsoft.AspNetCore.Http;

namespace Models;

public class UploadImageCommand
{
    public IFormFile File { get; set; }
    public string Description { get; set; }
    public IEnumerable<string> Tags { get; set; }
}