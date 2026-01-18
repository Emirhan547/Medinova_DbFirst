using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Areas.Admin.Models
{
    public class WeeklyAppointmentStatViewModel
    {
        public DateTime Date { get; set; }
        public string Label { get; set; }
        public int Count { get; set; }
    }
}