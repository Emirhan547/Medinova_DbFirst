using Medinova.Areas.Admin.Models;
using Medinova.Attributes;
using Medinova.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;


namespace Medinova.Areas.Admin.Controllers
{
    [RouteArea("Admin")]
    [CustomAuthorize("Admin")]
    public class LogsController : Controller
    {
        private readonly MedinovaContext context = new MedinovaContext();
        private static readonly ILogger Logger = Log.ForContext<LogsController>();
        public ActionResult Index(DateTime? startDate, DateTime? endDate, int? userId, string category = "All")
        {
            Logger.Information("Log overview requested {StartDate} {EndDate} {UserId} {Category}", startDate, endDate, userId, category);
            LogActivity("Logs Viewed", "Log overview page opened.");

            var activityQuery = context.ActivityLogs.Include(l => l.User).AsQueryable();
            if (startDate.HasValue)
            {
                activityQuery = activityQuery.Where(l => l.LogDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                activityQuery = activityQuery.Where(l => l.LogDate <= endDate.Value);
            }

            if (userId.HasValue)
            {
                activityQuery = activityQuery.Where(l => l.UserId == userId.Value);
            }

            var activityLogs = activityQuery
                .OrderByDescending(l => l.LogDate)
                .Take(750)
                .ToList()
                .Select(l => new LogEntryViewModel
                {
                    Category = ResolveActivityCategory(l),
                    Source = "Activity",
                    Title = l.Action ?? "Activity",
                    Description = BuildActivityDescription(l),
                    UserName = l.User != null ? l.User.UserName : "Sistem",
                    Timestamp = l.LogDate,
                    Status = "OK"
                })
                .ToList();

            var emailQuery = context.EmailLogs.AsQueryable();
            if (startDate.HasValue)
            {
                emailQuery = emailQuery.Where(l => l.SentDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                emailQuery = emailQuery.Where(l => l.SentDate <= endDate.Value);
            }

            var emailLogs = emailQuery
                .OrderByDescending(l => l.SentDate)
                .Take(500)
                .ToList()
                .Select(l => new LogEntryViewModel
                {
                    Category = "Email",
                    Source = "Email",
                    Title = string.IsNullOrWhiteSpace(l.Subject) ? "Email Log" : l.Subject,
                    Description = BuildEmailDescription(l),
                    UserName = l.RecipientEmail,
                    Timestamp = l.SentDate,
                    Status = l.IsSent.HasValue && l.IsSent.Value ? "Sent" : "Failed"
                })
                .ToList();

            var logs = activityLogs.Concat(emailLogs).OrderByDescending(l => l.Timestamp).ToList();

            logs = FilterByCategory(logs, category).ToList();

            var viewModel = new LogsOverviewViewModel
            {
                Logs = logs,
                Categories = BuildCategories(category),
                Users = context.Users.Select(u => new SelectListItem
                {
                    Text = u.UserName,
                    Value = u.UserId.ToString()
                }).ToList(),
                SelectedCategory = category,
                StartDate = startDate,
                EndDate = endDate,
                UserId = userId
            };

            return View(viewModel);
        }
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

        private void LogActivity(string action, string details)
        {
            var userId = GetSessionUserId();
            if (!userId.HasValue)
            {
                return;
            }

            context.ActivityLogs.Add(new ActivityLog
            {
                UserId = userId.Value,
                Action = action,
                Controller = "Logs",
                Area = "Admin",
                IpAddress = Request.UserHostAddress,
                LogDate = DateTime.Now,
                Details = details
            });

            context.SaveChanges();
        }

        private int? GetSessionUserId()
        {
            if (Session == null || Session["userId"] == null)
            {
                return null;
            }

            return int.TryParse(Session["userId"].ToString(), out var userId) ? userId : (int?)null;
        }

        private static string ResolveActivityCategory(ActivityLog log)
        {
            if (!string.IsNullOrWhiteSpace(log.Area) &&
                log.Area.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return "Admin";
            }

            if (string.Equals(log.Controller, "Account", StringComparison.OrdinalIgnoreCase))
            {
                return "Account";
            }

            if (string.Equals(log.Controller, "Appointment", StringComparison.OrdinalIgnoreCase))
            {
                return "Appointment";
            }

            return "Activity";
        }

        private static string BuildActivityDescription(ActivityLog log)
        {
            var details = new List<string>();

            if (!string.IsNullOrWhiteSpace(log.Controller))
            {
                details.Add($"Controller: {log.Controller}");
            }

            if (!string.IsNullOrWhiteSpace(log.Area))
            {
                details.Add($"Area: {log.Area}");
            }

            if (!string.IsNullOrWhiteSpace(log.Details))
            {
                details.Add(log.Details);
            }

            return details.Count > 0 ? string.Join(" · ", details) : "-";
        }

        private static string BuildEmailDescription(EmailLog log)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(log.RecipientEmail))
            {
                parts.Add($"To: {log.RecipientEmail}");
            }

            if (!string.IsNullOrWhiteSpace(log.ErrorMessage))
            {
                parts.Add($"Error: {log.ErrorMessage}");
            }

            return parts.Count > 0 ? string.Join(" · ", parts) : "-";
        }

        private static IEnumerable<LogEntryViewModel> FilterByCategory(IEnumerable<LogEntryViewModel> logs, string category)
        {
            if (string.IsNullOrWhiteSpace(category) || string.Equals(category, "All", StringComparison.OrdinalIgnoreCase))
            {
                return logs;
            }

            if (string.Equals(category, "Activity", StringComparison.OrdinalIgnoreCase))
            {
                return logs.Where(l => l.Source == "Activity");
            }

            if (string.Equals(category, "Email", StringComparison.OrdinalIgnoreCase))
            {
                return logs.Where(l => l.Source == "Email");
            }

            return logs.Where(l => string.Equals(l.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        private static IList<SelectListItem> BuildCategories(string category)
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = "Tümü", Value = "All", Selected = category == "All" },
                new SelectListItem { Text = "Aktivite", Value = "Activity", Selected = category == "Activity" },
                new SelectListItem { Text = "Email", Value = "Email", Selected = category == "Email" },
                new SelectListItem { Text = "Account", Value = "Account", Selected = category == "Account" },
                new SelectListItem { Text = "Appointment", Value = "Appointment", Selected = category == "Appointment" },
                new SelectListItem { Text = "Admin", Value = "Admin", Selected = category == "Admin" }
            };
        }
    }
}