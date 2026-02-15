using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.Entities
{
    public class ProductAboutItem : EntityBase
    {
        public long ProductId { get; set; }
        public string? AboutItem { get; set; }
        
        [NotMapped]
        public string? Name { get; set; }
        
        [NotMapped]
        public string? Description { get; set; }

        // Navigation property
        [JsonIgnore]
        public Product? Product { get; set; }
    }
}
