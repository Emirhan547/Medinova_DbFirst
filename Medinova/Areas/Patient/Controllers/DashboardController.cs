using Medinova.Attributes;
using Medinova.Dtos;
using Medinova.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Patient.Controllers
{
    [CustomAuthorize("Patient")]
    public class DashboardController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var userId = (int)Session["userId"];

            var model = new PatientAppointmentOverviewDto
            {
                ActiveAppointments = context.Appointments
                    .Where(a => a.PatientId == userId && a.Status == "Active")
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentTime)
                    .ToList(),

                PastAppointments = context.Appointments
                    .Where(a => a.PatientId == userId &&
                               (a.Status == "Completed" ||
                                a.AppointmentDate < DateTime.Today))
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(5)
                    .ToList(),

                PassiveAppointments = context.Appointments
                    .Where(a => a.PatientId == userId && a.Status == "Cancelled")
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToList()
            };

            return View(model);
        }


        public ActionResult MyAppointments()
        {
            var userId = (int)Session["userId"];

            var appointments = context.Appointments
                .Where(a => a.PatientId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToList();

            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelMyAppointment(int id)
        {
            var userId = (int)Session["userId"];
            var appointment = context.Appointments.Find(id);

            if (appointment != null && appointment.PatientId == userId)
            {
                appointment.Status = "Cancelled";
                appointment.CancellationReason = "Cancelled by patient";
                appointment.ModifiedDate = DateTime.Now;
                context.SaveChanges();

                TempData["Success"] = "Randevunuz iptal edildi";
            }

            return RedirectToAction("Index"); // 🔥 KRİTİK SATIR
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                context.Dispose();
            base.Dispose(disposing);
        }
    }
}