using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Medinova.Areas.Admin.Models
{
    public class UserListViewModel
    {
        public IReadOnlyList<UserRoleItemViewModel> Users { get; set; }
        public IReadOnlyList<RoleOptionViewModel> Roles { get; set; }
    }
}