using CRM.Domain.Constants;
using System.ComponentModel.DataAnnotations;

namespace CRM.Application.Services.Vendor_Service
{
    public class VendorVm
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vendor name is required.")]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(20)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email.")]
        [MaxLength(150)]
        public string Email { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Company name is required.")]
        [MaxLength(200)]
        public string CompanyName { get; set; }

        [MaxLength(300)]
        public string? CompanyWebsite { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public string? UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = VendorStatuses.Active;

        public bool IsActive { get; set; } = true;
    }

    public class VendorRegistrationRequestVm
    {
        [Required(ErrorMessage = "Vendor name is required.")]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(20)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email.")]
        [MaxLength(150)]
        public string Email { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Company name is required.")]
        [MaxLength(200)]
        public string CompanyName { get; set; }

        [MaxLength(300)]
        public string? CompanyWebsite { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
