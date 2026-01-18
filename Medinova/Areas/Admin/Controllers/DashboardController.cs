using Medinova.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            return View();
        }
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