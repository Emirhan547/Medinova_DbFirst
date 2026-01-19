using Medinova.Attributes;
using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [CustomAuthorize("Admin")]
    public class RoleController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var roles = context.Roles.OrderBy(r => r.RoleName).ToList();
            return View(roles);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Role role)
        {
            if (string.IsNullOrWhiteSpace(role?.RoleName))
            {
                return RedirectToAction("Index");
            }

            var exists = context.Roles.Any(r => r.RoleName == role.RoleName);
            if (!exists)
            {
                context.Roles.Add(role);
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