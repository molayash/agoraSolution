using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    public class CustomerDelivered : EntityBase
    {
        [Required]
        public long OrderId { get; set; }

        public Order? Order { get; set; }

        public long? CustomerId { get; set; }

        public Customer? Customer { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShipmentCharge { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal VatAmount { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(50)]
        public string ShipmentStatus { get; set; } = "Pending";

        [MaxLength(100)]
        public string? ShipmentProvider { get; set; }

        [MaxLength(100)]
        public string? TrackingNumber { get; set; }

        [MaxLength(255)]
        public string? ShipmentInfo { get; set; }

        public bool IsFinalized { get; set; }

        public ICollection<CustomerDeliveredDetail> Details { get; set; } = new List<CustomerDeliveredDetail>();
    }
}
