using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities
{
    public class ContactInfo : EntityBase
    {
        public string Phone1 { get; set; } = string.Empty;
        public string? Phone2 { get; set; }
        public string Email1 { get; set; } = string.Empty;
        public string? Email2 { get; set; }
        public string Website1 { get; set; } = string.Empty;
        public string? Website2 { get; set; }
        public string HeadOffice { get; set; } = string.Empty;
        public string BangladeshOffice { get; set; } = string.Empty;
    }
}
