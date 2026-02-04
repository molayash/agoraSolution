using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Menu_Permission_Service
{
    public class AddRolePermissionVM
    {
        public string UserId { get; set; }
        public List<RolePermissionVM> models { get; set;}
    }
}
