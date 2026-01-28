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
    public class TestimonialController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();
        private readonly AwsImageUploadService imageUploadService = new AwsImageUploadService();
        public ActionResult Index()
        {
            var testimonials=context.Testimonials.ToList();
            return View(testimonials);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTestimonial(int id)
        {
            var testimonials = context.Testimonials.Find(id);
            context.Testimonials.Remove(testimonials);
            context.SaveChanges();
            return RedirectToAction("Index");

        }
        [HttpGet]
        public ActionResult CreateTestimonial()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateTestimonial(Testimonial testimonial, HttpPostedFileBase ImageFile)
        {
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                try
                {
                    testimonial.ImageUrl = imageUploadService.UploadImage(ImageFile, "testimonials");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Görsel yüklenemedi: {ex.Message}");
                    return View(testimonial);
                }
            }
            context.Testimonials.Add(testimonial);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult UpdateTestimonial(int id)
        {
            var testimonial = context.Testimonials.Find(id);
            return View(testimonial);
        }
        [HttpPost]
        public ActionResult UpdateTestimonial(Testimonial testimonial, HttpPostedFileBase ImageFile)
        {
            var values=context.Testimonials.Find(testimonial.TestimonialId);
            values.Comment = testimonial.Comment;
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                try
                {
                    values.ImageUrl = imageUploadService.UploadImage(ImageFile, "testimonials");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Görsel yüklenemedi: {ex.Message}");
                    return View(testimonial);
                }
            }
            else
                values.ImageUrl = testimonial.ImageUrl;
            values.FullName = testimonial.FullName;
            values.Profession = testimonial.Profession;
            context.SaveChanges();
            return RedirectToAction("Index");
        }
       
    }
}