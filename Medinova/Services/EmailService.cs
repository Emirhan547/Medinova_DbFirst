using MailKit.Net.Smtp;
using MailKit.Security;
using Medinova.Helpers;
using Medinova.Models;
using MimeKit;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Medinova.Services
{
    public class EmailService : IDisposable
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _username;
        private readonly string _password;

        private readonly MedinovaContext _context;
        private bool _disposed = false;

        public EmailService()
        {


            _smtpServer = GetSetting("MEDINOVA_SMTP_SERVER", "SmtpServer");
            _username = GetSetting("MEDINOVA_SMTP_USERNAME", "SmtpUsername");
            _password = GetSetting("MEDINOVA_SMTP_PASSWORD", "SmtpPassword");

            // Port
            var envPort = Environment.GetEnvironmentVariable("MEDINOVA_SMTP_PORT");
            if (!int.TryParse(envPort, out _smtpPort))
            {
                var cfgPort = (ConfigurationManager.AppSettings["SmtpPort"] ?? "").Trim();
                if (!int.TryParse(cfgPort, out _smtpPort))
                    _smtpPort = 587; // default
            }

            _context = new MedinovaContext();
        }

        private static string GetSetting(string envKey, string configKey)
        {
            var envVal = Environment.GetEnvironmentVariable(envKey);
            if (!string.IsNullOrWhiteSpace(envVal))
                return envVal.Trim();

            return (ConfigurationManager.AppSettings[configKey] ?? "").Trim();
        }

        public async Task SendAppointmentConfirmation(Appointment appointment)
        {
            if (appointment == null) return;
            if (string.IsNullOrWhiteSpace(appointment.Email)) return;

            var doctor = _context.Doctors.Find(appointment.DoctorId);

            var subject = "Randevu Onayı - Medinova";

            var body = $@"
<h2>Sayın {appointment.FullName},</h2>
<p>Randevunuz başarıyla oluşturulmuştur.</p>

<h3>Randevu Detayları</h3>
<ul>
    <li><strong>Doktor:</strong> {doctor?.FullName}</li>
    <li><strong>Tarih:</strong> {appointment.AppointmentDate:dd MMMM yyyy, dddd}</li>
    <li><strong>Saat:</strong> {appointment.AppointmentTime}</li>
    <li><strong>Randevu No:</strong> #{appointment.AppointmentId}</li>
</ul>

<p>Lütfen randevu saatinizden 15 dakika önce hastanede olunuz.</p>
<p>Medinova Ekibi</p>";

            await SendEmail(appointment.Email, subject, body);
        }

        public async Task SendAppointmentCancellation(Appointment appointment)
        {
            if (appointment == null) return;
            if (string.IsNullOrWhiteSpace(appointment.Email)) return;

            var subject = "Randevu İptali - Medinova";

            var body = $@"
<h2>Sayın {appointment.FullName},</h2>
<p>Randevunuz iptal edilmiştir.</p>

<h3>İptal Edilen Randevu</h3>
<ul>
    <li><strong>Tarih:</strong> {appointment.AppointmentDate:dd MMMM yyyy}</li>
    <li><strong>Saat:</strong> {appointment.AppointmentTime}</li>
    <li><strong>Randevu No:</strong> #{appointment.AppointmentId}</li>
</ul>

{(!string.IsNullOrEmpty(appointment.CancellationReason)
    ? $"<p><strong>İptal Nedeni:</strong> {appointment.CancellationReason}</p>"
    : "")}

<p>Yeni randevu oluşturmak için sistemimizi kullanabilirsiniz.</p>
<p>Medinova Ekibi</p>";

            await SendEmail(appointment.Email, subject, body);
        }


        private async Task SendEmail(string toEmail, string subject, string htmlBody)
        {
            var emailLog = new EmailLog
            {
                RecipientEmail = toEmail,
                Subject = subject,
                Body = htmlBody,
                SentDate = DateTime.Now,
                IsSent = false
            };

            try
            {
                // Config kontrolü (boşsa direkt logla)
                if (string.IsNullOrWhiteSpace(_smtpServer) ||
                    string.IsNullOrWhiteSpace(_username) ||
                    string.IsNullOrWhiteSpace(_password) ||
                    _smtpPort <= 0)
                {
                    emailLog.ErrorMessage =
                        $"SMTP ayarları eksik. Server='{_smtpServer}', Port='{_smtpPort}', User='{_username}'";
                    return;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Medinova", _username));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

                using (var smtp = new SmtpClient())
                {
                    smtp.Timeout = 20000;
                    await smtp.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.Auto);
                    await smtp.AuthenticateAsync(_username, _password);
                    await smtp.SendAsync(message);
                    await smtp.DisconnectAsync(true);
                }

                emailLog.IsSent = true;
                emailLog.ErrorMessage = null;
            }
            catch (Exception ex)
            {
                emailLog.IsSent = false;
                emailLog.ErrorMessage = ex.ToString();
            }
            finally
            {
                try
                {
                    _context.EmailLogs.Add(emailLog);
                    _context.SaveChanges();
                }
                catch
                {
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _context.Dispose();
                _disposed = true;
            }
        }
    }
}
