

namespace Service.Services
{
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using MimeKit;
    using System;
    using System.Threading.Tasks;

    public class EmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("י ז", "z036166419@gmail.com"));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                    await client.AuthenticateAsync("z036166419@gmail.com", "סיסמה_לאפליקציה");

                    await client.SendAsync(message);

                    await client.DisconnectAsync(true);
                    Console.WriteLine("Email sent successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }
}

