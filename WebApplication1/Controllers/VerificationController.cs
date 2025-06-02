using Microsoft.AspNetCore.Mvc;
using Service;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerificationController : ControllerBase
    {
        private readonly VerificationService _verificationService;

        public VerificationController(VerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        // שליחת קוד אימות למייל
        [HttpPost("send-code")]
        public async Task<IActionResult> SendCode([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Email is required.");

            var result = await _verificationService.SendVerificationCodeAsync(email);
            if (result)
                return Ok("Verification code sent.");

            return StatusCode(500, "Failed to send verification code.");
        }

        // בדיקת קוד אימות
        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromQuery] string email, [FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                return BadRequest("Email and code are required.");

            var verified = await _verificationService.VerifyCodeAsync(email, code);
            if (verified)
                return Ok("Verification successful.");

            return Unauthorized("Invalid or expired verification code.");
        }
    }
}
