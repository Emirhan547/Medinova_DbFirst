using Medinova.Attributes;
using Medinova.Dtos.Profiles;
using Medinova.Models;
using Medinova.Services;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Doctor.Controllers
{
    [CustomAuthorize("Doctor")]
    public class ProfileController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();
        private readonly AwsImageUploadService imageUploadService = new AwsImageUploadService();
        public ActionResult Edit()
        {
            var userId = (int)Session["userId"];
            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
            var doctor = context.Doctors
                 .Include(d => d.Department)
                 .AsEnumerable()
                 .FirstOrDefault(d => d.UserId == userId);

            if (user == null || doctor == null)
                return HttpNotFound();

            var model = new DoctorProfileDto
            {
                DoctorId = doctor.DoctorId,
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                DepartmentName = doctor.Department?.Name,
                Description = doctor.Description,
                ImageUrl = doctor.ImageUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DoctorProfileDto model)
        {
            var userId = (int)Session["userId"];
            var user = context.Users.FirstOrDefault(u => u.UserId == userId);
            var doctor = context.Doctors
               .AsEnumerable()
               .FirstOrDefault(d => d.UserId == userId);

            if (user == null || doctor == null)
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

            doctor.FullName = $"{model.FirstName} {model.LastName}".Trim();
            doctor.Description = model.Description;
            if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
            {
                var awsImageUploadService = new AwsImageUploadService();
                try
                {
                    var uploadedUrl = awsImageUploadService.UploadImage(model.ImageFile, "doctors");
                    if (!string.IsNullOrWhiteSpace(uploadedUrl))
                        doctor.ImageUrl = uploadedUrl;
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Görsel yüklenemedi: {ex.Message}");
                    return View(model);
                }
            }
            else if (!string.IsNullOrWhiteSpace(model.ImageUrl))
            {
                doctor.ImageUrl = model.ImageUrl;
            }

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
                user.Password = model.NewPassword;

            context.SaveChanges();

            Session["fullName"] = $"{user.FirstName} {user.LastName}";
            Session["userName"] = user.UserName;
            Session["profileImageUrl"] = doctor.ImageUrl;
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