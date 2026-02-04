using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Auth_Service
{
    public class LogOutRequestVM
    {
        public string UserId {  get; set; } 
        public string RefreshToken {  get; set; } 
    }
}
