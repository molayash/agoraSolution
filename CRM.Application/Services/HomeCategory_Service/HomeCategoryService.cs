using CRM.Domain.Entities;
using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM.Application.Services.HomeCategory_Service
{
    public interface IHomeCategoryService
    {
        Task<List<HomeCategoryCollectionViewModel>> GetHomeCollectionsAsync();
        Task<HomeCategoryCollectionViewModel> GetByIdAsync(long id);
        Task<long> AddCollectionAsync(CreateHomeCategoryCollectionDto dto);
        Task UpdateCollectionAsync(UpdateHomeCategoryCollectionDto dto);
        Task DeleteCollectionAsync(long id);
        Task AddProductToCollectionAsync(AddProductToCollectionDto dto);
        Task RemoveProductFromCollectionAsync(long id);
        Task<List<HomeCategoryProductViewModel>> GetProductsInCollectionAsync(long collectionId);
    }

    public class HomeCategoryService : IHomeCategoryService
    {
        private readonly CrmDbContext _context;

        public HomeCategoryService(CrmDbContext context)
        {
            _context = context;
        }

        public async Task<List<HomeCategoryCollectionViewModel>> GetHomeCollectionsAsync()
        {
            var collections = await _context.HomeCategoryCollections
                .Include(c => c.Category)
                .Include(c => c.HomeCategoryProducts)
                    .ThenInclude(hp => hp.Product)
                        .ThenInclude(p => p.ProductImages)
                .OrderBy(c => c.SortOrder)
                .Select(c => new HomeCategoryCollectionViewModel
                {
                    Id = c.Id,
                    ProductCategoryId = c.ProductCategoryId,
                    CategoryName = c.Category.Name,
                    CategoryImageUrl = c.Category.ImageUrl,
                    CustomTitle = c.CustomTitle,
                    SortOrder = c.SortOrder,
                    Products = c.HomeCategoryProducts.Select(hp => new HomeCategoryProductViewModel
                    {
                        Id = hp.Id,
                        ProductId = hp.ProductId,
                        Name = hp.Product.ProductName,
                        UnitPrice = hp.Product.UnitPrice,
                        MRP = hp.Product.MRP,
                        Discount = hp.Product.MRP > hp.Product.UnitPrice 
                            ? (int)((hp.Product.MRP - hp.Product.UnitPrice) / hp.Product.MRP * 100) 
                            : 0,
                        ImageUrl = hp.Product.ProductImages.OrderBy(i => i.Id).Select(i => i.ImageUrl).FirstOrDefault() ?? ""
                    }).ToList()
                })
                .ToListAsync();

            return collections;
        }

        public async Task<HomeCategoryCollectionViewModel> GetByIdAsync(long id)
        {
             var c = await _context.HomeCategoryCollections
                .Include(c => c.Category)
                .Include(c => c.HomeCategoryProducts)
                    .ThenInclude(hp => hp.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(x => x.Id == id);

             if (c == null) return null;

             return new HomeCategoryCollectionViewModel
             {
                 Id = c.Id,
                 ProductCategoryId = c.ProductCategoryId,
                 CategoryName = c.Category.Name,
                 CategoryImageUrl = c.Category.ImageUrl,
                 CustomTitle = c.CustomTitle,
                 SortOrder = c.SortOrder,
                 Products = c.HomeCategoryProducts.Select(hp => new HomeCategoryProductViewModel
                 {
                     Id = hp.Id,
                     ProductId = hp.ProductId,
                     Name = hp.Product.ProductName,
                     UnitPrice = hp.Product.UnitPrice,
                     MRP = hp.Product.MRP,
                     Discount = hp.Product.MRP > hp.Product.UnitPrice 
                         ? (int)((hp.Product.MRP - hp.Product.UnitPrice) / hp.Product.MRP * 100) 
                         : 0,
                     ImageUrl = hp.Product.ProductImages.OrderBy(i => i.Id).Select(i => i.ImageUrl).FirstOrDefault() ?? ""
                 }).ToList()
             };
        }

        public async Task<long> AddCollectionAsync(CreateHomeCategoryCollectionDto dto)
        {
            var entity = new HomeCategoryCollection
            {
                ProductCategoryId = dto.ProductCategoryId,
                CustomTitle = dto.CustomTitle,
                SortOrder = dto.SortOrder,
                CreatedAt = System.DateTime.Now
            };

            await _context.HomeCategoryCollections.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task UpdateCollectionAsync(UpdateHomeCategoryCollectionDto dto)
        {
            var entity = await _context.HomeCategoryCollections.FindAsync(dto.Id);
            if (entity != null)
            {
                entity.ProductCategoryId = dto.ProductCategoryId;
                entity.CustomTitle = dto.CustomTitle;
                entity.SortOrder = dto.SortOrder;
                entity.UpdatedAt = System.DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteCollectionAsync(long id)
        {
            var entity = await _context.HomeCategoryCollections.FindAsync(id);
            if (entity != null)
            {
                _context.HomeCategoryCollections.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddProductToCollectionAsync(AddProductToCollectionDto dto)
        {
            var exists = await _context.HomeCategoryProducts
                .AnyAsync(x => x.HomeCategoryCollectionId == dto.HomeCategoryCollectionId && x.ProductId == dto.ProductId);

            if (!exists)
            {
                var entity = new HomeCategoryProduct
                {
                    HomeCategoryCollectionId = dto.HomeCategoryCollectionId,
                    ProductId = dto.ProductId,
                    CreatedAt = System.DateTime.Now
                };
                await _context.HomeCategoryProducts.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveProductFromCollectionAsync(long id)
        {
            var entity = await _context.HomeCategoryProducts.FindAsync(id);
            if (entity != null)
            {
                _context.HomeCategoryProducts.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<HomeCategoryProductViewModel>> GetProductsInCollectionAsync(long collectionId)
        {
            return await _context.HomeCategoryProducts
                .Where(hp => hp.HomeCategoryCollectionId == collectionId)
                .Include(hp => hp.Product)
                    .ThenInclude(p => p.ProductImages)
                .Select(hp => new HomeCategoryProductViewModel
                {
                    Id = hp.Id,
                    ProductId = hp.ProductId,
                    Name = hp.Product.ProductName,
                    UnitPrice = hp.Product.UnitPrice,
                    MRP = hp.Product.MRP,
                    Discount = hp.Product.MRP > hp.Product.UnitPrice 
                        ? (int)((hp.Product.MRP - hp.Product.UnitPrice) / hp.Product.MRP * 100) 
                        : 0,
                    ImageUrl = hp.Product.ProductImages.OrderBy(i => i.Id).Select(i => i.ImageUrl).FirstOrDefault() ?? ""
                })
                .ToListAsync();
        }
    }
}
