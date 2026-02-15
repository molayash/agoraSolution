using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.ProductCategory_Services
{
    public class ProductCategoryServices : IProductCategoryServices
    {
        private readonly IWorkContext _workContext;
        private readonly CrmDbContext _context;

        public ProductCategoryServices(IWorkContext workContext, CrmDbContext context)
        {
            _workContext = workContext;
            _context = context;
        }

        // ✅ ADD
        public async Task<int> AddRecord(ProductCategoryViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var exists = await _context.ProductCategory
                .AnyAsync(x => x.Name.Trim().ToLower() == model.Name.Trim().ToLower() 
                            && x.IsDelete == 0, ct);

            if (exists)
                return 1; // Duplicate

            var category = new ProductCategory
            {
                Name = model.Name.Trim(),
                Remarks = model.Remarks,
                OrderNo = model.OrderNo,
                IsShow = model.IsShow,
                ImageUrl = model.ImageUrl,
                IsDelete = 0,
                CreatedBy = user.FullName,
                CreatedAt = DateTime.UtcNow
            };

            await _context.ProductCategory.AddAsync(category, ct);
            await _context.SaveChangesAsync(ct);

            return 2; // Success
        }

    

        // ✅ GET BY ID
        public async Task<ProductCategoryViewModel> GetRecord(long id, CancellationToken ct)
        {
            var category = await _context.ProductCategory
                .AsNoTracking()
                .Where(x => x.Id == id && x.IsDelete == 0)
                .Select(x => new ProductCategoryViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Remarks = x.Remarks,
                    OrderNo = x.OrderNo,
                    ImageUrl = x.ImageUrl
                })
                .FirstOrDefaultAsync(ct);

            if (category == null)
                throw new Exception("Category not found");

            return category;
        }

        // ✅ UPDATE
        public async Task<int> UpdateRecord(ProductCategoryViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var category = await _context.ProductCategory
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0, ct);

            if (category == null)
                return 0; // Not found

            var duplicate = await _context.ProductCategory
                .AnyAsync(x => x.Name.Trim().ToLower() == model.Name.Trim().ToLower()
                            && x.Id != model.Id
                            && x.IsDelete == 0, ct);

            if (duplicate)
                return 1; // Duplicate

            category.Name = model.Name.Trim();
            category.Remarks = model.Remarks;
            category.OrderNo = model.OrderNo;
            category.ImageUrl = model.ImageUrl;
            category.UpdatedBy = user.FullName;
            category.IsShow = model.IsShow;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return 2; // Updated
        }

        // ✅ SOFT DELETE
        public async Task<bool> DeleteRecord(long id, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var category = await _context.ProductCategory
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == 0, ct);

            if (category == null)
                return false;

            category.IsDelete = 1;
            category.UpdatedBy = user.FullName;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<ProductCategoryViewModel> GetAllRecord(CancellationToken ct)
        {
            ProductCategoryViewModel model = new ProductCategoryViewModel();
            model.ProductCategoriesList = await Task.Run(() => (from t1 in _context.ProductCategory
                                                                where t1.IsDelete==0
                                                                select new ProductCategoryViewModel
                                                                {
                                                                    Id = t1.Id,
                                                                    Name = t1.Name,
                                                                    Remarks = t1.Remarks,
                                                                    OrderNo = t1.OrderNo,
                                                                    ImageUrl = t1.ImageUrl,
                                                                    CreatedBy = t1.CreatedBy,
                                                                    CreatedOn = (DateTime)t1.CreatedAt,
                                                                    IsShow = t1.IsShow,
                                                                }).AsQueryable());
            return model;
        }
    }
}
