using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities
{
    public class Product : EntityBase
    {
        public long ProductCategoryId { get; set; }
        public ProductCategory Category { get; set; }
        public long ProductSubCategoryId { get; set; }
        public ProductSubCategory SubCategory { get; set; }
        public long BrandId { get; set; }
        public Brand Brand { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ShortName { get; set; }
        public decimal UnitPrice { get; set; }
        public string UnitName { get; set; }
        public decimal CostingPrice { get; set; }
        public decimal AVGPrice { set; get; }
        public decimal MRP { set; get; }
        public decimal Weight { get; set; }
        public decimal Rating { get; set; } = 0;
        public int StockItems { get; set; } = 0;

        public ICollection<ProductAboutItem> ProductAboutItems { get; set; }
        public ICollection<ProductColor> ProductColors { get; set; }
        public ICollection<ProductImage> ProductImages { get; set; }
        public ICollection<ProductReview> ProductReviews { get; set; }
    }
}
