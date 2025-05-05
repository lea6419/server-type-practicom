public class UserFileDto
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string FileType { get; set; }
    public DateTime Deadline { get; set; }
    public int Status { get; set; }
    public int Size { get; set; }
    public DateTime CreatedAt { get; set; }

    public string DownloadUrl { get; set;}
}
