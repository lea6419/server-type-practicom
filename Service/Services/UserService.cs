using BCrypt.Net; // הוסף שימוש בספריית BCrypt
using Microsoft.Extensions.Logging;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly AuthService _authService;
    private readonly ILogger<UserService> _logger; // הוסף ILogger

    public UserService(IUserRepository userRepository, AuthService authService, ILogger<UserService> logger) // הוסף ILogger ל-constructor
    {
        _userRepository = userRepository;
        _authService = authService;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        try
        {
            return await _userRepository.GetAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users.");
            throw; // זרוק את השגיאה לאחר רישום הלוג
        }
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found.");
                return null; // החזר null במקום לזרוק חריגה
            }
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving user with ID {userId}.");
            throw;
        }
    }

    public async Task<User> CreateUserAsync(User user)
    {
        try
        {
            // בדיקה אם קיים משתמש עם אותו אימייל
            var existingUser = await _userRepository.GetByUsernameAsync(user.Email);
            if (existingUser != null)
            {
                _logger.LogWarning($"Username {user.Email} already taken.");
                throw new ArgumentException("Username already taken.");
            }

            // הצפנת סיסמה
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            await _userRepository.AddAsync(user);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating user {user.Email}.");
            throw;
        }
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        try
        {
            var existingUser = await _userRepository.GetByIdAsync(user.Id);
            if (existingUser == null)
            {
                _logger.LogWarning($"User with ID {user.Id} not found for update.");
                throw new ArgumentException("User not found.");
            }

            // הצפנת סיסמה רק אם סופקה סיסמה חדשה
            if (!string.IsNullOrEmpty(user.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }

            await _userRepository.UpdateAsync(user);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating user with ID {user.Id}.");
            throw;
        }
    }

    public async Task<User> DeleteUserAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found for deletion.");
                throw new ArgumentException("User not found.");
            }

            await _userRepository.DeleteAsync(userId);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting user with ID {userId}.");
            throw;
        }
    }

    public async Task<User> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _userRepository.LoginAsync(email, password);
            if (user == null)
            {
                _logger.LogWarning($"Login failed for user {email}.");
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // בדיקת סיסמה מוצפנת
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                _logger.LogWarning($"Login failed for user {email} due to incorrect password.");
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Login failed for user {email}.");
            throw;
        }
    }

    public async Task<User?> GetUserByTokenAsync(string token)
    {
        try
        {
            var userId = _authService.GetUserIdFromToken(token);
            return await _userRepository.GetByIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by token.");
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetUsersByTypeistAsync(int typeistId)
    {
        try
        {
            return await _userRepository.GetAllAsync(u => u.TypeistId == typeistId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving users for typeist {typeistId}.");
            throw;
        }
    }
}