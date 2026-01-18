using Medinova.Dtos;
using Medinova.Helpers;
using Medinova.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace Medinova.Areas.Admin.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginDto model)
        {
            var user = context.Users
               .Include(x => x.UserRoles.Select(ur => ur.Role))
                .FirstOrDefault(x => x.UserName == model.UserName && x.Password == model.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı Adı veya Şifre Hatalı");
                return View(model);
            }

            var roles = user.UserRoles.Select(x => x.Role.RoleName).ToArray();
            AccountAuthHelper.SignIn(Response, user.UserName, roles, false);
            Session["userName"] = user.UserName;
            Session["fullName"] = user.FirstName + " " + user.LastName;
            Session["roles"] = roles;
            Session["role"] = roles.FirstOrDefault();

            return RedirectToRoleHome(roles);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        private ActionResult RedirectToRoleHome(string[] roles)
        {
            if (roles != null && roles.Contains("Admin"))
            {
                return RedirectToAction("Index", "About", new { area = "Admin" });
            }

            if (roles != null && roles.Contains("Doctor"))
            {
                return RedirectToAction("Index", "Home", new { area = "Doctor" });
            }

            if (roles != null && roles.Contains("Patient"))
            {
                return RedirectToAction("Index", "Home", new { area = "Patient" });
            }

            return RedirectToAction("Index", "Default", new { area = "" });
        }
    }
}