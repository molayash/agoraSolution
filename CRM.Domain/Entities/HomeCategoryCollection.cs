using System;
using System.Collections.Generic;

namespace CRM.Domain.Entities
{
    public class HomeCategoryCollection : EntityBase
    {
        public long ProductCategoryId { get; set; }
        public ProductCategory Category { get; set; }
        public string? CustomTitle { get; set; }
        public int SortOrder { get; set; }
        
        public ICollection<HomeCategoryProduct> HomeCategoryProducts { get; set; } = new List<HomeCategoryProduct>();
    }

    public class HomeCategoryProduct : EntityBase
    {
        public long HomeCategoryCollectionId { get; set; }
        public HomeCategoryCollection HomeCategoryCollection { get; set; }
        
        public long ProductId { get; set; }
        public Product Product { get; set; }
    }
}
