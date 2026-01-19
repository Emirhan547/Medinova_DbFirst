using Medinova.Areas.Admin.Models;
using Medinova.Attributes;
using Medinova.Models;
using Medinova.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [CustomAuthorize("Admin")]
    public class DashboardController : Controller
    {
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            using (var context = new MedinovaContext())
            {
                // 🔹 Tarihler (EF6 uyumlu şekilde DIŞARIDA hesaplanır)
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                var startDate = today.AddDays(-6);

                // 🔹 Genel istatistikler
                var totalDoctors = context.Doctors.Count();
                var totalPatients = context.Users.Count();
                var totalAppointments = context.Appointments.Count();

                var todayAppointments = context.Appointments.Count(a =>
                    a.AppointmentDate >= today && a.AppointmentDate < tomorrow);

                // 🔹 Son randevular
                var recentAppointments = context.Appointments
                    .OrderByDescending(a => a.AppointmentDate ?? a.CreatedDate)
                    .Take(5)
                    .Select(a => new DashboardRecentAppointmentViewModel
                    {
                        PatientName = a.FullName
                            ?? (a.User != null
                                ? a.User.FirstName + " " + a.User.LastName
                                : "Bilinmiyor"),
                        DoctorName = a.Doctor != null
                            ? a.Doctor.FullName
                            : "Bilinmiyor",
                        AppointmentDate = a.AppointmentDate,
                        AppointmentTime = a.AppointmentTime,
                        Status = a.Status ?? "Beklemede"
                    })
                    .ToList();

                // 🔹 En yoğun doktorlar
                var topDoctorEntries = context.Doctors
                    .Select(d => new
                    {
                        d.FullName,
                        d.ImageUrl,
                        DepartmentName = d.Department != null ? d.Department.Name : "Genel",
                        AppointmentCount = d.Appointments.Count()
                    })
                    .OrderByDescending(d => d.AppointmentCount)
                    .Take(5)
                    .ToList();

                var maxDoctorAppointments = topDoctorEntries.Any()
                    ? topDoctorEntries.Max(d => d.AppointmentCount)
                    : 0;

                var topDoctors = topDoctorEntries
                    .Select(d => new DashboardTopDoctorViewModel
                    {
                        DoctorName = d.FullName,
                        DepartmentName = d.DepartmentName,
                        ImageUrl = d.ImageUrl,
                        AppointmentCount = d.AppointmentCount,
                        PerformancePercentage = maxDoctorAppointments > 0
                            ? (int)Math.Round(d.AppointmentCount * 100m / maxDoctorAppointments)
                            : 0
                    })
                    .ToList();

                // 🔹 Departman dağılımı
                var departmentStats = context.Appointments
                    .Where(a => a.DoctorId != null && a.Doctor.DepartmentId != null)
                    .GroupBy(a => a.Doctor.Department.Name)
                    .Select(g => new
                    {
                        DepartmentName = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(g => g.Count)
                    .ToList();

                var totalDepartmentAppointments = departmentStats.Sum(x => x.Count);
                var topDepartments = departmentStats.Take(2).ToList();
                var remainingCount = totalDepartmentAppointments - topDepartments.Sum(x => x.Count);

                var departmentDistribution = new List<DepartmentDistributionItemViewModel>();

                foreach (var department in topDepartments)
                {
                    departmentDistribution.Add(new DepartmentDistributionItemViewModel
                    {
                        Name = department.DepartmentName,
                        Percentage = totalDepartmentAppointments > 0
                            ? (int)Math.Round(department.Count * 100m / totalDepartmentAppointments)
                            : 0
                    });
                }

                departmentDistribution.Add(new DepartmentDistributionItemViewModel
                {
                    Name = "Diğer",
                    Percentage = totalDepartmentAppointments > 0
                        ? (int)Math.Round(remainingCount * 100m / totalDepartmentAppointments)
                        : 0
                });

                // 🔹 Haftalık randevu istatistikleri
                var weeklyCounts = context.Appointments
                    .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate < tomorrow)
                    .GroupBy(a => DbFunctions.TruncateTime(a.AppointmentDate))
                    .Select(g => new
                    {
                        Day = g.Key.Value,
                        Count = g.Count()
                    })
                    .ToList()
                    .ToDictionary(x => x.Day.Date, x => x.Count);

                var weeklyAppointments = Enumerable.Range(0, 7)
                    .Select(i =>
                    {
                        var date = startDate.AddDays(i).Date;
                        return new WeeklyAppointmentStatViewModel
                        {
                            Date = date,
                            Label = date.ToString("ddd"),
                            Count = weeklyCounts.TryGetValue(date, out var count) ? count : 0
                        };
                    })
                    .ToList();

                // 🔹 ViewModel
                var viewModel = new DashboardViewModel
                {
                    TotalDoctors = totalDoctors,
                    TotalPatients = totalPatients,
                    TotalAppointments = totalAppointments,
                    TodayAppointments = todayAppointments,
                    RecentAppointments = recentAppointments,
                    TopDoctors = topDoctors,
                    DepartmentDistribution = departmentDistribution,
                    WeeklyAppointments = weeklyAppointments
                };

                return View(viewModel);
            }
        }

        // GET: Admin/Dashboard/MLPredictions
        public ActionResult MLPredictions()
        {
            var predictionService = new PredictionService();
            var predictions = predictionService.PredictNextThreeMonths();

            var monthlyPredictions = predictions
                .GroupBy(p => new { p.PredictionDate.Year, p.PredictionDate.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalPredicted = g.Sum(p => p.PredictedCount),
                    AvgConfidence = g.Average(p => (double)p.ConfidenceScore)
                })
                .ToList();

            return View(monthlyPredictions);
        }
    }
}
