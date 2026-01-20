using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Dtos.Defaults
{
    public class DefaultTeamDto
    {
        public IList<Doctor> Doctors { get; set; }
    }
}