using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.Entities
{
    public class OrderVendorForward : EntityBase
    {
        [Required]
        public long OrderId { get; set; }

        public Order? Order { get; set; }

        [Required]
        public long VendorId { get; set; }

        public Vendor? Vendor { get; set; }

        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        public string? ForwardedByUserId { get; set; }

        [MaxLength(250)]
        public string? ForwardedByName { get; set; }

        [MaxLength(150)]
        public string? VendorEmail { get; set; }

        [Required]
        [MaxLength(30)]
        public string FulfillmentStatus { get; set; } = "pending";

        public DateTime? StatusUpdatedAt { get; set; }

        [MaxLength(450)]
        public string? StatusUpdatedByUserId { get; set; }

        [MaxLength(250)]
        public string? StatusUpdatedByName { get; set; }

        public DateTime? AdminLastViewedAt { get; set; }

        public DateTime? VendorLastViewedAt { get; set; }

        public bool IsLocked { get; set; }

        public bool IsSuccess { get; set; }

        [MaxLength(2000)]
        public string? ErrorMessage { get; set; }
    }
}