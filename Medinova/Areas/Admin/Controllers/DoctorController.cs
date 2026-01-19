using Medinova.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
   [CustomAuthorize("Admin")]]
    public class DoctorController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var doctors = context.Doctors.Include(d => d.Department).ToList();
            return View(doctors);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.Departments = new SelectList(context.Departments.ToList(), "DepartmentId", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult Create(Medinova.Models.Doctor doctor)
        {
            context.Doctors.Add(doctor);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            var doctor = context.Doctors.Find(id);
            if (doctor == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Departments = new SelectList(context.Departments.ToList(), "DepartmentId", "Name", doctor.DepartmentId);
            return View(doctor);
        }

        [HttpPost]
        public ActionResult Update(Medinova.Models.Doctor doctor)
        {
            var values = context.Doctors.Find(doctor.DoctorId);
            if (values == null)
            {
                return RedirectToAction("Index");
            }

            values.FullName = doctor.FullName;
            values.ImageUrl = doctor.ImageUrl;
            values.DepartmentId = doctor.DepartmentId;
            values.Description = doctor.Description;
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var doctor = context.Doctors.Find(id);
            if (doctor != null)
            {
                context.Doctors.Remove(doctor);
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}