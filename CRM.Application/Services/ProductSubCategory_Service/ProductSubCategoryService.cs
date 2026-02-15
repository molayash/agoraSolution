using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ProductSubCategory_Service
{
    public class ProductSubCategoryService : IProductSubCategoryService
    {
        private readonly IWorkContext _workContext;
        private readonly CrmDbContext _context;

        public ProductSubCategoryService(IWorkContext workContext, CrmDbContext context)
        {
            _workContext = workContext;
            _context = context;
        }
        public async Task<int> AddRecord(ProductSubCategoryViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();
            var checkname = await _context.ProductSubCategory.FirstOrDefaultAsync(f => f.Name.Trim() == model.Name.Trim() && f.ProductCategoryId == model.ProductCategoryId);
            if (checkname == null)
            {
                ProductSubCategory category = new ProductSubCategory();
                var productCategory = await _context.ProductCategory.FindAsync(model.ProductCategoryId);
                category.Name = model.Name;
                category.ProductCategoryId = model.ProductCategoryId;
                category.ImageUrl = model.ImageUrl;
                category.Remarks = model.Remarks;
                category.OrderNo = model.OrderNo;
                category.IsShow = model.IsShow;
                 await _context.ProductSubCategory.AddAsync(category);
                await _context.SaveChangesAsync();  
                return 2;
            }
            return 1;
        }

        public async Task<bool> DeleteRecord(long id, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();

            var entity = await _context.ProductSubCategory
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (entity == null)
                return false;

            entity.IsDelete = 1;
            entity.CreatedBy = user.FullName;
            entity.CreatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return true;
        }

        public async Task<ProductSubCategoryViewModel> GetAll(CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();
            ProductSubCategoryViewModel model = new ProductSubCategoryViewModel();
            model.ProductSubCategoriesList = await Task.Run(() => (from t1 in _context.ProductSubCategory
                                                                   join t2 in _context.ProductCategory on t1.ProductCategoryId equals t2.Id
                                                                   where t1.IsDelete == 0|| t1.IsDelete ==null
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
                                                                   }).AsQueryable());
            return model;
        }

        public async Task<ProductSubCategoryViewModel> GetProductTypeAndCatagoryWiseList(string type, long id, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();
            ProductSubCategoryViewModel model = new ProductSubCategoryViewModel();
            model.ProductSubCategoriesList = await Task.Run(() => (from t1 in _context.ProductSubCategory
                                                                   where t1.IsDelete == 0 && t1.ProductCategoryId == id
                                                                   select new ProductSubCategoryViewModel
                                                                   {
                                                                       Id = t1.Id,
                                                                       Name = t1.Name,
                                                                   }).AsQueryable());
            return model;
        }

        public async Task<ProductSubCategoryViewModel> GetProductTypeWiseList(long id, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();
            ProductSubCategoryViewModel model = new ProductSubCategoryViewModel();
            model.ProductSubCategoriesList = await Task.Run(() => (from t1 in _context.ProductSubCategory
                                                                   where t1.IsDelete == 0 && t1.ProductCategoryId == id
                                                                   select new ProductSubCategoryViewModel
                                                                   {
                                                                       Id = t1.Id,
                                                                       Name = t1.Name,
                                                                   }).AsQueryable());
            return model;
        }

        public async Task<ProductSubCategoryViewModel> GetRecord(long id, CancellationToken ct)
        {
            var result = await _context.ProductSubCategory.FirstOrDefaultAsync(g=>g.Id==id);
            ProductSubCategoryViewModel model = new ProductSubCategoryViewModel();
            model.Id = result.Id;
            model.Name = result.Name;
            model.ProductCategoryId = result.ProductCategoryId;
            model.ImageUrl = result.ImageUrl;
            model.Remarks = result.Remarks;
            model.OrderNo = result.OrderNo;
            return model;
        }

        public async Task<int> UpdateRecord(ProductSubCategoryViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();
            var checkname = await _context.ProductSubCategory.FirstOrDefaultAsync(f => f.Name.Trim() == model.Name.Trim() && f.ProductCategoryId == model.ProductCategoryId && f.Id != model.Id);
            if (checkname == null)
            {

                ProductSubCategory category = await _context.ProductSubCategory.FirstOrDefaultAsync(d => d.Id == model.Id);
                var productCategory = await _context.ProductCategory.FindAsync(model.ProductCategoryId);
                category.Name = model.Name;
                category.ProductCategoryId = model.ProductCategoryId;
                category.ImageUrl = model.ImageUrl;
                category.Remarks = model.Remarks;
                category.OrderNo = model.OrderNo;
                category.IsShow = model.IsShow;
                 _context.ProductSubCategory.Update(category);
                await _context.SaveChangesAsync();
                return 2;
            }
            return 1;
        }
    }
}
