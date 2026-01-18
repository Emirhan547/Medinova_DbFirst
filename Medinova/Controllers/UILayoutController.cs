using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Controllers
{
    public class UILayoutController : Controller
    {
        // GET: _UILayout
        public ActionResult Index()
        {
            return View();
        }
        [ChildActionOnly]
        public PartialViewResult UILayoutHead()
        {
            return PartialView();
        }
        [ChildActionOnly]
        public PartialViewResult UILayoutTopbar()
        {
            return PartialView();
        }
        [ChildActionOnly]
        public PartialViewResult UILayoutNavbar()
        {
            return PartialView();
        }
        [ChildActionOnly]
        public PartialViewResult UILayoutFooter()
        {
            return PartialView();
        }
        [ChildActionOnly]
        public PartialViewResult UILayoutScript()
        {
            return PartialView();
        }
    }
}