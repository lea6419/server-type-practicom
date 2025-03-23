using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly AuthService _authService;
    private readonly ILogger<UserController> _logger; // הוסף את ILogger

    public UserController(IUserService userService, AuthService authService, ILogger<UserController> logger) // הוסף ILogger ל-constructor
    {
        _userService = userService;
        _authService = authService;
        _logger = logger;
    }
    
    // רישום משתמש חדש
    [HttpPost("register")]

    public async Task<IActionResult> RegisterUser([FromBody] UserDto userDto)
    {
        _logger.LogInformation($"Registering user: {userDto.Email}");

        var user = new User
        {
            Username = userDto.FullName,
            Email = userDto.Email,
            Role = userDto.Role,
            Password = userDto.Password,
        };

        try
        {
            var result = await _userService.CreateUserAsync(user);
            _logger.LogInformation($"User {userDto.Email} registered successfully.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error registering user: {userDto.Email}");
            return BadRequest(new { message = "Error registering user." });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        _logger.LogInformation($"Login attempt for user: {model.Email}");

        var user = await _userService.LoginAsync(model.Email, model.Password);

        if (user == null)
        {
            _logger.LogWarning($"Login failed for user: {model.Email}");
            return BadRequest(new { message = "שם המשתמש או הסיסמא שגויים." });
        }

        var token = _authService.GenerateJwtToken(user.Username,user.Role,user.Id);

        _logger.LogInformation($"Login successful for user: {model.Email}");
        _logger.LogInformation($"Login response: Token={token}, User={(new { user.Id, user.Username, user.Email, user.Role })}");

        // החזרת פרטי המשתמש יחד עם הטוקן
        return Ok(new { Token = token, User = new { user.Id, user.Username, user.Email, user.Role } });
        
    }

    // קבלת פרטי משתמש לפי ID
    [HttpGet("{userId}")]
    [Authorize(Policy = "TypeistOnly")]
    public async Task<IActionResult> GetUserById(int userId)
    {
        _logger.LogInformation($"Getting user by ID: {userId}");

        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning($"User with ID {userId} not found.");
            return NotFound();
        }

        var userDto = new UserDto
        {
            FullName = user.Username,
            Email = user.Email,
            Role = user.Role,
            Password=user.Password
        };

        _logger.LogInformation($"User with ID {userId} found.");
        return Ok(userDto);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(int userId, [FromBody] UserDto userDto)
    {
        _logger.LogInformation($"Updating user with ID: {userId}");

        // וודא ש-userId ב-URL תואם ל-userId בטוקן (אם צריך)
        // ...

        var user = new User
        {
            Id = userId,
            Username = userDto.FullName,
            Email = userDto.Email,
            Role = userDto.Role,
            Password = userDto.Password //שים לב אם אתה רוצה לעדכן סיסמא, או לא.
        };

        try
        {
            var result = await _userService.UpdateUserAsync(user);
            _logger.LogInformation($"User with ID {userId} updated successfully.");
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating user with ID: {userId}");
            return BadRequest(new { message = "Error updating user." });
        }
    }
    // מחיקת משתמש
    [HttpDelete("{userId}")]
    [Authorize(Policy = "TypeistOnly")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        _logger.LogInformation($"Deleting user with ID: {userId}");

        try
        {
            var result = await _userService.DeleteUserAsync(userId);
            _logger.LogInformation($"User with ID {userId} deleted successfully.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting user with ID: {userId}");
            return BadRequest(new { message = "Error deleting user." });
        }
    }
    [HttpGet("client")]
    [Authorize] // רק משתמשים מחוברים יכולים לגשת לפרופיל
    public async Task<IActionResult> GetUsers()
    {
            _logger.LogInformation("Getting users.");
        try
        {
            var user = await _userService.GetAllUsersAsync();
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting get client user.");
            return BadRequest(new { message = "Error getting user client." });
        }
       
    }

    //[HttpGet("profile")]
    //[Authorize] // רק משתמשים מחוברים יכולים לגשת לפרופיל
    //public async Task<IActionResult> GetUserProfile()
    //{
    //    _logger.LogInformation("Getting user profile.");

    //    try
    //    {
    //        var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
    //        if (string.IsNullOrEmpty(token))
    //        {
    //            return Unauthorized();
    //        }

    //        var user = await _userService.GetUserByTokenAsync(token);
    //        if (user == null)
    //        {
    //            return NotFound();
    //        }

    //        // החזרת פרטי משתמש (ללא סיסמה)
    //        return Ok(new
    //        {
    //            user.Id,
    //            user.Username,
    //            user.Email,
    //            user.Role,
    //            user.TypeistId
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error getting user profile.");
    //        return BadRequest(new { message = "Error getting user profile." });
    //    }
    //}
    //[HttpGet("typeist/{typeistId}/clients")]
    //[Authorize(Policy = "TypeistOnly")] // רק קלדניות יכולות לקבל רשימה זו
    //public async Task<IActionResult> GetClientsByTypeist(int typeistId)
    //{
    //    _logger.LogInformation($"Getting clients for typeist ID: {typeistId}");

    //    try
    //    {
    //        var users = await _userService.GetUsersByTypeistAsync(typeistId);
    //        var clients = users.Where(u => u.Role == "Client"); // סינון לקוחות בלבד
    //        return Ok(clients);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, $"Error getting clients for typeist ID: {typeistId}");
    //        return BadRequest(new { message = $"Error getting clients for typeist ID: {typeistId}" });
    //    }
    //}

}
