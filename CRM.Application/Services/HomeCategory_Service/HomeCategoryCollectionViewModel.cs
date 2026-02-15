using System.Collections.Generic;

namespace CRM.Application.Services.HomeCategory_Service
{
    public class HomeCategoryCollectionViewModel
    {
        public long Id { get; set; }
        public long ProductCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryImageUrl { get; set; }
        public string? CustomTitle { get; set; }
        public int SortOrder { get; set; }
        public List<HomeCategoryProductViewModel> Products { get; set; } = new List<HomeCategoryProductViewModel>();
    }

    public class HomeCategoryProductViewModel
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal MRP { get; set; }
        public int Discount { get; set; }
        public string ImageUrl { get; set; }
    }

    public class CreateHomeCategoryCollectionDto
    {
        public long ProductCategoryId { get; set; }
        public string? CustomTitle { get; set; }
        public int SortOrder { get; set; }
    }

    public class UpdateHomeCategoryCollectionDto : CreateHomeCategoryCollectionDto
    {
        public long Id { get; set; }
    }

    public class AddProductToCollectionDto
    {
        public long HomeCategoryCollectionId { get; set; }
        public long ProductId { get; set; }
    }
}
