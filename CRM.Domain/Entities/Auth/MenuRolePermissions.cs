using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities.Auth
{
    public class MenuRolePermissions
    {
        [Key]
        public long Id { get; set; }
        [StringLength(450)]
        public  string RoleId { get; set; }
        public  long MenuId { get; set; }
        public ModuleMenu? Menu { get; set; } 
        public bool? CanAdd { get; set; }
        public bool? CanEdit { get; set; }
        public bool? CanDelete { get; set; }
        public bool? CanView { get; set; }
    }
}
