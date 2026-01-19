using Medinova.Areas.Admin.Models;
using Medinova.Attributes;
using Medinova.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [CustomAuthorize("Admin")]
    public class UserController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();

        public ActionResult Index()
        {
            var roles = context.Roles
                .OrderBy(r => r.RoleName)
                .Select(r => new RoleOptionViewModel
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName
                })
                .ToList();

            // 1) DB'den entity olarak çek
            var userEntities = context.Users
                .Include(u => u.UserRoles.Select(ur => ur.Role))
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToList();

            // 2) ViewModel map'i LINQ-to-Objects (memory)
            var users = userEntities
                .Select(u => new UserRoleItemViewModel
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    FullName = ((u.FirstName ?? "") + " " + (u.LastName ?? "")).Trim(),
                    Roles = (u.UserRoles ?? Enumerable.Empty<UserRole>())
                                .Where(ur => ur.Role != null)
                                .Select(ur => ur.Role.RoleName)
                                .ToList(),
                    SelectedRoleId = (u.UserRoles ?? Enumerable.Empty<UserRole>())
                                .Select(ur => (int?)ur.RoleId)
                                .FirstOrDefault()
                })
                .ToList();

            var viewModel = new UserListViewModel
            {
                Users = users,
                Roles = roles
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignRole(int userId, int roleId)
        {
            if (roleId <= 0)
            {
                return RedirectToAction("Index");
            }

            var user = context.Users.Include(u => u.UserRoles).FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return RedirectToAction("Index");
            }

            var role = context.Roles.FirstOrDefault(r => r.RoleId == roleId);
            if (role == null)
            {
                return RedirectToAction("Index");
            }

            var existingRoles = user.UserRoles.ToList();
            if (existingRoles.Any())
            {
                context.UserRoles.RemoveRange(existingRoles);
            }

            context.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = role.RoleId,
                AssignedDate = DateTime.Now
            });

            context.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}