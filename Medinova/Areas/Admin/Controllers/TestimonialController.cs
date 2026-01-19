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
    public class TestimonialController : Controller
    {
        MedinovaContext context=new MedinovaContext();
        public ActionResult Index()
        {
            var testimonials=context.Testimonials.ToList();
            return View(testimonials);
        }
        [HttpGet]
        public ActionResult DeleteTestimonials(int id)
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
        public ActionResult CreateTestimonial(Testimonial testimonial)
        {
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
        public ActionResult UpdateTestimonial(Testimonial testimonial)
        {
            var values=context.Testimonials.Find(testimonial.TestimonialId);
            values.Comment = testimonial.Comment;
            values.ImageUrl = testimonial.ImageUrl;
            values.FullName = testimonial.FullName;
            values.Profession = testimonial.Profession;
            context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}