using Medinova.Attributes;
using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [RouteArea("Admin")]
    [CustomAuthorize("Admin")]
    public class DepartmentController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var departments = context.Departments.ToList();
            return View(departments);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Department department)
        {
            context.Departments.Add(department);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            var department = context.Departments.Find(id);
            if (department == null)
            {
                return RedirectToAction("Index");
            }

            return View(department);
        }

        [HttpPost]
        public ActionResult Update(Department department)
        {
            var value = context.Departments.Find(department.DepartmentId);
            if (value == null)
            {
                return RedirectToAction("Index");
            }

            value.Name = department.Name;
            context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var department = context.Departments.Find(id);
            if (department != null)
            {
                context.Departments.Remove(department);
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}