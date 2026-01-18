using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int TodayAppointments { get; set; }
        public IReadOnlyList<DashboardRecentAppointmentViewModel> RecentAppointments { get; set; }
        public IReadOnlyList<DashboardTopDoctorViewModel> TopDoctors { get; set; }
        public IReadOnlyList<DepartmentDistributionItemViewModel> DepartmentDistribution { get; set; }
        public IReadOnlyList<WeeklyAppointmentStatViewModel> WeeklyAppointments { get; set; }
    }
}