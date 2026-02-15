using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ContactMessage_Service
{
    public class ContactMessageVm
    {
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Message { get; set; } = string.Empty;
        public bool IsSeen { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
