using Serilog;
using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace Medinova
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            LoggingConfig.ConfigureLogging();
            Log.Information("Uygulama başlatılıyor");

            GlobalFilters.Filters.Add(new AuthorizeAttribute());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            var doctorSearchService = new Services.DoctorSearchService();
            if (doctorSearchService.IsEnabled)
            {
                using (var context = new Models.MedinovaContext())
                {
                    doctorSearchService.EnsureSeeded(context);
                }
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null) return;

            var ticket = FormsAuthentication.Decrypt(authCookie.Value);
            if (ticket == null) return;

            var identity = new GenericIdentity(ticket.Name);

            var roles = string.IsNullOrEmpty(ticket.UserData)
                ? new string[0]
                : ticket.UserData.Split(',');

            Context.User = new GenericPrincipal(identity, roles);
        }
    }
}
