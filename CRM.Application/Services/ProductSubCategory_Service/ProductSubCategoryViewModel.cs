using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ProductSubCategory_Service
{
    public class ProductSubCategoryViewModel
    {
        public long Id { get; set; }

        public string? Name { get; set; }
        public long ProductCategoryId { get; set; }
        public string? ProductCategoryName { get; set; }
        public string? ProductCategoryImageUrl { get; set; }
        public string? ProductType { get; set; }
        public string? Remarks { get; set; }
        public int OrderNo { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsShow { get; set; }=true;
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public IEnumerable<ProductSubCategoryViewModel>? ProductSubCategoriesList { get; set; }

        public List<ProductSubCategoryViewModel>? PubCList { get; set; }
    }
}
