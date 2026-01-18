using Medinova.Dtos;
using Medinova.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace Medinova.Areas.Patient.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {

        public ActionResult Login(string returnUrl)
        {
            return RedirectToAction("Login", "Account", new { area = "", returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginDto model, string returnUrl)
        {
            return RedirectToAction("Login", "Account", new { area = "", returnUrl });
        }

        public ActionResult Logout()
        {
            return RedirectToAction("Logout", "Account", new { area = "" });
        }
    }
}