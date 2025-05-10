
using System;


public class UserFile:IEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }  // מפתח זר (המשתמש                 // שהעלה את הקובץ)
    public FileStatus Status { get; set; }  // 
    public string FileName { get; set; }  // שם הקובץ

    public string   Name { get; set; }         
    public string? OriginalFileUrl { get; set; }  // קובץ סרוק שהועלה ע"י המשתמש
    public string? TranscribedFileUrl { get; set; }  // קובץ מוקלד שהועלה ע"י הקלדנית
    public string UploadedBy { get; set; } // User / Admin / Transcriber
    public string FileType { get; set; }  // סוג הקובץ (PDF, DOCX וכו')
    public DateTime Deadline { get; set; }  // דדליין להשלמת הקובץ
    public DateTime CreatedAt { get; set; }  // תאריך יצירה
    public DateTime UpdatedAt { get; set; }  // תאריך עדכון
    public bool IsDeleted { get; set; } = false; // סימון מחיקה רכה
    public int Size { get; set; }

    // קשרים
    public User User { get; set; }  // קשר למשתמש
   
}
