

public class Backup : IEntity
{
    public int Id { get ; set ; }
    public int BackupId { get; set; }  // מפתח ראשי
    public int UserId { get; set; }  // מפתח זר (המשתמש שהשלים את הגיבוי)
    public int FileId { get; set; }  // מפתח זר (הקובץ שבוצע לו גיבוי)
    public string BackupPath { get; set; }  // נתיב לקובץ הגיבוי
    public DateTime CreatedAt { get; set; }  // תאריך יצירה של הגיבוי

    // קשרים
    public UserFile File { get; set; }  // קשר לקובץ
    public User User { get; set; }  // קשר למשתמש
    
}
