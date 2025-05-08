public class User:IEntity
{
    public int Id { get; set; }
    public string Username { get; set; }  // שם משתמש
    public string Password { get; set; }  // סיסמה (מומלץ לאחסן כ-hash)
    public string Role { get; set; }  // קלדנית/משתמש
    public string Email { get; set; }  // דוא"ל

    public int? TypeistId { get; set; } 
    public DateTime CreatedAt { get; set; }  // תאריך יצירה
    public DateTime UpdatedAt { get; set; }  // תאריך עדכון

    // קשרים
    public List<UserFile> Files { get; set; }  // קשר אחד לרבים עם קבצים
    public List<Progress> Progresses { get; set; }  // קשר אחד לרבים עם התקדמות

    public List<Customer> Customers { get; set; }
}
