using Microsoft.AspNetCore.Mvc;
using Service;

[ApiController]
[Route("api/[controller]")]
public class VerificationController : ControllerBase
{
    private readonly VerificationService _verificationService;

    public VerificationController(VerificationService verificationService)
    {
        _verificationService = verificationService;
    }

    [HttpPost("send-code")]
    public async Task<IActionResult> SendCode([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { error = "Email is required." });

        var result = await _verificationService.SendVerificationCodeAsync(email);
        if (result)
            return Ok(new { message = "Verification code sent." });

        return StatusCode(500, new { error = "Failed to send verification code." });
    }

    [HttpPost("verify-code")]
    public async Task<IActionResult> VerifyCode([FromQuery] string email, [FromQuery] string code)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            return BadRequest(new { error = "Email and code are required." });

        var verified = await _verificationService.VerifyCodeAsync(email, code);
        if (verified)
            return Ok(new { message = "Verification successful." });

        return Unauthorized(new { error = "Invalid or expired verification code." });
    }
}
