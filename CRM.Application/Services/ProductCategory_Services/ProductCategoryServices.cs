using CRM.Application.Common.Result;
using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.ProductCategory_Services
{
    public class ProductCategoryServices : IProductCategoryServices
    {
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;

        public ProductCategoryServices(IWorkContext workContext, IUnitOfWork unitOfWork)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult> AddRecord(ProductCategoryViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var exists = await _unitOfWork.ProductCategories.AnyAsync(
                x => x.Name.Trim().ToLower() == model.Name.Trim().ToLower() && x.IsDelete == 0, ct);

            if (exists)
                return ServiceResult.Duplicate("Category already exists.");

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

            await _unitOfWork.ProductCategories.AddAsync(category, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok("Category created successfully.");
        }

        public async Task<ProductCategoryViewModel> GetRecord(long id, CancellationToken ct)
        {
            var category = await _unitOfWork.ProductCategories.Query()
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

        public async Task<ServiceResult> UpdateRecord(ProductCategoryViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var category = await _unitOfWork.ProductCategories.Query()
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0, ct);

            if (category == null)
                return ServiceResult.NotFound("Category not found.");

            var duplicate = await _unitOfWork.ProductCategories.AnyAsync(
                x => x.Name.Trim().ToLower() == model.Name.Trim().ToLower()
                  && x.Id != model.Id
                  && x.IsDelete == 0, ct);

            if (duplicate)
                return ServiceResult.Duplicate("Another category with same name exists.");

            category.Name = model.Name.Trim();
            category.Remarks = model.Remarks;
            category.OrderNo = model.OrderNo;
            category.ImageUrl = model.ImageUrl;
            category.IsShow = model.IsShow;
            category.UpdatedBy = user.FullName;
            category.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok("Category updated successfully.");
        }

        public async Task<ServiceResult> DeleteRecord(long id, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var category = await _unitOfWork.ProductCategories.Query()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == 0, ct);

            if (category == null)
                return ServiceResult.NotFound("Category not found.");

            category.IsDelete = 1;
            category.UpdatedBy = user.FullName;
            category.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok("Category deleted successfully.");
        }

        public async Task<ProductCategoryViewModel> GetAllRecord(CancellationToken ct)
        {
            var model = new ProductCategoryViewModel();
            model.ProductCategoriesList = (from t1 in _unitOfWork.ProductCategories.Query()
                                           where t1.IsDelete == 0
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
                                           }).AsQueryable();
            return model;
        }
    }
}
