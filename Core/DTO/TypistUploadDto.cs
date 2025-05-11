using Microsoft.AspNetCore.Http;

public class TypistUploadDto
{
    public IFormFile File { get; set; }
    public string OriginalFileId { get; set; }
    public int FileId { get; set; }
}
