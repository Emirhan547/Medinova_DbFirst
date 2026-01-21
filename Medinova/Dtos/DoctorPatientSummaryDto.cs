using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Dtos
{
    public class DoctorPatientSummaryDto
    {
        public int PatientId { get; set; }
        public string FullName { get; set; }
        public string ImageUrl { get; set; }
        public string BloodType { get; set; }
        public int? HeightCm { get; set; }
        public int? WeightKg { get; set; }
        public DateTime? LastAppointmentDate { get; set; }
        public int AppointmentCount { get; set; }
    }
}