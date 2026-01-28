using Medinova.Attributes;
using Medinova.Models;
using Medinova.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [RouteArea("Admin")]
    [CustomAuthorize("Admin")]
    public class BannerController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();
        private readonly AwsImageUploadService imageUploadService = new AwsImageUploadService();
        public ActionResult Index()
        {
            var banners=context.Banners.ToList();
            return View(banners);
        }
        [HttpGet]
        public ActionResult CreateBanner()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateBanner(Banner banner, HttpPostedFileBase ImageFile)
        {
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                try
                {
                    banner.ImageUrl = imageUploadService.UploadImage(ImageFile, "banners");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Görsel yüklenemedi: {ex.Message}");
                    return View(banner);
                }
            }

            context.Banners.Add(banner);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult UpdateBanner(int id)
        {
            var banners= context.Banners.Find(id);
            return View(banners);
        }
        [HttpPost]
        public ActionResult UpdateBanner(Banner banner, HttpPostedFileBase ImageFile)
        {
            var values = context.Banners.Find(banner.BannerId);
            if (values == null)
                return HttpNotFound();

            values.Title = banner.Title;
            values.Description = banner.Description;

            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                try
                {
                    values.ImageUrl = imageUploadService.UploadImage(ImageFile, "banners");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Görsel yüklenemedi: {ex.Message}");
                    return View(values);
                }
            }
 

            context.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DeleteBanner(int id)
        {
           var values= context.Banners.Find(id);
            context.Banners.Remove(values);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}