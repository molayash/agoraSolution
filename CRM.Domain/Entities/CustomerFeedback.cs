using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.Entities
{
    public class CustomerFeedback : EntityBase
    {
        [Required]
        public long CustomerId { get; set; }

        public Customer? Customer { get; set; }

        public long? OrderId { get; set; }

        public Order? Order { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "new";
    }
}
