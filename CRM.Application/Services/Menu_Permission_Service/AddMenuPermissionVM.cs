using CRM.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Menu_Permission_Service
{
    public class AddMenuPermissionVM
    {
        public string UserId { get; set; }
        public IList<MenuPermissionVM> Permissions { get; set;}
    }
}
