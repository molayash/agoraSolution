using CRM.Application.Common.Result;
using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.ProductSubCategory_Service
{
    public class ProductSubCategoryService : IProductSubCategoryService
    {
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;

        public ProductSubCategoryService(IWorkContext workContext, IUnitOfWork unitOfWork)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult> AddRecord(ProductSubCategoryViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var checkname = await _unitOfWork.ProductSubCategories.Query()
                .FirstOrDefaultAsync(f => f.Name.Trim() == model.Name.Trim()
                                       && f.ProductCategoryId == model.ProductCategoryId, ct);

            if (checkname != null)
                return ServiceResult.Duplicate("SubCategory already exists in this category.");

            var category = new ProductSubCategory
            {
                Name = model.Name,
                ProductCategoryId = model.ProductCategoryId,
                ImageUrl = model.ImageUrl,
                Remarks = model.Remarks,
                OrderNo = model.OrderNo,
                IsShow = model.IsShow
            };

            await _unitOfWork.ProductSubCategories.AddAsync(category, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok("SubCategory created successfully.");
        }

        public async Task<ServiceResult> DeleteRecord(long id, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var entity = await _unitOfWork.ProductSubCategories.Query()
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity == null)
                return ServiceResult.NotFound("SubCategory not found.");

            entity.IsDelete = 1;
            entity.CreatedBy = user.FullName;
            entity.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok("SubCategory deleted successfully.");
        }

        public async Task<ProductSubCategoryViewModel> GetAll(CancellationToken ct)
        {
            var model = new ProductSubCategoryViewModel();
            model.ProductSubCategoriesList = (from t1 in _unitOfWork.ProductSubCategories.Query()
                                              join t2 in _unitOfWork.ProductCategories.Query()
                                                  on t1.ProductCategoryId equals t2.Id
                                              where t1.IsDelete == 0 || t1.IsDelete == null
                                              select new ProductSubCategoryViewModel
                                              {
                                                  Id = t1.Id,
                                                  Name = t1.Name,
                                                  ProductCategoryId = t2.Id,
                                                  ProductCategoryName = t2.Name,
                                                  ProductCategoryImageUrl = t2.ImageUrl,
                                                  Remarks = t1.Remarks,
                                                  OrderNo = t1.OrderNo,
                                                  ImageUrl = t1.ImageUrl,
                                                  IsShow = t1.IsShow,
                                              }).AsQueryable();
            return model;
        }

        public async Task<ProductSubCategoryViewModel> GetProductTypeAndCatagoryWiseList(string type, long id, CancellationToken ct)
        {
            var model = new ProductSubCategoryViewModel();
            model.ProductSubCategoriesList = (from t1 in _unitOfWork.ProductSubCategories.Query()
                                              where t1.IsDelete == 0 && t1.ProductCategoryId == id
                                              select new ProductSubCategoryViewModel
                                              {
                                                  Id = t1.Id,
                                                  Name = t1.Name,
                                              }).AsQueryable();
            return model;
        }

        public async Task<ProductSubCategoryViewModel> GetProductTypeWiseList(long id, CancellationToken ct)
        {
            var model = new ProductSubCategoryViewModel();
            model.ProductSubCategoriesList = (from t1 in _unitOfWork.ProductSubCategories.Query()
                                              where t1.IsDelete == 0 && t1.ProductCategoryId == id
                                              select new ProductSubCategoryViewModel
                                              {
                                                  Id = t1.Id,
                                                  Name = t1.Name,
                                              }).AsQueryable();
            return model;
        }

        public async Task<ProductSubCategoryViewModel> GetRecord(long id, CancellationToken ct)
        {
            var result = await _unitOfWork.ProductSubCategories.Query()
                .FirstOrDefaultAsync(g => g.Id == id, ct);

            return new ProductSubCategoryViewModel
            {
                Id = result.Id,
                Name = result.Name,
                ProductCategoryId = result.ProductCategoryId,
                ImageUrl = result.ImageUrl,
                Remarks = result.Remarks,
                OrderNo = result.OrderNo
            };
        }

        public async Task<ServiceResult> UpdateRecord(ProductSubCategoryViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var checkname = await _unitOfWork.ProductSubCategories.Query()
                .FirstOrDefaultAsync(f => f.Name.Trim() == model.Name.Trim()
                                       && f.ProductCategoryId == model.ProductCategoryId
                                       && f.Id != model.Id, ct);

            if (checkname != null)
                return ServiceResult.Duplicate("Another subcategory with same name already exists in this category.");

            var category = await _unitOfWork.ProductSubCategories.Query()
                .FirstOrDefaultAsync(d => d.Id == model.Id, ct);

            if (category == null)
                return ServiceResult.NotFound("SubCategory not found.");

            category.Name = model.Name;
            category.ProductCategoryId = model.ProductCategoryId;
            category.ImageUrl = model.ImageUrl;
            category.Remarks = model.Remarks;
            category.OrderNo = model.OrderNo;
            category.IsShow = model.IsShow;

            _unitOfWork.ProductSubCategories.Update(category);
            await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok("SubCategory updated successfully.");
        }
    }
}
