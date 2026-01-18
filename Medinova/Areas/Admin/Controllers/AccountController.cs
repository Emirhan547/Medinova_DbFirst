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
  

        public ActionResult Login()
        {
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        [HttpPost]
        public ActionResult Login(LoginDto model)
        {
          
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
          
            return RedirectToAction("Login", "Account", new { area = "" });
        }
    }
}
