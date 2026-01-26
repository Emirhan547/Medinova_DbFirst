using Medinova.Attributes;
using Medinova.Dtos;
using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Doctor.Controllers
{
    [CustomAuthorize("Doctor")]
    public class UserController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var userId = Session["userId"] as int?;
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });

            var doctor = EnsureDoctor(userId.Value);

            if (doctor == null)
                return RedirectToAction("Logout", "Account", new { area = "" });

            var patients = context.Appointments
     .Where(a => a.DoctorId == doctor.DoctorId && a.PatientId.HasValue)
     .AsEnumerable() // 🔥 EF → LINQ to Objects
     .GroupBy(a => a.PatientId)
     .Select(g =>
     {
         var first = g.First();

         return new DoctorPatientSummaryDto
         {
             PatientId = g.Key.Value,
             FullName = (first.User.FirstName + " " + first.User.LastName).Trim(),
             ImageUrl = first.User.ImageUrl,
             BloodType = first.User.BloodType,
             HeightCm = first.User.HeightCm,
             WeightKg = first.User.WeightKg,
             LastAppointmentDate = g.Max(a => a.AppointmentDate),
             AppointmentCount = g.Count()
         };
     })
     .OrderByDescending(p => p.LastAppointmentDate)
     .ToList();


            return View(patients);
        }

        public ActionResult Details(int id)
        {
            var userId = Session["userId"] as int?;
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });
            var doctor = EnsureDoctor(userId.Value);
            if (doctor == null)
                return RedirectToAction("Logout", "Account", new { area = "" });

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
        private Models.Doctor EnsureDoctor(int userId)
        {
            // 🔥 EF6 HATASINI KESİN BİTSATIRİREN
            var doctor = context.Doctors
                .ToList()
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor != null)
                return doctor;

            var user = context.Users.Find(userId);
            if (user == null)
                return null;

            doctor = new Models.Doctor
            {
                UserId = user.UserId,
                FullName = (user.FirstName + " " + user.LastName).Trim()
            };

            context.Doctors.Add(doctor);
            context.SaveChanges();

            return doctor;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                context.Dispose();
            base.Dispose(disposing);
        }
    }
}
