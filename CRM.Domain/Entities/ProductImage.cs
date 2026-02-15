using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;
 
namespace CRM.Domain.Entities
{
    public class ProductImage : EntityBase
    {
        public long ProductId { get; set; }
        public string? ImageUrl { get; set; }
        [JsonIgnore]
        public Product? Product { get; set; }
    }
}
