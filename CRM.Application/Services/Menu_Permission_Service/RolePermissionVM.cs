using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Menu_Permission_Service
{
    public class RolePermissionVM
    {
        public long? Id { get; set; }
        public string? RoleId { get; set; }
        public long MenuId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public bool? CanAdd { get; set; }
        public bool? CanEdit { get; set; }
        public bool? CanDelete { get; set; }
        public bool? CanView { get; set; }
    }
}
