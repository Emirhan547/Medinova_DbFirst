using System;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Medinova.Helpers
{
    public static class AccountAuthHelper
    {
        public static void SignIn(
            HttpResponseBase response,
            string userName,
            string[] roles,
            bool rememberMe)
        {
            var ticket = new FormsAuthenticationTicket(
                1,
                userName,
                DateTime.Now,
                DateTime.Now.AddHours(8),
                rememberMe,
                string.Join(",", roles),
                FormsAuthentication.FormsCookiePath
            );

            string encryptedTicket = FormsAuthentication.Encrypt(ticket);

            var cookie = new HttpCookie(
                FormsAuthentication.FormsCookieName,
                encryptedTicket
            );

            if (rememberMe)
                cookie.Expires = ticket.Expiration;

            response.Cookies.Add(cookie);
        }
    }
}
