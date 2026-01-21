using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Dtos
{
    public class DoctorPatientDetailDto
    {
        public int PatientId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string ImageUrl { get; set; }
        public string BloodType { get; set; }
        public int? HeightCm { get; set; }
        public int? WeightKg { get; set; }
        public IList<Appointment> Appointments { get; set; }
    }
}