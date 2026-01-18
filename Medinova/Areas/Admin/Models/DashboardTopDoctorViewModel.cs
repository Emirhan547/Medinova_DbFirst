using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Areas.Admin.Models
{
    public class DashboardTopDoctorViewModel
    {
        public string DoctorName { get; set; }
        public string DepartmentName { get; set; }
        public string ImageUrl { get; set; }
        public int AppointmentCount { get; set; }
        public int PerformancePercentage { get; set; }
    }
}