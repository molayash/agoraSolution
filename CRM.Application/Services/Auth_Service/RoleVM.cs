using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Auth_Service
{
    public class RoleVM
    {
        public string Id {  get; set; }   
        public string Name {  get; set; }   
        public bool IsSystem {  get; set; }   
    }
}
