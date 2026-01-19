using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Areas.Admin.Models
{
    public class UserRoleItemViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public IReadOnlyList<string> Roles { get; set; }
        public int? SelectedRoleId { get; set; }
    }
}