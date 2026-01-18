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

            var user = context.Users.FirstOrDefault(u =>
                u.UserName == model.UserName && u.Password == model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı");
                return View(model);
            }

            var userRole = context.UserRoles
                .Where(ur => ur.UserId == user.UserId)
                .Select(ur => ur.Role.RoleName)
                .FirstOrDefault();

            if (userRole != "Patient")
            {
                ModelState.AddModelError("", "Bu giriş sayfası sadece hastalar içindir");
                return View(model);
            }

            FormsAuthentication.SetAuthCookie(user.UserName, false);
            Session["userId"] = user.UserId;
            Session["userName"] = user.UserName;
            Session["fullName"] = user.FirstName + " " + user.LastName;
            Session["userRole"] = userRole;

            LogActivity(user.UserId, "Patient Login", "Account", "Patient");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard", new { area = "Patient" });
        }

        public ActionResult Logout()
        {
            if (Session["userId"] != null)
            {
                LogActivity((int)Session["userId"], "Patient Logout", "Account", "Patient");
            }

            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }

        private void LogActivity(int userId, string action, string controller, string area)
        {
            try
            {
                context.ActivityLogs.Add(new ActivityLog
                {
                    UserId = userId,
                    Action = action,
                    Controller = controller,
                    Area = area,
                    IpAddress = Request.UserHostAddress,
                    LogDate = DateTime.Now
                });
                context.SaveChanges();
            }
            catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                context.Dispose();
            base.Dispose(disposing);
        }
    }
}