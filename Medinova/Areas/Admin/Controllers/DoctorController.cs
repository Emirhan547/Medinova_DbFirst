using Medinova.Attributes;
using Medinova.Models;
using Medinova.Services;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [RouteArea("Admin")]
    [CustomAuthorize("Admin")]
    public class DoctorController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();
        private readonly AwsImageUploadService imageUploadService = new AwsImageUploadService();
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
        public ActionResult Create(Medinova.Models.Doctor doctor, HttpPostedFileBase ImageFile)
        {
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                doctor.ImageUrl = imageUploadService.UploadImage(ImageFile, "doctors");
            }
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
        public ActionResult Update(Medinova.Models.Doctor doctor, HttpPostedFileBase ImageFile)
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
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                values.ImageUrl = imageUploadService.UploadImage(ImageFile, "doctors");
            }
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