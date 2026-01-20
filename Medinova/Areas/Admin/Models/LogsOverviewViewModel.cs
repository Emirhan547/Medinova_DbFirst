using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Medinova.Areas.Admin.Models
{
    public class LogsOverviewViewModel
    {
        public IList<LogEntryViewModel> Logs { get; set; }
        public IList<SelectListItem> Categories { get; set; }
        public IList<SelectListItem> Users { get; set; }
        public string SelectedCategory { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? UserId { get; set; }
    }
}