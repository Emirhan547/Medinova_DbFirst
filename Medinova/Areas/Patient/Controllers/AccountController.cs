using Medinova.Dtos;
using Medinova.Helpers;
using Medinova.Models;
using Serilog;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace Medinova.Areas.Patient.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private static readonly ILogger Logger = Log.ForContext<AccountController>();
        private readonly MedinovaContext context = new MedinovaContext();
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginDto model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            Logger.Information("Patient login attempt for {UserName}", model.UserName);

            var loginResult = AccountLoginService.ValidateCredentials(context, model, "Patient");
            if (!loginResult.IsSuccess)
            {
                Logger.Warning("Patient login failed for {UserName}", model.UserName);
                ModelState.AddModelError("", loginResult.ErrorMessage);
                return View(model);
            }

            var user = loginResult.User;
            FormsAuthentication.SetAuthCookie(user.UserName, false);
            Session["userId"] = user.UserId;
            Session["userName"] = user.UserName;
            Session["fullName"] = user.FirstName + " " + user.LastName;
            Session["userRole"] = loginResult.RoleName;

            Logger.Information("Patient logged in {UserId} {UserName}", user.UserId, user.UserName);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard", new { area = "Patient" });
        }

        public ActionResult Logout()
        {
            Logger.Information("Patient logout {UserId} {UserName}", Session["userId"], Session["userName"]);
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login", "Account", new { area = "Patient" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                context.Dispose();
            base.Dispose(disposing);
        }
    }
}