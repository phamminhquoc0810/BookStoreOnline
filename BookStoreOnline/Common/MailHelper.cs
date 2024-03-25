using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net;

namespace BookStoreOnline.Common
{
    public class MailHelper
    {
        public void SendMail(string toEmailAddress, string subject, string content)
        {
            try
            {
                var fromEmailAddress = ConfigurationManager.AppSettings["FromEmailAddress"].ToString();

                var fromEmailPassword = ConfigurationManager.AppSettings["FromEmailPassword"].ToString();
                var SMTPHost = ConfigurationManager.AppSettings["SMTPHost"].ToString();
                var SMTPPort = ConfigurationManager.AppSettings["SMTPPort"].ToString();

                bool EnabledSSL = bool.Parse(ConfigurationManager.AppSettings["EnabledSSL"].ToString());
                string body = content;
                MailMessage message = new MailMessage(new MailAddress(fromEmailAddress), new MailAddress(toEmailAddress));
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = body;
                var client = new SmtpClient();
                client.Credentials = new NetworkCredential(fromEmailAddress, fromEmailPassword);
                client.Host = SMTPHost;
                client.EnableSsl = EnabledSSL;
                client.Port = !string.IsNullOrEmpty(SMTPPort) ? Convert.ToInt32(SMTPPort) : 0;
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
                throw; // Throw lại exception để có thể xem lỗi chi tiết.
            }
        }
        
    }
}