

public class Progress:IEntity
{
    public int Id { get; set; }
    public int FileId { get; set; }  // מפתח זר (הקובץ שהתקדמותו מתועדת)
    public int UserId { get; set; }  // מפתח זר (המשתמש שעוקב אחרי ההתקדמות)
    public string Status { get; set; }  // סטטוס הקובץ (ביצוע, הושלם וכו')
    public int ProgressPercentage { get; set; }  // אחוז התקדמות
    public DateTime UpdatedAt { get; set; }  // תאריך עדכון

    // קשרים
    public UserFile File { get; set; }  // קשר לקובץ
    public User User { get; set; }  // קשר למשתמש
}
