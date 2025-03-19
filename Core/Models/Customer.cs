

public class Customer:IEntity
{
    public int Id { get; set; }
    public string CustomerName { get; set; }  // שם הלקוח
    public string ContactEmail { get; set; }  // דוא"ל של הלקוח
    public string ContactPhone { get; set; }  // טלפון של הלקוח
    public DateTime CreatedAt { get; set; }  // תאריך יצירה
    public DateTime UpdatedAt { get; set; }  // תאריך עדכון

    // קשרים
    //  public List<User> Users { get; set; }  // קשר אחד לרבים עם משתמשים'

    public User user { get; set; }
    public List<UserFile> Files { get; set; }  // קשר אחד לרבים עם קבצים
}
