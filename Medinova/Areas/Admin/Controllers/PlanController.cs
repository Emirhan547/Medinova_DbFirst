using Medinova.Attributes;
using Medinova.Models;
using Medinova.Services;
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
        private readonly MedinovaContext context = new MedinovaContext();
        private readonly AwsImageUploadService imageUploadService = new AwsImageUploadService();
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
        public ActionResult UpdatePlan(Plan plan, HttpPostedFileBase ImageFile)
        {
            var value = context.Plans.Find(plan.PlanId);
            value.Duration = plan.Duration;
            value.Title = plan.Title;
            value.ButtonText = plan.ButtonText;
            value.Feature1 = plan.Feature1;
            value.Feature2 = plan.Feature2;
            value.Feature3 = plan.Feature3;
            value.Feature4 = plan.Feature4;
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                try
                {
                    value.ImageUrl = imageUploadService.UploadImage(ImageFile, "plans");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Görsel yüklenemedi: {ex.Message}");
                    return View(plan);
                }
            }
            else
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
        public ActionResult CreatePlan(Plan plan, HttpPostedFileBase ImageFile)
        {
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                try
                {
                    plan.ImageUrl = imageUploadService.UploadImage(ImageFile, "plans");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Görsel yüklenemedi: {ex.Message}");
                    return View(plan);
                }
            }
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