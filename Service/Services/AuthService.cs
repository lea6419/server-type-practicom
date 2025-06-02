using Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Service;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly EmailService _emailService;

    public AuthService(IConfiguration configuration, ILogger<AuthService> logger,EmailService emailService)
    {
        _configuration = configuration;
        _logger = logger;
        _emailService = emailService;
    }


    public async Task<bool> SendVerificationCodeAsync(string userEmail)
    {
        try
        {
            var code = new Random().Next(100000, 999999).ToString();

            // מחיקה של קוד קודם אם קיים
            var existing = _context.VerificationCodes.FirstOrDefault(v => v.UserEmail == userEmail);
            if (existing != null)
            {
                _context.VerificationCodes.Remove(existing);
            }

            var newCode = new VerificationCode
            {
                UserEmail = userEmail,
                Code = code,
                Expiration = DateTime.UtcNow.AddMinutes(10) // תקף ל-10 דקות
            };

            _context.VerificationCodes.Add(newCode);
            await _context.SaveChangesAsync();

            var subject = "קוד אימות למערכת";
            var body = $"<p>קוד האימות שלך הוא: <strong>{code}</strong></p>";

            await _emailService.SendEmailAsync(userEmail, subject, body);
            _logger.LogInformation("Verification code sent and saved to DB for {Email}", userEmail);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send/save verification code to {Email}", userEmail);
            return false;
        }
    }


    public int GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "id");

            if (userIdClaim == null)
            {
                _logger.LogWarning("Token does not contain 'id' claim.");
                return -1;
            }

            int userId = int.Parse(userIdClaim.Value);
            _logger.LogInformation($"User ID extracted from token: {userId}");
            return userId;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogError(ex, "Token validation failed.");
            return -1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting user ID from token.");
            return -1;
        }
    }

    public string GenerateJwtToken(string username, string role, int id)
    {
        var key = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(key) || key.Length < 16)
        {
            throw new ArgumentException("JWT Key must be at least 16 characters long.");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim("id", id.ToString()) // הוספת 'id' ל-claims
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}