using CRM.Domain.Entities.Auth;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.Entities
{
    public class Customer : EntityBase
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ApplicationUser? User { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CustomerFeedback> Feedbacks { get; set; } = new List<CustomerFeedback>();
    }
}
