using CRM.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.Entities
{
    public class Vendor : EntityBase
    {
        [Required, MaxLength(150)]
        public string Name { get; set; }

        [Required, MaxLength(20)]
        public string Phone { get; set; }

        [Required, MaxLength(150)]
        public string Email { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [Required, MaxLength(200)]
        public string CompanyName { get; set; }

        [MaxLength(300)]
        public string? CompanyWebsite { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public string? UserId { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = VendorStatuses.Pending;

        public bool IsActive { get; set; } = false;
    }
}
