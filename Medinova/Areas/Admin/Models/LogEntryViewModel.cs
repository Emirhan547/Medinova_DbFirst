using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Areas.Admin.Models
{
    public class LogEntryViewModel
    {
        public string Category { get; set; }
        public string Source { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string UserName { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Status { get; set; }
    }
}