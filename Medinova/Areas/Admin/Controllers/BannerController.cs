using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BannerController : Controller
    {
       MedinovaContext context=new MedinovaContext();
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
        public ActionResult CreateBanner(Banner banner)
        {
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
        public ActionResult UpdateBanner(Banner banner)
        {
            var values=context.Banners.Find(banner.BannerId);
            values.Description = banner.Description;
            values.Title = banner.Title;
            values.ImageUrl = banner.ImageUrl;
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult DeleteBanner(int id)
        {
            context.Banners.Find(id);
            context.SaveChanges();
            return RedirectToAction("Index");
        }

    }
}