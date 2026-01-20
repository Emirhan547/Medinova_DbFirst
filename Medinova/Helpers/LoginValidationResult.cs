using Medinova.Dtos;
using Medinova.Models;
using System;
using System.Linq;

namespace Medinova.Helpers
{
    public class LoginValidationResult
    {
        public LoginValidationResult(User user, string roleName)
        {
            User = user;
            RoleName = roleName;
        }

        public LoginValidationResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        public User User { get; }
        public string RoleName { get; }
        public string ErrorMessage { get; }
        public bool IsSuccess => User != null && string.IsNullOrWhiteSpace(ErrorMessage);
    }

    public static class AccountLoginService
    {
        public static LoginValidationResult ValidateCredentials(
            MedinovaContext context,
            LoginDto model,
            string requiredRole = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (model == null)
                return new LoginValidationResult("Kullanıcı adı veya şifre hatalı");

            var user = context.Users.FirstOrDefault(u =>
                u.UserName == model.UserName && u.Password == model.Password);

            if (user == null)
                return new LoginValidationResult("Kullanıcı adı veya şifre hatalı");

            var userRole = context.UserRoles
                .Where(ur => ur.UserId == user.UserId)
                .Select(ur => ur.Role.RoleName)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(userRole))
                return new LoginValidationResult("Kullanıcı rolü bulunamadı");

            if (!string.IsNullOrWhiteSpace(requiredRole)
                && !string.Equals(userRole, requiredRole, StringComparison.OrdinalIgnoreCase))
                return new LoginValidationResult("Bu alana giriş yetkiniz yok");

            return new LoginValidationResult(user, userRole);
        }

       
    }
}