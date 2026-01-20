using Medinova.Attributes;
using Medinova.Dtos;
using Medinova.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Medinova.Areas.Doctor.Controllers
{
    [CustomAuthorize("Doctor")]
    public class DashboardController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var userId = Session["userId"] as int?;
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });

            // 🩺 Doctor kaydını BUL ya da OLUŞTUR (EDMX FIX)
            var doctor = context.Doctors
                .ToList()
                .FirstOrDefault(d => d.UserId == userId.Value);

            if (doctor == null)
            {
                var user = context.Users.Find(userId.Value);
                if (user == null)
                    return RedirectToAction("Logout", "Account", new { area = "" });

                doctor = new Models.Doctor
                {
                    UserId = user.UserId,
                    FullName = (user.FirstName + " " + user.LastName).Trim()
                };

                context.Doctors.Add(doctor);
                context.SaveChanges();
            }

            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(1);

            var todayAppointments = context.Appointments
                .Where(a => a.DoctorId == doctor.DoctorId &&
                            a.AppointmentDate >= startDate &&
                            a.AppointmentDate < endDate &&
                            a.Status == "Active")
                .OrderBy(a => a.AppointmentTime)
                .ToList();


            var overview = new DoctorAppointmentOverviewDto
            {
                DoctorName = doctor.FullName,
                ActiveAppointments = todayAppointments
            };

            ViewBag.TodayAppointments = todayAppointments;
            ViewBag.TotalToday = todayAppointments.Count;

            return View(overview);
        }

        public ActionResult Appointments()
        {
            var userId = Session["userId"] as int?;
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });

            var doctor = context.Doctors.FirstOrDefault(d => d.UserId == userId.Value);
            if (doctor == null)
                return RedirectToAction("Index");

            var appointments = context.Appointments
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            return View(appointments);
        }

        [HttpPost]
        public async Task<ActionResult> CancelAppointment(int id, string reason)
        {
            var appointment = context.Appointments.Find(id);
            if (appointment != null)
            {
                ApplyCancellation(appointment, reason, "CancelAppointment");
                await SendCancellationEmail(appointment);
            }

            return RedirectToAction("Appointments");
        }

        [HttpPost]
        public async Task<ActionResult> Deactivate(int id)
        {
            var appointment = context.Appointments.Find(id);
            if (appointment != null)
            {
                ApplyCancellation(appointment, "Doktor tarafından pasife alındı.", "Deactivate");
                await SendCancellationEmail(appointment);
            }

            return RedirectToAction("Index");
        }

        private void ApplyCancellation(Appointment appointment, string reason, string source)
        {
            appointment.Status = "Cancelled";
            appointment.CancellationReason = reason;
            appointment.ModifiedDate = DateTime.Now;
            context.SaveChanges();

            Trace.WriteLine(
                $"Appointment {appointment.AppointmentId} cancelled via {source} at {appointment.ModifiedDate:O}."
            );
        }

        private async Task SendCancellationEmail(Appointment appointment)
        {
            using (var emailService = new Services.EmailService())
            {
                await emailService.SendAppointmentCancellation(appointment);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                context.Dispose();
            base.Dispose(disposing);
        }
    }
}
