using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ProductCategory_Services
{
    public class ProductCategoryViewModel
    {
        public string Name { get; set; }
        public string? Remarks { get; set; }
        public int OrderNo { get; set; } = 0;
        public string? ImageUrl { get; set; }
        public bool IsShow { get; set; }=true;
        public long Id { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public IEnumerable<ProductCategoryViewModel>? ProductCategoriesList { get; set; }
    }
}
