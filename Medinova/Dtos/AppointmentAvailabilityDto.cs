using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Dtos
{
    public class AppointmentAvailabilityDto
    {
        public string Time { get; set; }
        public bool IsBooked { get; set; }
    }
}