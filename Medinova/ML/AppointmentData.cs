using Microsoft.ML.Data;
using System;

namespace Medinova.ML
{
    public class AppointmentData
    {
        [LoadColumn(0)]
        public DateTime Date { get; set; }

        [LoadColumn(1)]
        public float Count { get; set; }
    }

    public class AppointmentForecast
    {
        [VectorType(90)] // 3 months prediction
        public float[] ForecastedCounts { get; set; }

        [VectorType(90)]
        public float[] LowerBound { get; set; }

        [VectorType(90)]
        public float[] UpperBound { get; set; }
    }
}