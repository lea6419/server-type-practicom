using System.Drawing;

public class FileDto
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public DateTime UploadDate { get; set; }
    public int UserId { get; set; }

    public string UserName { get; set; }

    public int Size { get; set; }
    public string FileType { get; set; }
}
