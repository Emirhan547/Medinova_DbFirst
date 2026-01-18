using Medinova.Attributes;
using Medinova.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Doctor.Controllers
{
    [CustomAuthorize("Doctor")]
    public class DashboardController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var userId = (int)Session["userId"];

            // Get doctor info
            var doctor = context.Doctors.FirstOrDefault(d => d.DoctorId == userId);
            if (doctor == null)
                return RedirectToAction("Login", "Account", new { area = "" });

            // Today's appointments
            var today = DateTime.Today;
            var todayAppointments = context.Appointments
                .Where(a => a.DoctorId == doctor.DoctorId &&
                           a.AppointmentDate == today &&
                           a.Status == "Active")
                .OrderBy(a => a.AppointmentTime)
                .ToList();

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

            return View();
        }

        public ActionResult Appointments()
        {
            var userId = (int)Session["userId"];
            var doctor = context.Doctors.FirstOrDefault(d => d.DoctorId == userId);

            var appointments = context.Appointments
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            return View(appointments);
        }

        [HttpPost]
        public ActionResult CancelAppointment(int id, string reason)
        {
            var appointment = context.Appointments.Find(id);
            if (appointment != null)
            {
                appointment.Status = "Cancelled";
                appointment.CancellationReason = reason;
                appointment.ModifiedDate = DateTime.Now;
                context.SaveChanges();

                // Send email notification
                SendCancellationEmail(appointment);
            }

            return RedirectToAction("Appointments");
        }

        private void SendCancellationEmail(Appointment appointment)
        {
            // Email gönderme kodunu ADIM 6'da ekleyeceğiz
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                context.Dispose();
            base.Dispose(disposing);
        }
    }
}