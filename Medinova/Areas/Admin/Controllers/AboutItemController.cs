using Medinova.Attributes;
using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [RouteArea("Admin")]
    [CustomAuthorize("Admin")]
    public class AboutItemController : Controller
    {
        MedinovaContext context=new MedinovaContext();
        public ActionResult Index()
        {
            var aboutItems=context.AboutItems.ToList();
            return View(aboutItems);
        }
        [HttpGet]
        public ActionResult CreateAboutItem()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateAboutItem(AboutItem aboutItem)
        {
            context.AboutItems.Add(aboutItem);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult UpdateAboutItem(int id)
        {
            var aboutItems=context.AboutItems.Find(id);
            return View(aboutItems);
        }
        [HttpPost]
        public ActionResult UpdateAboutItems(AboutItem aboutItem)
        {
            var value=context.AboutItems.Find(aboutItem.AboutItemId);
            value.Name = aboutItem.Name;
            value.Icon = aboutItem.Icon;
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult DeleteAboutItem(int id)
        {
            var aboutItems= context.AboutItems.Find(id);
            context.AboutItems.Remove(aboutItems);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}