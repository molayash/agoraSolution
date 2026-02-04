using CRM.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Auth_Service
{
    public class LoginResponseVM
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public LoginUserVM User { get; set; }    
        public DateTime AccessTokenExpiry { get; set; }
    }
}
