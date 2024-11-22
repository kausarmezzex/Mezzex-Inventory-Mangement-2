using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Mezzex_Inventory_Mangement.Services
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com"; // Replace with your SMTP server
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "faizraza349@gmail.com"; // Replace with your email
        private readonly string _smtpPass = "dkahtbtprxtsmjql"; // Replace with your email password

        public async Task SendOtpEmailAsync(string toEmail, string otp)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpUser),
                Subject = "Your Password Reset OTP",
                Body = $"Your OTP for password reset is: {otp}",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                smtpClient.EnableSsl = true;

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}
