using Medinova.Attributes;
using Medinova.Dtos;
using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Doctor.Controllers
{
    [CustomAuthorize("Doctor")]
    public class PatientsController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var userId = (int)Session["userId"];
            var doctor = context.Doctors.FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return HttpNotFound();

            var patients = context.Appointments
                .Where(a => a.DoctorId == doctor.DoctorId && a.PatientId.HasValue)
                .GroupBy(a => a.PatientId)
                .Select(g => new DoctorPatientSummaryDto
                {
                    PatientId = g.Key.Value,
                    FullName = g.Select(a => a.User.FirstName + " " + a.User.LastName).FirstOrDefault(),
                    ImageUrl = g.Select(a => a.User.ImageUrl).FirstOrDefault(),
                    BloodType = g.Select(a => a.User.BloodType).FirstOrDefault(),
                    HeightCm = g.Select(a => a.User.HeightCm).FirstOrDefault(),
                    WeightKg = g.Select(a => a.User.WeightKg).FirstOrDefault(),
                    LastAppointmentDate = g.Max(a => a.AppointmentDate),
                    AppointmentCount = g.Count()
                })
                .OrderByDescending(p => p.LastAppointmentDate)
                .ToList();

            return View(patients);
        }

        public ActionResult Details(int id)
        {
            var userId = (int)Session["userId"];
            var doctor = context.Doctors.FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return HttpNotFound();

            var patient = context.Users.FirstOrDefault(u => u.UserId == id);
            if (patient == null)
                return HttpNotFound();

            var appointments = context.Appointments
                .Where(a => a.DoctorId == doctor.DoctorId && a.PatientId == id)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToList();

            var model = new DoctorPatientDetailDto
            {
                PatientId = patient.UserId,
                FullName = $"{patient.FirstName} {patient.LastName}".Trim(),
                UserName = patient.UserName,
                ImageUrl = patient.ImageUrl,
                BloodType = patient.BloodType,
                HeightCm = patient.HeightCm,
                WeightKg = patient.WeightKg,
                Appointments = appointments
            };

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                context.Dispose();
            base.Dispose(disposing);
        }
    }
}