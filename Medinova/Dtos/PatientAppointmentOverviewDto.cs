using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Dtos
{
    public class PatientAppointmentOverviewDto
    {
        public string PatientName { get; set; }
        public IList<Appointment> ActiveAppointments { get; set; }
        public IList<Appointment> PastAppointments { get; set; }
        public IList<Appointment> PassiveAppointments { get; set; }
    }
}
