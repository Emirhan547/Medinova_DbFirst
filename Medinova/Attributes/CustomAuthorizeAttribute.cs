using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Medinova.Models;

namespace Medinova.Attributes
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _allowedRoles;

        public CustomAuthorizeAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            if (_allowedRoles == null || _allowedRoles.Length == 0)
                return true;

            var username = httpContext.User.Identity.Name;

            using (var context = new MedinovaContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserName == username);
                if (user == null) return false;

                var userRoles = context.UserRoles
                    .Where(ur => ur.UserId == user.UserId)
                    .Select(ur => ur.Role.RoleName)
                    .ToList();

                return userRoles.Any(role => _allowedRoles.Contains(role));
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
            else
            {
                filterContext.Result = new RedirectResult("~/Account/AccessDenied");
            }
        }
    }
}