using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Email_Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
            // Initialize QuestPDF license (Community)
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, byte[]? attachment = null, string? attachmentName = null)
        {
            try
            {
                // Temporarily bypass certificate validation for servers with Name Mismatch or Untrusted Certs
                ServicePointManager.ServerCertificateValidationCallback = 
                    (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;

                using (var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port))
                {
                    client.EnableSsl = true; 
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_emailSettings.Sender, _emailSettings.Password);

                    string htmlBody = GetHtmlTemplate(body);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_emailSettings.Sender, _emailSettings.From),
                        Subject = subject,
                        Body = htmlBody,
                        IsBodyHtml = true 
                    };

                    mailMessage.To.Add(toEmail);

                    // Add attachment if provided
                    if (attachment != null && !string.IsNullOrEmpty(attachmentName))
                    {
                        var ms = new MemoryStream(attachment);
                        mailMessage.Attachments.Add(new Attachment(ms, attachmentName, "application/pdf"));
                    }

                    await client.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false;
            }
        }

        private string GetHtmlTemplate(string content)
        {
            // Convert plain text newlines to HTML line breaks
            string formattedContent = content.Replace("\n", "<br/>");
            string logoUrl = "https://i.postimg.cc/W4N26c0T/mainlogo.jpg";

            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #1e293b; margin: 0; padding: 0; background-color: #f1f5f9; }}
        .email-wrapper {{ width: 100%; background-color: #f1f5f9; padding: 40px 0; }}
        .email-card {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0; }}
        .header {{ padding: 30px; text-align: center; background-color: #ffffff; border-bottom: 2px solid #f1f5f9; }}
        .logo-img {{ height: 60px; width: auto; max-width: 200px; object-contain: fit; }}
        .content-body {{ padding: 40px; }}
        .status-badge {{ display: inline-block; background-color: #ecfdf5; color: #065f46; font-size: 12px; font-weight: 700; padding: 4px 12px; border-radius: 9999px; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 20px; border: 1px solid #10b981; }}
        .title {{ font-size: 24px; font-weight: 800; color: #0f172a; margin: 0 0 24px 0; letter-spacing: -0.025em; line-height: 1.2; text-transform: uppercase; }}
        .message-box {{ color: #475569; font-size: 15px; background-color: #f8fafc; padding: 25px; border-radius: 12px; border-left: 4px solid #10b981; margin-bottom: 30px; line-height: 1.8; }}
        .signature-section {{ margin-top: 40px; padding-top: 30px; border-top: 1px solid #f1f5f9; }}
        .signature-text {{ font-size: 14px; color: #64748b; margin-bottom: 2px; }}
        .signature-name {{ font-size: 16px; font-weight: 700; color: #0f172a; margin: 4px 0; }}
        .signature-title {{ color: #10b981; font-weight: 600; font-size: 13px; text-transform: uppercase; letter-spacing: 0.5px; }}
        .footer {{ text-align: center; padding: 25px; font-size: 12px; color: #94a3b8; background-color: #f8fafc; }}
        .footer-links {{ margin-bottom: 15px; }}
        .footer-links a {{ color: #10b981; text-decoration: none; margin: 0 10px; font-weight: 600; }}
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <div class='email-card'>
            <div class='header'>
                <img src='{logoUrl}' alt='Agora Food Logo' class='logo-img'>
            </div>
            
            <div class='content-body'>
                <div class='status-badge'>Fulfillment Required</div>
                <h1 class='title'>Stock Fulfillment Request</h1>
                
                <div class='message-box'>
                    {formattedContent}
                </div>
                
                <div class='signature-section'>
                    <p class='signature-text'>Message dispatched by</p>
                    <p class='signature-name'>Mir Mohammad Faruk</p>
                    <p class='signature-title'>Founder & CEO, Agora Food</p>
                    <div style='margin-top: 15px; font-size: 13px; color: #64748b;'>
                        <strong>M:</strong> +45 60818181<br>
                        <strong>A:</strong> Vognmandsmarken 45, 2mf, 2100 Copenhagen, Denmark
                    </div>
                </div>
            </div>
            
            <div class='footer'>
                <div class='footer-links'>
                    <a href='#'>Global Logistics</a> | <a href='#'>Vendor Portal</a> | <a href='#'>Support</a>
                </div>
                &copy; {DateTime.Now.Year} AGORA FOOD. All rights reserved.<br>
                This request is generated by the Agora Food Management System.
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}
