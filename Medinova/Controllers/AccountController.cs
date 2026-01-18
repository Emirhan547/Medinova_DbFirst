using Medinova.Dtos;
using Medinova.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace Medinova.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        // GET: Login
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

            // Get user roles
            var userRole = context.UserRoles
                .Where(ur => ur.UserId == user.UserId)
                .Select(ur => ur.Role.RoleName)
                .FirstOrDefault();

            FormsAuthentication.SetAuthCookie(user.UserName, false);
            Session["userId"] = user.UserId;
            Session["userName"] = user.UserName;
            Session["fullName"] = user.FirstName + " " + user.LastName;
            Session["userRole"] = userRole;

            // Log activity
            LogActivity(user.UserId, "User Login", "Account", null);

            // Redirect based on role
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            switch (userRole)
            {
                case "Admin":
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                case "Doctor":
                    return RedirectToAction("Index", "Dashboard", new { area = "Doctor" });
                case "Patient":
                    return RedirectToAction("Index", "Dashboard", new { area = "Patient" });
                default:
                    return RedirectToAction("Index", "Default");
            }
        }

        // GET: Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (context.Users.Any(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "Bu kullanıcı adı zaten kullanılıyor");
                return View(model);
            }

            var user = new User
            {
                UserName = model.UserName,
                Password = model.Password, // Gerçek projede hash'leyin!
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            context.Users.Add(user);
            context.SaveChanges();

            // Assign Patient role by default
            var patientRole = context.Roles.FirstOrDefault(r => r.RoleName == "Patient");
            if (patientRole != null)
            {
                context.UserRoles.Add(new UserRole
                {
                    UserId = user.UserId,
                    RoleId = patientRole.RoleId
                });
                context.SaveChanges();
            }

            return RedirectToAction("Login");
        }

        public ActionResult Logout()
        {
            if (Session["userId"] != null)
            {
                LogActivity((int)Session["userId"], "User Logout", "Account", null);
            }

            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }

        public ActionResult AccessDenied()
        {
            return View();
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
            catch { /* Silent fail for logging */ }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                context.Dispose();
            base.Dispose(disposing);
        }
    }
}