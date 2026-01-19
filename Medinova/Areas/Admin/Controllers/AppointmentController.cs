using Medinova.Attributes;
using Medinova.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [RouteArea("Admin")]
    [CustomAuthorize(Roles = "Admin")]
    public class AppointmentController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        public ActionResult Index(DateTime? startDate, DateTime? endDate, int? doctorId, int? departmentId)
        {
            var appointmentsQuery = context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Doctor.Department)
                .Include(a => a.User)
                .AsQueryable();

            if (startDate.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.AppointmentDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.AppointmentDate <= endDate.Value);
            }

            if (doctorId.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.DoctorId == doctorId.Value);
            }

            if (departmentId.HasValue)
            {
                appointmentsQuery = appointmentsQuery.Where(a => a.Doctor.DepartmentId == departmentId.Value);
            }

            var appointments = appointmentsQuery
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToList();

            var doctors = context.Doctors.OrderBy(d => d.FullName).ToList();
            var departments = context.Departments.OrderBy(d => d.Name).ToList();

            ViewBag.Doctors = new SelectList(doctors, "DoctorId", "FullName", doctorId);
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "Name", departmentId);
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(appointments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id, string reason)
        {
            var appointment = context.Appointments.Find(id);
            if (appointment != null)
            {
                appointment.Status = "Cancelled";
                appointment.IsActive = false;
                appointment.CancellationReason = string.IsNullOrWhiteSpace(reason)
                    ? "Admin tarafından iptal edildi."
                    : reason;
                appointment.ModifiedDate = DateTime.Now;
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(int id)
        {
            var appointment = context.Appointments.Find(id);
            if (appointment != null)
            {
                appointment.Status = "Cancelled";
                appointment.IsActive = false;
                appointment.CancellationReason = "Admin tarafından pasife alındı.";
                appointment.ModifiedDate = DateTime.Now;
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}