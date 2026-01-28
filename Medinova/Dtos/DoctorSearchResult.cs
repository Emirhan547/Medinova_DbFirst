using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Dtos
{
    public class DoctorSearchResult
    {
        public int DoctorId { get; set; }
        public string FullName { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
    }
}