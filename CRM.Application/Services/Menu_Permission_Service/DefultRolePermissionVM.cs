using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Menu_Permission_Service
{
    public class DefultRolePermissionVM
    {
        public string RoleId { get; set; }
        public List<RolePermissionVM> models { get; set; }
    }
}
