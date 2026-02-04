using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CRM.Domain.Entities.Auth
{
    public class ApplicationRole : IdentityRole<string>
    {
        public bool IsSystem { get; set; }
    }
}
