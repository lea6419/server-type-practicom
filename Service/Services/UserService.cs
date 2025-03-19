

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private AuthService _authService;

    public UserService(IUserRepository userRepository, AuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        try
        {
            return await _userRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            // ייתכן שתשקול להוסיף יומן לוגים (logging) כאן
            throw new Exception("Error retrieving users.", ex);
        }
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }
            return user;
        }
        catch (Exception ex)
        {
            // ניתן גם להוסיף טיפול בשגיאות מסוגים שונים כאן
            throw new Exception("Error retrieving user.", ex);
        }
    }

    public async Task<User> CreateUserAsync(User user)
    {
        try
        {
            // למשל, נוודא שאין כבר משתמש עם אותו שם משתמש או אימייל
            var existingUser = await _userRepository.GetByUsernameAsync(user.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("Username already taken.");
            }

            // הוספת המשתמש
            await _userRepository.AddAsync(user);
            return user;
        }
        catch (Exception ex)
        {
            // יש להחזיר שגיאה במידה ויש בעיה בהוספת המשתמש
            throw new Exception("Error creating user.", ex);
        }
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        try
        {
            var existingUser = await _userRepository.GetByIdAsync(user.Id);
            if (existingUser == null)
            {
                throw new ArgumentException("User not found.");
            }

            await _userRepository.UpdateAsync(user);
            return user;
        }
        catch (Exception ex)
        {
            // טיפול בשגיאות בעדכון
            throw new Exception("Error updating user.", ex);
        }
    }

    public async Task<User> DeleteUserAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            await _userRepository.DeleteAsync(userId);
            return user;
        }
        catch (Exception ex)
        {
            // טיפול בשגיאות במחיקת משתמש
            throw new Exception("Error deleting user.", ex);
        }
    }

    public async Task<User> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _userRepository.LoginAsync(email, password);
            if (user==null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return user;
        }
        catch (Exception ex)
        {
            // טיפול בשגיאות במהלך התחברות
            throw new Exception("Login failed.", ex);
        }
    }
    public Task<User> ValidateUser(string userName, string password)
    {
        // חיפוש המשתמש במאגר הנתונים
        var user = _userRepository.LoginAsync(password, userName);

        // אם לא נמצא משתמש, מחזירים null
        if (user == null)
        {
            return null;
        }
        

        // אם נמצא משתמש, מחזירים את המשתמש (כולל את התפקידים שלו)
        return user;
    }
    public async Task<User?> GetUserByTokenAsync(string token)
    {
        try
        {
            var userId = _authService.GetUserIdFromToken(token); // החלף במימוש שלך
            return await _userRepository.GetByIdAsync(userId);
        }
        catch (Exception ex)
        {
            // טיפול בשגיאות
            throw new Exception("Error retrieving user by token.", ex);
        }
    }
    public async Task<IEnumerable<User>> GetUsersByTypeistAsync(int typeistId)
    {
        try
        {
            // נניח ש-User כולל שדה TypeistId שמקשר
            return await _userRepository.GetAllAsync(u => u.TypeistId == typeistId);
        }
        catch (Exception ex)
        {
            // טיפול בשגיאות
            throw new Exception($"Error retrieving users for typeist {typeistId}.", ex);
        }
    }
}

