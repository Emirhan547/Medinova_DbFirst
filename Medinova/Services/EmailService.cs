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
        private bool _disposed = false;

        public EmailService()
        {
            _smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
            _smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            _username = ConfigurationManager.AppSettings["SmtpUsername"];
            _password = ConfigurationManager.AppSettings["SmtpPassword"];

            _context = new MedinovaContext();
        }

        // ---------------------------
        // RANDEVU ONAY MAİLİ
        // ---------------------------
        public async Task SendAppointmentConfirmation(Appointment appointment)
        {
            var patient = _context.Users.Find(appointment.PatientId);
            var doctor = _context.Doctors.Find(appointment.DoctorId);

            if (patient == null || string.IsNullOrEmpty(patient.UserName))
                return;

            var subject = "Randevu Onayı - Medinova";

            var body = $@"
<h2>Sayın {patient.FirstName} {patient.LastName},</h2>
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

            await SendEmail(patient.UserName, subject, body);
        }

        // ---------------------------
        // RANDEVU İPTAL MAİLİ
        // ---------------------------
        public async Task SendAppointmentCancellation(Appointment appointment)
        {
            var patient = _context.Users.Find(appointment.PatientId);

            if (patient == null || string.IsNullOrEmpty(patient.UserName))
                return;

            var subject = "Randevu İptali - Medinova";

            var body = $@"
<h2>Sayın {patient.FirstName} {patient.LastName},</h2>
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

            await SendEmail(patient.UserName, subject, body);
        }

        // ---------------------------
        // MAİL GÖNDERME + LOG
        // ---------------------------
        private async Task SendEmail(string toEmail, string subject, string htmlBody)
        {
            var emailLog = new EmailLog
            {
                RecipientEmail = toEmail,
                Subject = subject,
                Body = htmlBody,
                SentDate = DateTime.Now
            };

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Medinova", _username));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                message.Body = new BodyBuilder
                {
                    HtmlBody = htmlBody
                }.ToMessageBody();

                using (var smtp = new SmtpClient())
                {
                    await smtp.ConnectAsync(_smtpServer, _smtpPort, SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(_username, _password);
                    await smtp.SendAsync(message);
                    await smtp.DisconnectAsync(true);
                }

                emailLog.IsSent = true;
            }
            catch (Exception ex)
            {
                emailLog.IsSent = false;
                emailLog.ErrorMessage = ex.Message;
            }
            finally
            {
                _context.EmailLogs.Add(emailLog);
                _context.SaveChanges();
            }
        }

        // ---------------------------
        // DISPOSE
        // ---------------------------
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
