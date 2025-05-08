public class UserFileDto
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string? OriginalFileUrl { get; set; }  // קובץ סרוק שהועלה ע"י המשתמש
    public string? TranscribedFileUrl { get; set; }  // קובץ מוקלד שהועלה ע"י הקלדנית
    public string FileType { get; set; }
    public DateTime Deadline { get; set; }
    public FileStatus Status { get; set; }
    public int Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; }



    public string DownloadUrl { get; set;}
}
