using CRM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace CRM.Application.Services.Product_Service
{
    public class ProductViewModel
    {
        public long Id { get; set; }
        
        [Required]
        public long ProductCategoryId { get; set; }
        public string? ProductCategoryName { get; set; }
        
        [Required]
        public long ProductSubCategoryId { get; set; }
        public string? ProductSubCategoryName { get; set; }
        
        public long BrandId { get; set; }
        public string? BrandName { get; set; }
        public string? CategoryImageUrl { get; set; }
        public string? SubCategoryImageUrl { get; set; }
        public string? BrandImageUrl { get; set; }
     
        [Required]
        public string ProductCode { get; set; }
    
        [Required]
        public string ProductName { get; set; }
        public string? ShortName { get; set; }
        public decimal UnitPrice { get; set; }
        public string? UnitName { get; set; }
        public decimal CostingPrice { get; set; }
        public decimal AVGPrice { get; set; }
        public decimal MRP { get; set; }
        public decimal Weight { get; set; }
        public decimal Rating { get; set; }
        public int StockItems { get; set; }
        
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public ICollection<ProductAboutItem> ProductAboutItems { get; set; } = new List<ProductAboutItem>();
        public ICollection<ProductColor> ProductColors { get; set; } = new List<ProductColor>();
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
        public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

        public IQueryable<ProductViewModel>? ProductList { get; set; }
    }
}
