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
    public class PlanController : Controller
    {
        MedinovaContext context=new MedinovaContext();
        public ActionResult Index()
        {
            var plans=context.Plans.ToList();
            return View(plans);
        }
        [HttpGet]
        public ActionResult UpdatePlan(int id)
        {
            var plans = context.Plans.Find(id);
            return View(plans);
        }
        [HttpPost]
        public ActionResult UpdatePlan(Plan plan)
        {
            var value = context.Plans.Find(plan.PlanId);
            value.Duration = plan.Duration;
            value.ButtonText = plan.ButtonText;
            value.Feature1 = plan.Feature1;
            value.Feature2 = plan.Feature2;
            value.Feature3 = plan.Feature3;
            value.Feature4 = plan.Feature4;
            value.ImageUrl = plan.ImageUrl;
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult CreatePlan()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreatePlan(Plan plan)
        {
            context.Plans.Add(plan);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult DeletePlan(int id)
        {
            var plans = context.Plans.Find(id);
            context.Plans.Remove(plans);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}