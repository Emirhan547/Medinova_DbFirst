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

            var doctor = context.Doctors.FirstOrDefault(d => d.UserId == userId.Value);
            if (doctor == null)
                return RedirectToAction("Login", "Account", new { area = "Doctor" });


            var today = DateTime.Today;
            var todayAppointments = context.Appointments
                .Where(a => a.DoctorId == doctor.DoctorId &&
                           a.AppointmentDate == today &&
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

            // Upcoming appointments (next 7 days)
            var nextWeek = today.AddDays(7);
            var upcomingCount = context.Appointments
                .Count(a => a.DoctorId == doctor.DoctorId &&
                           a.AppointmentDate > today &&
                           a.AppointmentDate <= nextWeek &&
                           a.Status == "Active");

            ViewBag.UpcomingCount = upcomingCount;

            // Total patients
            var totalPatients = context.Appointments
                .Where(a => a.DoctorId == doctor.DoctorId)
                .Select(a => a.PatientId)
                .Distinct()
                .Count();

            ViewBag.TotalPatients = totalPatients;

            return View(overview);
        }

        public ActionResult Appointments()
        {
            var userId = Session["userId"] as int?;
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });

            var doctor = context.Doctors.FirstOrDefault(d => d.UserId == userId.Value);
            if (doctor == null)
                return RedirectToAction("Login", "Account", new { area = "" });

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
            Trace.WriteLine($"Appointment {appointment.AppointmentId} cancelled via {source} at {appointment.ModifiedDate:O}.");
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
