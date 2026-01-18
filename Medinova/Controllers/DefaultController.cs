using Medinova.Dtos;
using Medinova.Enums;
using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Medinova.Controllers
{
    [AllowAnonymous]
    public class DefaultController : Controller
    {

      

        public ActionResult Index()
        {
            return View();
        }

        [ChildActionOnly]
        public PartialViewResult DefaultAbout()
        {
            return PartialView();
        }
        

        [ChildActionOnly]
        public PartialViewResult DefaultService()
        {
            return PartialView();
        }
        [ChildActionOnly]
        public PartialViewResult DefaultHero()
        {
            return PartialView();
        }

        [ChildActionOnly]
        public PartialViewResult DefaultPlan()
        {
            return PartialView();
        }

        [ChildActionOnly]
        public PartialViewResult DefaultTeam()
        {
            return PartialView();
        }

        [ChildActionOnly]
        public PartialViewResult DefaultSearch()
        {
            return PartialView();
        }

        [ChildActionOnly]
        public PartialViewResult DefaultTestimonial()
        {
            return PartialView();
        }
     
    }
}