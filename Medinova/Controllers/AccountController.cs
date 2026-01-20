using Medinova.Dtos;
using Medinova.Helpers;
using Medinova.Models;
using Serilog;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Medinova.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private static readonly ILogger Logger = Log.ForContext<AccountController>();
        MedinovaContext context = new MedinovaContext();

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

            
            Logger.Information("Login attempt for {UserName}", model.UserName);

           
                var loginResult = AccountLoginService.ValidateCredentials(context, model);
            if (!loginResult.IsSuccess)
            {
                Logger.Warning("Login failed for {UserName}", model.UserName);
                ModelState.AddModelError("", loginResult.ErrorMessage);
                return View(model);
            }
           
            var user = loginResult.User;
            var userRole = loginResult.RoleName;

            // 🔐 ROLE BİLGİSİ AUTH COOKIE’YE YAZILIYOR
            var ticket = new FormsAuthenticationTicket(
                1,
                user.UserName,
                DateTime.Now,
                DateTime.Now.AddHours(8),
                false,
                userRole // 👈 ROLE BURADA
            );

            string encryptedTicket = FormsAuthentication.Encrypt(ticket);

            var authCookie = new HttpCookie(
                FormsAuthentication.FormsCookieName,
                encryptedTicket
            )
            {
                HttpOnly = true
            };

            Response.Cookies.Add(authCookie);

            Session["userId"] = user.UserId;
            Session["userName"] = user.UserName;
            Session["fullName"] = user.FirstName + " " + user.LastName;
            Session["userRole"] = userRole;

            Logger.Information("User logged in {UserId} {UserName} with role {UserRole}", user.UserId, user.UserName, userRole);

            // Log activity
            LogActivity(user.UserId, "User Login", "Account", null);

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

            Logger.Information("Registration attempt for {UserName}", model.UserName);

            if (context.Users.Any(u => u.UserName == model.UserName))
            {
                Logger.Warning("Registration blocked for existing username {UserName}", model.UserName);
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

            Logger.Information("User registered {UserId} {UserName}", user.UserId, user.UserName);

            return RedirectToAction("Login");
        }

        public ActionResult Logout()
        {
            if (Session["userId"] != null)
            {
                Logger.Information("User logout {UserId} {UserName}", Session["userId"], Session["userName"]);
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
