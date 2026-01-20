using Medinova.Attributes;
using Medinova.Dtos;
using Medinova.Dtos.Profiles;
using Medinova.Models;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Patient.Controllers
{
    [CustomAuthorize("Patient")]
    public class ProfileController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        public ActionResult Edit()
        {
            var userId = (int)Session["userId"];
            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return HttpNotFound();

            var model = new PatientProfileDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PatientProfileDto model)
        {
            var userId = (int)Session["userId"];
            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return HttpNotFound();

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (user.Password != model.CurrentPassword)
                    ModelState.AddModelError("CurrentPassword", "Mevcut şifre hatalı.");

                if (model.NewPassword != model.ConfirmPassword)
                    ModelState.AddModelError("ConfirmPassword", "Şifreler eşleşmiyor.");
            }

            if (!ModelState.IsValid)
                return View(model);

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UserName = model.UserName;

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
                user.Password = model.NewPassword;

            context.SaveChanges();

            Session["fullName"] = $"{user.FirstName} {user.LastName}";
            Session["userName"] = user.UserName;

            TempData["Success"] = "Profil bilgileriniz güncellendi.";
            return RedirectToAction("Edit");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                context.Dispose();
            base.Dispose(disposing);
        }
    }
}