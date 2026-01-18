using Medinova.ML;
using Medinova.Models;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Medinova.Services
{
    public class PredictionService
    {
        private readonly MLContext mlContext;
        private readonly MedinovaContext context;

        public PredictionService()
        {
            mlContext = new MLContext();
            context = new MedinovaContext();
        }

        public List<AppointmentPrediction> PredictNextThreeMonths()
        {
            // Get historical data (last 6 months)
            var startDate = DateTime.Today.AddMonths(-6);
            var historicalData = context.Appointments
                .Where(a => a.AppointmentDate >= startDate && a.Status != "Cancelled")
                .GroupBy(a => a.AppointmentDate)
                .Select(g => new AppointmentData
                {
                    Date = g.Key.Value,
                    Count = g.Count()
                })
                .OrderBy(a => a.Date)
                .ToList();
            if (historicalData.Count < 30)
                return new List<AppointmentPrediction>();

            // Load data
            var dataView = mlContext.Data.LoadFromEnumerable(historicalData);

            // Create forecasting pipeline
            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(AppointmentForecast.ForecastedCounts),
                inputColumnName: nameof(AppointmentData.Count),
                windowSize: 7,
                seriesLength: 90,
                trainSize: historicalData.Count,
                horizon: 90,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: nameof(AppointmentForecast.LowerBound),
                confidenceUpperBoundColumn: nameof(AppointmentForecast.UpperBound));

            // Train model
            var model = forecastingPipeline.Fit(dataView);

            // Make predictions
            var forecastEngine = model.CreateTimeSeriesEngine<AppointmentData, AppointmentForecast>(mlContext);
            var forecast = forecastEngine.Predict();

            // Convert to predictions
            var predictions = new List<AppointmentPrediction>();
            var baseDate = DateTime.Today;

            for (int i = 0; i < 90; i++)
            {
                predictions.Add(new AppointmentPrediction
                {
                    PredictionDate = baseDate.AddDays(i + 1),
                    PredictedCount = (int)Math.Round(forecast.ForecastedCounts[i]),
                    ConfidenceScore = CalculateConfidence(
                        forecast.ForecastedCounts[i],
                        forecast.LowerBound[i],
                        forecast.UpperBound[i]),
                    CreatedDate = DateTime.Now
                });
            }

            // Save to database
            context.AppointmentPredictions.AddRange(predictions);
            context.SaveChanges();

            return predictions;
        }

        private decimal CalculateConfidence(float predicted, float lower, float upper)
        {
            if (upper == lower) return 100;
            var range = upper - lower;
            var margin = Math.Abs(predicted - (lower + upper) / 2);
            return Math.Round((decimal)((1 - (margin / range)) * 100), 2);
        }

        public void Dispose()
        {
            context.Dispose();
        }
       
        }

}