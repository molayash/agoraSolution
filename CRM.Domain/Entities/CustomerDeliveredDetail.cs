using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    public class CustomerDeliveredDetail : EntityBase
    {
        [Required]
        public long CustomerDeliveredId { get; set; }

        public CustomerDelivered? CustomerDelivered { get; set; }

        [Required]
        public long ProductId { get; set; }

        public long? VendorId { get; set; }

        public long? VendorDeliveredId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }
    }
}
