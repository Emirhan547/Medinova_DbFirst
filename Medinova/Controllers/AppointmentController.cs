using Medinova.Attributes;
using Medinova.Dtos;
using Medinova.Enums;
using Medinova.Models;
using Medinova.Services;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Medinova.Controllers
{
    public class AppointmentController : Controller
    {
        private static readonly ILogger Logger = Log.ForContext<AppointmentController>();
        MedinovaContext context = new MedinovaContext();

        [HttpGet]
        public ActionResult Index()
        {
            // Check if user is logged in
            if (Session["userId"] == null)
            {
                TempData["LoginRequired"] = "Randevu almak için giriş yapmalısınız";
                return RedirectToAction("Login", "Account", new { area = "", returnUrl = Request.RawUrl });
            }

            return View();
        }

        [ChildActionOnly]
        public PartialViewResult DefaultAppointment()
        {
            var departments = context.Departments.ToList();
            ViewBag.departments = departments.Select(d => new SelectListItem
            {
                Text = d.Name,
                Value = d.DepartmentId.ToString()
            }).ToList();

            var dateList = Enumerable.Range(0, 14)
                .Select(i => DateTime.Now.AddDays(i))
                .Select(date => new SelectListItem
                {
                    Text = date.ToString("dd MMMM dddd"),
                    Value = date.ToString("yyyy-MM-dd")
                })
                .ToList();

            ViewBag.dateList = dateList;
            return PartialView();
        }

        [HttpPost]
        [CustomAuthorize("Patient")]
        [ValidateAntiForgeryToken]
        public ActionResult MakeAppointment(Appointment appointment)
        {
            if (Session["userId"] == null)
            {
                Logger.Warning("Appointment attempt rejected due to missing session");
                return Json(new { success = false, message = "Lütfen giriş yapın" });
            }

            var userId = (int)Session["userId"];
            Logger.Information("Appointment request received {UserId} {DoctorId} {AppointmentDate} {AppointmentTime}", userId, appointment.DoctorId, appointment.AppointmentDate, appointment.AppointmentTime);
            // Check if time slot is available
            var existingAppointment = context.Appointments.FirstOrDefault(a =>
                a.DoctorId == appointment.DoctorId &&
                a.AppointmentDate == appointment.AppointmentDate &&
                a.AppointmentTime == appointment.AppointmentTime &&
                a.Status == "Active");

            if (existingAppointment != null)
            {
                Logger.Warning("Appointment slot conflict {UserId} {DoctorId} {AppointmentDate} {AppointmentTime}", userId, appointment.DoctorId, appointment.AppointmentDate, appointment.AppointmentTime);
                return Json(new { success = false, message = "Bu saat dolu!" });
            }

            appointment.PatientId = userId;
            appointment.IsActive = true;
            appointment.Status = "Active";
            appointment.CreatedDate = DateTime.Now;

            context.Appointments.Add(appointment);
            context.SaveChanges();

            // Send confirmation email
            SendAppointmentConfirmationEmail(appointment);

            // Log activity
            LogActivity(userId, "Appointment Created", "Appointment", null);
            Logger.Information("Appointment created {UserId} {AppointmentId} {DoctorId} {AppointmentDate} {AppointmentTime}", userId, appointment.AppointmentId, appointment.DoctorId, appointment.AppointmentDate, appointment.AppointmentTime);

            return RedirectToAction("Index", "Dashboard", new { area = "Patient" });
        }

        [HttpGet]
        public JsonResult GetDoctorsByDepartmentId(int departmentId)
        {
            var doctors = context.Doctors
                .Where(d => d.DepartmentId == departmentId)
                .Select(d => new SelectListItem
                {
                    Text = d.FullName,
                    Value = d.DoctorId.ToString()
                })
                .ToList();

            return Json(doctors, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GetAvailableHours(DateTime selectedDate, int doctorId)
        {
            var bookedTimes = context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate == selectedDate &&
                           a.Status == "Active")
                .Select(a => a.AppointmentTime)
                .ToList();

            var availabilityList = Times.AppointmentHours.Select(hour => new AppointmentAvailabilityDto
            {
                Time = hour,
                IsBooked = bookedTimes.Contains(hour)
            }).ToList();

            return Json(availabilityList, JsonRequestBehavior.AllowGet);
        }

        private void LogActivity(int userId, string action, string controller, string area)
        {
            try
            {
                context.ActivityLogs.Add(new ActivityLog
                {
                    UserId = userId,
                    Action = action,
                    Controller = controller,
                    Area = area,
                    IpAddress = Request.UserHostAddress,
                    LogDate = DateTime.Now
                });
                context.SaveChanges();
            }
            catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                context.Dispose();
            base.Dispose(disposing);
        }
        private async Task SendAppointmentConfirmationEmail(Appointment appointment)
        {
            using (var emailService = new EmailService())
            {
                await emailService.SendAppointmentConfirmation(appointment);
            }
        }

        private async Task SendCancellationEmail(Appointment appointment)
        {
            using (var emailService = new EmailService())
            {
                await emailService.SendAppointmentCancellation(appointment);
            }
        }
    }
}