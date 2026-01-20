using Medinova.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Dtos.Defaults
{
    public class DefaultServiceDto
    {
        public IList<Service> Services { get; set; }
    }
}