using CRM.Application.DTOs.Product;
using System.ComponentModel.DataAnnotations;

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
        public string? ProductImageUrl { get; set; }
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

        public ICollection<ProductAboutItemDto> ProductAboutItems { get; set; } = new List<ProductAboutItemDto>();
        public ICollection<ProductColorDto> ProductColors { get; set; } = new List<ProductColorDto>();
        public ICollection<ProductImageDto> ProductImages { get; set; } = new List<ProductImageDto>();
        public ICollection<ProductReviewDto> ProductReviews { get; set; } = new List<ProductReviewDto>();

        public IQueryable<ProductViewModel>? ProductList { get; set; }
    }
}
