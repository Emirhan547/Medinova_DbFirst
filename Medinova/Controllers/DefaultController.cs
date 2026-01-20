using Medinova.Dtos;
using Medinova.Dtos.Defaults;
using Medinova.Enums;
using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Data.Entity;


namespace Medinova.Controllers
{
    [AllowAnonymous]
    public class DefaultController : Controller
    {

        private readonly MedinovaContext context = new MedinovaContext();
        public ActionResult Index()
        {
            return View();
        }

        [ChildActionOnly]
        public PartialViewResult DefaultAbout()
        {
            var model = new DefaultAboutDto
            {
                About = context.Abouts
                    .OrderByDescending(about => about.AboutId)
                    .FirstOrDefault(),
                Items = context.AboutItems
                    .OrderBy(item => item.AboutItemId)
                    .ToList()
            };

            return PartialView(model);
        }
        

        [ChildActionOnly]
        public PartialViewResult DefaultService()
        {
            var model = new DefaultServiceDto
            {
                Services = context.Services
                       .OrderBy(service => service.SeviceId)
                       .ToList()
            };

            return PartialView(model);
        }
        [ChildActionOnly]
        public PartialViewResult DefaultHero()
        {
            var model = new DefaultHeroDto
            {
                Banner = context.Banners
                     .OrderByDescending(banner => banner.BannerId)
                     .FirstOrDefault()
            };

            return PartialView(model);
        }

        [ChildActionOnly]
        public PartialViewResult DefaultPlan()
        {
            var model = new DefaultPlanDto
            {
                Plans = context.Plans
                     .OrderBy(plan => plan.PlanId)
                     .ToList()
            };

            return PartialView(model);
        }

        [ChildActionOnly]
        public PartialViewResult DefaultTeam()
        {
            var model = new DefaultTeamDto
            {
                Doctors = context.Doctors
                     .Include(doctor => doctor.Department)
                     .OrderBy(doctor => doctor.DoctorId)
                     .ToList()
            };

            return PartialView(model);
        }

        [ChildActionOnly]
        public PartialViewResult DefaultSearch()
        {
            return PartialView();
        }

        [ChildActionOnly]
        public PartialViewResult DefaultTestimonial()
        {
            var model = new DefaultTestimonialDto
            {
                Testimonials = context.Testimonials
                    .OrderBy(testimonial => testimonial.TestimonialId)
                    .ToList()
            };

            return PartialView(model);
        }
     
    }
}