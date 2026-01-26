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

            Logger.Information("{UserName} için giriş denemesi", model.UserName);

            var loginResult = AccountLoginService.ValidateCredentials(context, model);
            if (!loginResult.IsSuccess)
            {
                Logger.Warning("{UserName} için giriş başarısız", model.UserName);
                ModelState.AddModelError("", loginResult.ErrorMessage);
                return View(model);
            }

            var user = loginResult.User;
            var userRole = loginResult.RoleName;

            // 🔐 AUTH COOKIE
            var ticket = new FormsAuthenticationTicket(
                1,
                user.UserName,
                DateTime.Now,
                DateTime.Now.AddHours(8),
                false,
                userRole
            );

            string encryptedTicket = FormsAuthentication.Encrypt(ticket);

            Response.Cookies.Add(new HttpCookie(
                FormsAuthentication.FormsCookieName,
                encryptedTicket
            )
            {
                HttpOnly = true
            });

            Session["userId"] = user.UserId;
            Session["userName"] = user.UserName;
            Session["fullName"] = user.FirstName + " " + user.LastName;
            Session["userRole"] = userRole;

            // ✅ EF6 UYUMLU DÜZELTME
            if (string.Equals(userRole, "Doctor", StringComparison.OrdinalIgnoreCase))
            {
                int userId = user.UserId;

                var doctor = context.Doctors
                    .AsEnumerable() // 🔥 LINQ to Entities HATASI BURADA BİTER
                    .FirstOrDefault(d => d.UserId == userId);

                Session["profileImageUrl"] = doctor?.ImageUrl;
            }
            else
            {
                Session["profileImageUrl"] = user.ImageUrl;
            }

            Logger.Information(
                "Kullanıcı giriş yaptı {UserId} {UserName} rol {UserRole}",
                user.UserId,
                user.UserName,
                userRole
            );

            LogActivity(user.UserId, "Kullanıcı Girişi", "Account", null);

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

            Logger.Information("{UserName} için kayıt denemesi", model.UserName);

            if (context.Users.Any(u => u.UserName == model.UserName))
            {
                ModelState.AddModelError("UserName", "Bu kullanıcı adı zaten kullanılıyor");
                return View(model);
            }

            var user = new User
            {
                UserName = model.UserName,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            context.Users.Add(user);
            context.SaveChanges();

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
