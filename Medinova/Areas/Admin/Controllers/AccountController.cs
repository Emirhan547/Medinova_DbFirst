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
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var loginResult = AccountLoginService.ValidateCredentials(context, model, "Admin");
            if (!loginResult.IsSuccess)
            {
                ModelState.AddModelError("", loginResult.ErrorMessage);
                return View(model);
            }

            var user = loginResult.User;
            FormsAuthentication.SetAuthCookie(user.UserName, false);
            Session["userId"] = user.UserId;
            Session["userName"] = user.UserName;
            Session["fullName"] = user.FirstName + " " + user.LastName;
            Session["userRole"] = loginResult.RoleName;

            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login", "Account", new { area = "Admin" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                context.Dispose();
            base.Dispose(disposing);
        }
    }
}
