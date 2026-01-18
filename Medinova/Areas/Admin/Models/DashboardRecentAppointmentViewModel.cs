using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Areas.Admin.Models
{
    public class DashboardRecentAppointmentViewModel
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string AppointmentTime { get; set; }
        public string Status { get; set; }
    }
}