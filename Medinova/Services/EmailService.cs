using MailKit.Net.Smtp;
using MailKit.Security;
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
        private bool _disposed;

        public EmailService()
        {
            _smtpServer = GetSetting("MEDINOVA_SMTP_SERVER", "SmtpServer");
            _username = GetSetting("MEDINOVA_SMTP_USERNAME", "SmtpUsername");
            _password = GetSetting("MEDINOVA_SMTP_PASSWORD", "SmtpPassword");

            var envPort = Environment.GetEnvironmentVariable("MEDINOVA_SMTP_PORT");
            if (!int.TryParse(envPort, out _smtpPort))
            {
                var cfgPort = ConfigurationManager.AppSettings["SmtpPort"];
                if (!int.TryParse(cfgPort, out _smtpPort))
                    _smtpPort = 587;
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

        // =====================================================
        // RANDEVU ONAY MAİLİ
        // =====================================================
        public async Task SendAppointmentConfirmation(Appointment appointment)
        {
            if (appointment == null) return;
            if (string.IsNullOrWhiteSpace(appointment.Email)) return;

            var doctor = _context.Doctors.Find(appointment.DoctorId);

            var subject = "Randevu Onayı - Medinova";

            var body = $@"
<div style='max-width:600px;margin:0 auto;
            font-family:Segoe UI,Arial,sans-serif;
            background:#ffffff;border-radius:12px;
            border:1px solid #e5e7eb;overflow:hidden'>

    <div style='background:#2563eb;color:#ffffff;padding:24px 32px'>
        <h1 style='margin:0;font-size:22px'>Randevu Onayı</h1>
        <p style='margin:6px 0 0;font-size:14px;opacity:.9'>
            Medinova Sağlık Yönetimi
        </p>
    </div>

    <div style='padding:32px;color:#111827'>
        <p style='font-size:15px;margin-bottom:20px'>
            Sayın <strong>{appointment.FullName}</strong>,
        </p>

        <p style='font-size:14px;line-height:1.6;margin-bottom:24px'>
            Randevunuz <strong>başarıyla oluşturulmuştur</strong>.
            Aşağıda randevunuza ait detayları bulabilirsiniz:
        </p>

        <div style='background:#f9fafb;border:1px solid #e5e7eb;
                    border-radius:10px;padding:20px;margin-bottom:24px'>
            <table style='width:100%;font-size:14px'>
                <tr>
                    <td style='padding:8px 0;color:#6b7280'>👨‍⚕️ Doktor</td>
                    <td style='padding:8px 0;font-weight:600'>
                        {doctor?.FullName ?? "-"}
                    </td>
                </tr>
                <tr>
                    <td style='padding:8px 0;color:#6b7280'>📅 Tarih</td>
                    <td style='padding:8px 0;font-weight:600'>
                        {appointment.AppointmentDate:dd MMMM yyyy, dddd}
                    </td>
                </tr>
                <tr>
                    <td style='padding:8px 0;color:#6b7280'>⏰ Saat</td>
                    <td style='padding:8px 0;font-weight:600'>
                        {appointment.AppointmentTime}
                    </td>
                </tr>
                <tr>
                    <td style='padding:8px 0;color:#6b7280'>🆔 Randevu No</td>
                    <td style='padding:8px 0;font-weight:600'>
                        #{appointment.AppointmentId}
                    </td>
                </tr>
            </table>
        </div>

        <div style='background:#eff6ff;border-left:4px solid #2563eb;
                    padding:16px;border-radius:6px;margin-bottom:24px'>
            <p style='margin:0;font-size:13px'>
                ⏰ Randevu saatinizden <strong>15 dakika önce</strong>
                hastanede bulunmanızı rica ederiz.
            </p>
        </div>

        <p style='font-size:14px;font-weight:600'>
            Medinova Ekibi
        </p>
    </div>

    <div style='background:#f9fafb;padding:16px 32px;
                font-size:12px;color:#6b7280;text-align:center'>
        © {DateTime.Now.Year} Medinova · Tüm hakları saklıdır
    </div>
</div>";

            await SendEmail(appointment.Email, subject, body);
        }

        // =====================================================
        // RANDEVU İPTAL MAİLİ
        // =====================================================
        public async Task SendAppointmentCancellation(Appointment appointment)
        {
            if (appointment == null) return;
            if (string.IsNullOrWhiteSpace(appointment.Email)) return;

            var subject = "Randevu İptali - Medinova";

            var body = $@"
<h2>Sayın {appointment.FullName},</h2>
<p>Randevunuz iptal edilmiştir.</p>

<ul>
    <li><strong>Tarih:</strong> {appointment.AppointmentDate:dd MMMM yyyy}</li>
    <li><strong>Saat:</strong> {appointment.AppointmentTime}</li>
    <li><strong>Randevu No:</strong> #{appointment.AppointmentId}</li>
</ul>

{(!string.IsNullOrEmpty(appointment.CancellationReason)
    ? $"<p><strong>İptal Nedeni:</strong> {appointment.CancellationReason}</p>"
    : "")}

<p>Medinova Ekibi</p>";

            await SendEmail(appointment.Email, subject, body);
        }

        // =====================================================
        // MAİL GÖNDER + LOG
        // =====================================================
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
                if (string.IsNullOrWhiteSpace(_smtpServer) ||
                    string.IsNullOrWhiteSpace(_username) ||
                    string.IsNullOrWhiteSpace(_password))
                {
                    emailLog.ErrorMessage = "SMTP ayarları eksik.";
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
            }
            catch (Exception ex)
            {
                emailLog.ErrorMessage = ex.ToString();
            }
            finally
            {
                try
                {
                    _context.EmailLogs.Add(emailLog);
                    _context.SaveChanges();
                }
                catch { }
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
