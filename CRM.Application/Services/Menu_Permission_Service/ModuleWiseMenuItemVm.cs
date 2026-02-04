using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Menu_Permission_Service
{
    public class ModuleWiseMenuItemVm
    {
        public long Id { get; set; }
        public string ModuleName { get; set; }
        public int SortOrder { get; set; }
        public List<RolePermissionVM>? RolePermissions { get; set; }
        public List<ModuleWiseMenuItemVm>? list { get; set; }
    }
}
