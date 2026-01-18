

using Medinova.Models;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Medinova.Areas.Admin.Controllers
{
    [RouteArea("Admin")]
    [Authorize(Roles = "Admin")]
    public class AboutController : Controller
    {
        MedinovaContext context= new MedinovaContext();
        public ActionResult Index()
        {
            var abouts=context.Abouts.ToList();
            return View(abouts);
        }
        public ActionResult CreateAbout()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateAbout(About about)
        {
            context.Abouts.Add(about);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult UpdateAbout(int id)
        {
            var abouts = context.Abouts.Find(id);
            return View(abouts);
        }
        [HttpPost]
        public ActionResult UpdateAbout(About about)
        {
            var value = context.Abouts.Find(about.AboutId);
            value.Title= about.Title;
            value.Description=about.Description;
            value.ImageUrl=about.ImageUrl;
            context.SaveChanges();
            return RedirectToAction("Index");

        }
        public ActionResult DeleteAbout(int id)
        {
            var aboutDeletes=context.Abouts.Find(id);
            context.Abouts.Remove(aboutDeletes);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}