using Medinova.Attributes;
using Medinova.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [RouteArea("Admin")]
    [CustomAuthorize("Admin")]
    public class LogsController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();
        private static readonly ILogger Logger = Log.ForContext<LogsController>();
        public ActionResult ActivityLogs(DateTime? startDate, DateTime? endDate, int? userId)
        {
            Logger.Information("Activity logs requested {StartDate} {EndDate} {UserId}", startDate, endDate, userId);
            var query = context.ActivityLogs.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.LogDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.LogDate <= endDate.Value);

            if (userId.HasValue)
                query = query.Where(l => l.UserId == userId.Value);

            var logs = query
                .OrderByDescending(l => l.LogDate)
                .Take(1000)
                .ToList();

            ViewBag.Users = context.Users.Select(u => new SelectListItem
            {
                Text = u.UserName,
                Value = u.UserId.ToString()
            }).ToList();

            return View(logs);
        }

        public ActionResult EmailLogs()
        {
            Logger.Information("Email logs requested");
            var logs = context.EmailLogs
                .OrderByDescending(e => e.SentDate)
                .Take(500)
                .ToList();

            return View(logs);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                context.Dispose();
            base.Dispose(disposing);
        }
    }
}