using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Attributes
{
    public class LogActivityAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                var userId = filterContext.HttpContext.Session["userId"];
                if (userId == null) return;

                var controller = filterContext.RouteData.Values["controller"]?.ToString();
                var action = filterContext.RouteData.Values["action"]?.ToString();
                var area = filterContext.RouteData.DataTokens["area"]?.ToString();

                using (var context = new MedinovaContext())
                {
                    context.ActivityLogs.Add(new ActivityLog
                    {
                        UserId = (int)userId,
                        Action = $"{controller}.{action}",
                        Controller = controller,
                        Area = area,
                        IpAddress = filterContext.HttpContext.Request.UserHostAddress,
                        LogDate = DateTime.Now
                    });
                    context.SaveChanges();
                }
            }
            catch
            {
                // Silent fail
            }

            base.OnActionExecuted(filterContext);
        }
    }

}