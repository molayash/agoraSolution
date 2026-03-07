using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Email_Service
{
    public class EmailSettings
    {
        public string Host { get; set; }
        public string From { get; set; }
        public string Sender { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string Aliass { get; set; }
        public string AdminEmail { get; set; }
    }
}
