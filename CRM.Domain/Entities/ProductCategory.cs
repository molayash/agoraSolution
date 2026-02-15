using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities
{
    public class ProductCategory: EntityBase
    {
        public string Name { get; set; }
        public string Remarks { get; set; }
        public int OrderNo { get; set; }
        public string ImageUrl { get; set; }
        public bool IsShow { get; set; }
    }
}
