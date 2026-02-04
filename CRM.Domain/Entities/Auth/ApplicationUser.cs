using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities.Auth
{
    public class ApplicationUser : IdentityUser<string>
    {
        [Required(ErrorMessage = "FullName is required.")]
        [MaxLength(150)]
        public required string FullName { get; set; }

        [Required][MaxLength(50)] public required string EntryBy { get; set; }

        [Required]
        [Column(TypeName = "datetime")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }

        public string? SubscriberId { get; set; }
    }
}
