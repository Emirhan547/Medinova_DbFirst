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
    public class ServiceController : Controller
    {
        MedinovaContext context=new MedinovaContext();
        public ActionResult Index()
        {
            var service=context.Services.ToList();   
            return View(service);
        }
        [HttpGet]
        public ActionResult CreateService()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateService(Service service)
        {
            context.Services.Add(service);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult UpdateService(int id)
        {
            var service=context.Services.Find(id);
            return View(service);
        }
        [HttpPost]
        public ActionResult UpdateService(Service service)
        {
            var values=context.Services.Find(service.SeviceId);
            values.Title = service.Title;
            values.Description = service.Description;
            values.Icon = service.Icon;
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult DeleteService(int id)
        {
            var service= context.Services.Find(id);
            context.Services.Remove(service);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}