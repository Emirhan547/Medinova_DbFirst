

using Medinova.Attributes;
using Medinova.Models;
using Medinova.Services;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Medinova.Areas.Admin.Controllers
{
    [RouteArea("Admin")]
    [CustomAuthorize("Admin")]
    public class AboutController : Controller
    {
      
        private readonly MedinovaContext context = new MedinovaContext();
        private readonly AwsImageUploadService imageUploadService = new AwsImageUploadService();
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
        public ActionResult CreateAbout(About about, HttpPostedFileBase ImageFile)
        {
            if (ImageFile == null || ImageFile.ContentLength == 0)
            {
                ModelState.AddModelError("", "Görsel zorunludur.");
                return View(about);
            }

            about.ImageUrl = imageUploadService.UploadImage(ImageFile, "abouts");

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
        public ActionResult UpdateAbout(About about, HttpPostedFileBase ImageFile)
        {
            var value = context.Abouts.Find(about.AboutId);
            value.Title= about.Title;
            value.Description=about.Description;
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                try
                {
                    value.ImageUrl = imageUploadService.UploadImage(ImageFile, "abouts");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Görsel yüklenemedi: {ex.Message}");
                    return View(about);
                }
            }
            else
                value.ImageUrl = about.ImageUrl;
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