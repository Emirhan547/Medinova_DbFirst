using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Dtos
{
    public class DoctorAppointmentOverviewDto
    {
        public string DoctorName { get; set; }
        public IList<Appointment> ActiveAppointments { get; set; }
    }
}
