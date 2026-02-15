using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ContactInfo_Service
{
    public class ContactInfoVm
    {
        public long Id { get; set; }
        [Required]
        public string Phone1 { get; set; } = string.Empty;
        public string? Phone2 { get; set; }
        [Required]
        [EmailAddress]
        public string Email1 { get; set; } = string.Empty;
        public string? Email2 { get; set; }
        [Required]
        public string Website1 { get; set; } = string.Empty;
        public string? Website2 { get; set; }
        [Required]
        public string HeadOffice { get; set; } = string.Empty;
        [Required]
        public string BangladeshOffice { get; set; } = string.Empty;
    }
}
