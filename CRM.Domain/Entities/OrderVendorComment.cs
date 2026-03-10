using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.Entities
{
    public class OrderVendorComment : EntityBase
    {
        [Required]
        public long OrderId { get; set; }

        public Order? Order { get; set; }

        [Required]
        public long VendorId { get; set; }

        public Vendor? Vendor { get; set; }

        public long? OrderVendorForwardId { get; set; }

        public OrderVendorForward? OrderVendorForward { get; set; }

        [MaxLength(450)]
        public string? SenderUserId { get; set; }

        [Required]
        [MaxLength(250)]
        public string SenderName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string SenderRole { get; set; } = "admin";

        [Required]
        [MaxLength(4000)]
        public string Message { get; set; } = string.Empty;
    }
}
