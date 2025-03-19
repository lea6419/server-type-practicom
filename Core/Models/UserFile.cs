
using System;

public class UserFile:IEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }  // מפתח זר (המשתמש                 // שהעלה את הקובץ)
    public string Status { get; set; }  // 
    public string FileName { get; set; }  // שם הקובץ
    public string FilePath { get; set; }  // נתיב לקובץ (שמור ב-S3)
    public string FileType { get; set; }  // סוג הקובץ (PDF, DOCX וכו')
    public DateTime Deadline { get; set; }  // דדליין להשלמת הקובץ
    public DateTime CreatedAt { get; set; }  // תאריך יצירה
    public DateTime UpdatedAt { get; set; }  // תאריך עדכון
    public bool IsDeleted { get; set; } = false; // סימון מחיקה רכה
    public int Size { get; set; }

    // קשרים
    public User User { get; set; }  // קשר למשתמש
    public List<Progress> Progresses { get; set; }  // קשר אחד לרבים עם התקדמות
    public List<Backup> Backups { get; set; }  // קשר אחד לרבים עם גיבויים
    public SpeechToText SpeechToText { get; set; }  // קשר אחד לאחד עם המרת דיבור לטקסט
}
