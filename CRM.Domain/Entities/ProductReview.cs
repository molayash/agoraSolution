using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;
 
namespace CRM.Domain.Entities
{
    public class ProductReview : EntityBase
    {
        public long ProductId { get; set; }
        public string? UserId { get; set; } // Nullable because in your table it's optional
        public string? UserName { get; set; }
        public decimal? Rating { get; set; } // Nullable in case rating not provided
        public string? Comment { get; set; }
        public DateTime? ReviewDate { get; set; }
 
  
        [JsonIgnore]
        public Product? Product { get; set; }
    }
}
