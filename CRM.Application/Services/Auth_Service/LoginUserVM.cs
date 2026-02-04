using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Auth_Service
{
    public class LoginUserVM
    {
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        // Role names array
        public List<string> RoleNames { get; set; } = new();
    }
}
