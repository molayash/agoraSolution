using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    public class ProductColor : EntityBase
    {
        public long ProductId { get; set; }
        public string? Color { get; set; }
        
        [NotMapped]
        public string? Name { get; set; }
        
        [NotMapped]
        public string? ColorCode { get; set; }

        // Navigation property
        [JsonIgnore]
        public Product? Product { get; set; }
    }
}
