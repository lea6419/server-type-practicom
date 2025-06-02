using Core.Interface.Repositories;
using Core.Models;
using Microsoft.Extensions.Logging;
using Service.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class VerificationService
    {
        private readonly IVerificationCodeRepository _verificationCodeRepository;
        private readonly EmailService _emailService;
        private readonly ILogger<VerificationService> _logger;

        public VerificationService(
            IVerificationCodeRepository verificationCodeRepository,
            EmailService emailService,
            ILogger<VerificationService> logger)
        {
            _verificationCodeRepository = verificationCodeRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> SendVerificationCodeAsync(string email)
        {
            try
            {
                // מחיקה של כל הקודים הקודמים
                await _verificationCodeRepository.RemoveExistingCodesAsync(email);

                // יצירת קוד אימות אקראי בן 6 ספרות
                var code = new Random().Next(100000, 999999).ToString();

                var verificationCode = new VerificationCode
                {
                    UserEmail = email,
                    Code = code,
                    Expiration = DateTime.UtcNow.AddMinutes(10)
                };

                await _verificationCodeRepository.AddAsync(verificationCode);

                var subject = "קוד אימות למערכת";
                var body = $"<p>קוד האימות שלך הוא: <strong>{code}</strong></p>";

                await _emailService.SendEmailAsync(email, subject, body);
                _logger.LogInformation("Verification code sent to {Email}", email);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification code to {Email}", email);
                return false;
            }
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            var validCode = await _verificationCodeRepository.GetValidCodeAsync(email, code);
            if (validCode != null)
            {
                // אפשר למחוק את הקוד אחרי אימות מוצלח
                await _verificationCodeRepository.RemoveExistingCodesAsync(email);
                return true;
            }
            return false;

        }
    }
}
