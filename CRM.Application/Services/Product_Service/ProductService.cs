using CRM.Application.Common.Pagination;
using CRM.Application.Interfaces.Medias;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Product_Service
{
    public class ProductService : IProductService
    {
        private readonly IWorkContext _workContext;
        private readonly CrmDbContext _context;
        private readonly IMediaService _mediaService;
        private readonly IPaginationService _paginationService;

        public ProductService(
            IWorkContext workContext, 
            CrmDbContext context,
            IPaginationService paginationService,
            IMediaService mediaService
            )
        {
            _workContext = workContext;
            _context = context;
            _paginationService = paginationService;
            _mediaService = mediaService;
        }

        public async Task<int> AddRecord(ProductViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();
            var exists = await _context.Product
                .AnyAsync(x => x.ProductName.Trim().ToLower() == model.ProductName.Trim().ToLower() && x.IsDelete == 0, ct);

            if (exists) return 1; // Duplicate

            var product = new Product
            {
                ProductCategoryId = model.ProductCategoryId,
                ProductSubCategoryId = model.ProductSubCategoryId,
                BrandId = model.BrandId,
                ProductCode = model.ProductCode,
                ProductName = model.ProductName,
                ShortName = model.ShortName,
                UnitPrice = model.UnitPrice,
                UnitName = model.UnitName,
                CostingPrice = model.CostingPrice,
                AVGPrice = model.AVGPrice,
                MRP = model.MRP,
                Weight = model.Weight,
                Rating = model.Rating,
                StockItems = model.StockItems,
                IsDelete = 0,
                CreatedBy = user.FullName,
                CreatedAt = DateTime.UtcNow
            };

            // Add Related Collections safely
            if (model.ProductAboutItems != null && model.ProductAboutItems.Any())
            {
                product.ProductAboutItems = new List<ProductAboutItem>();
                foreach (var item in model.ProductAboutItems)
                {
                    item.Id = 0;
                    // Map frontend fields (Name/Description) to AboutItem if AboutItem is null
                    if (string.IsNullOrEmpty(item.AboutItem))
                    {
                        item.AboutItem = string.IsNullOrEmpty(item.Description) ? item.Name : $"{item.Name}: {item.Description}";
                    }
                    product.ProductAboutItems.Add(item);
                }
            }

            if (model.ProductColors != null && model.ProductColors.Any())
            {
                product.ProductColors = new List<ProductColor>();
                foreach (var item in model.ProductColors)
                {
                    item.Id = 0;
                    // Map frontend fields (Name/ColorCode) to Color if Color is null
                    if (string.IsNullOrEmpty(item.Color))
                    {
                        item.Color = string.IsNullOrEmpty(item.ColorCode) ? item.Name : $"{item.Name} ({item.ColorCode})";
                    }
                    product.ProductColors.Add(item);
                }
            }

            if (model.ProductImages != null && model.ProductImages.Any())
            {
                product.ProductImages = new List<ProductImage>();
                foreach (var item in model.ProductImages)
                {
                    item.Id = 0;
                    product.ProductImages.Add(item);
                }
            }

            if (model.ProductReviews != null && model.ProductReviews.Any())
            {
                product.ProductReviews = new List<ProductReview>();
                foreach (var item in model.ProductReviews)
                {
                    item.Id = 0;
                    product.ProductReviews.Add(item);
                }
            }

            await _context.Product.AddAsync(product, ct);
            await _context.SaveChangesAsync(ct);

            return 2; // Success
        }

        public async Task<bool> DeleteRecord(long id, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();
            var product = await _context.Product
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == 0, ct);

            if (product == null) return false;

            product.IsDelete = 1;
            product.UpdatedBy = user.FullName;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<ProductViewModel> GetAll(CancellationToken ct)
        {
            var query = from p in _context.Product.Include(p => p.ProductImages)
                        join c in _context.ProductCategory on p.ProductCategoryId equals c.Id
                        join s in _context.ProductSubCategory on p.ProductSubCategoryId equals s.Id
                        join b in _context.Brands on p.BrandId equals b.Id into brandGroup
                        from b in brandGroup.DefaultIfEmpty()
                        where p.IsDelete == 0
                        select new ProductViewModel
                        {
                            Id = p.Id,
                            ProductCategoryId = p.ProductCategoryId,
                            ProductCategoryName = c.Name,
                            ProductSubCategoryId = p.ProductSubCategoryId,
                            ProductSubCategoryName = s.Name,
                            BrandId = p.BrandId,
                            BrandName = b != null ? b.Name : null,
                            CategoryImageUrl = c.ImageUrl,
                            SubCategoryImageUrl = s.ImageUrl,
                            BrandImageUrl = b != null ? b.ImageUrl : null,
                            ProductCode = p.ProductCode,
                            ProductName = p.ProductName,
                            ShortName = p.ShortName,
                            UnitPrice = p.UnitPrice,
                            UnitName = p.UnitName,
                            CostingPrice = p.CostingPrice,
                            AVGPrice = p.AVGPrice,
                            MRP = p.MRP,
                            Weight = p.Weight,
                            Rating = p.Rating,
                            StockItems = p.StockItems,
                            CreatedBy = p.CreatedBy,
                            CreatedDate = p.CreatedAt,
                            ProductImageUrl=p.ProductImages.Select(i=>i.ImageUrl).FirstOrDefault(),    
                        };

            var model = new ProductViewModel
            {
                ProductList = query.AsQueryable()
            };

            return model;
        }

        public async Task<PaginatedResult<ProductViewModel>> GetPagination(PaginationRequest request, CancellationToken ct)
        {
            var query = from p in _context.Product
                        join c in _context.ProductCategory on p.ProductCategoryId equals c.Id
                        join s in _context.ProductSubCategory on p.ProductSubCategoryId equals s.Id
                        join b in _context.Brands on p.BrandId equals b.Id into brandGroup
                        from b in brandGroup.DefaultIfEmpty()
                        where p.IsDelete == 0
                        orderby p.Id descending
                        select new ProductViewModel
                        {
                            Id = p.Id,
                            ProductCategoryId = p.ProductCategoryId,
                            ProductCategoryName = c.Name,
                            ProductSubCategoryId = p.ProductSubCategoryId,
                            ProductSubCategoryName = s.Name,
                            BrandId = p.BrandId,
                            BrandName = b != null ? b.Name : null,
                            CategoryImageUrl = c.ImageUrl,
                            SubCategoryImageUrl = s.ImageUrl,
                            BrandImageUrl = b != null ? b.ImageUrl : null,
                            ProductCode = p.ProductCode,
                            ProductName = p.ProductName,
                            ShortName = p.ShortName,
                            UnitPrice = p.UnitPrice,
                            UnitName = p.UnitName,
                            CostingPrice = p.CostingPrice,
                            AVGPrice = p.AVGPrice,
                            MRP = p.MRP,
                            Weight = p.Weight,
                            Rating = p.Rating,
                            StockItems = p.StockItems,
                            CreatedBy = p.CreatedBy,
                            CreatedDate = p.CreatedAt
                        };

            return await _paginationService.PaginateAsync(query, request, ct);
        }

        public async Task<ProductViewModel> GetRecord(long id, CancellationToken ct)
        {
            var productModel = await (from p in _context.Product
                                 join c in _context.ProductCategory on p.ProductCategoryId equals c.Id
                                 join s in _context.ProductSubCategory on p.ProductSubCategoryId equals s.Id
                                 join b in _context.Brands on p.BrandId equals b.Id into brandGroup
                                 from b in brandGroup.DefaultIfEmpty()
                                 where p.Id == id && p.IsDelete == 0
                                 select new ProductViewModel
                                 {
                                     Id = p.Id,
                                     ProductCategoryId = p.ProductCategoryId,
                                     ProductCategoryName = c.Name,
                                     ProductSubCategoryId = p.ProductSubCategoryId,
                                     ProductSubCategoryName = s.Name,
                                     BrandId = p.BrandId,
                                     BrandName = b != null ? b.Name : null,
                                     CategoryImageUrl = c.ImageUrl,
                                     SubCategoryImageUrl = s.ImageUrl,
                                     BrandImageUrl = b != null ? b.ImageUrl : null,
                                     ProductCode = p.ProductCode,
                                     ProductName = p.ProductName,
                                     ShortName = p.ShortName,
                                     UnitPrice = p.UnitPrice,
                                     UnitName = p.UnitName,
                                     CostingPrice = p.CostingPrice,
                                     AVGPrice = p.AVGPrice,
                                     MRP = p.MRP,
                                     Weight = p.Weight,
                                     Rating = p.Rating,
                                     StockItems = p.StockItems,
                                     CreatedBy = p.CreatedBy,
                                     CreatedDate = p.CreatedAt,
                                     ProductAboutItems = p.ProductAboutItems.Where(x => x.IsDelete == 0 || x.IsDelete == null).ToList(),
                                     ProductColors = p.ProductColors.Where(x => x.IsDelete == 0 || x.IsDelete == null).ToList(),
                                     ProductImages = p.ProductImages.Where(x => x.IsDelete == 0 || x.IsDelete == null).ToList(),
                                     ProductReviews = p.ProductReviews.Where(x => x.IsDelete == 0 || x.IsDelete == null).ToList()
                                 }).FirstOrDefaultAsync(ct);

            if (productModel != null)
            {
                // Populate unmapped fields for AboutItems
                foreach (var item in productModel.ProductAboutItems)
                {
                    if (string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.AboutItem))
                    {
                        var parts = item.AboutItem.Split(": ");
                        if (parts.Length > 1)
                        {
                            item.Name = parts[0];
                            item.Description = parts[1];
                        }
                        else
                        {
                            item.Name = item.AboutItem;
                        }
                    }
                }

                // Populate unmapped fields for Colors
                foreach (var item in productModel.ProductColors)
                {
                    if (string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.Color))
                    {
                        var parts = item.Color.Split(" (");
                        if (parts.Length > 1)
                        {
                            item.Name = parts[0];
                            item.ColorCode = parts[1].Replace(")", "");
                        }
                        else
                        {
                            item.Name = item.Color;
                        }
                    }
                }
            }

            return productModel;
        }

        public async Task<int> UpdateRecord(ProductViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();
            var product = await _context.Product
                .Include(x => x.ProductAboutItems)
                .Include(x => x.ProductColors)
                .Include(x => x.ProductImages)
                .Include(x => x.ProductReviews)
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0, ct);

            if (product == null) return 0; // Not found

            var exists = await _context.Product
                .AnyAsync(x => x.ProductName.Trim().ToLower() == model.ProductName.Trim().ToLower() && x.Id != model.Id && x.IsDelete == 0, ct);

            if (exists) return 1; // Duplicate

            product.ProductCategoryId = model.ProductCategoryId;
            product.ProductSubCategoryId = model.ProductSubCategoryId;
            product.BrandId = model.BrandId;
            product.ProductCode = model.ProductCode;
            product.ProductName = model.ProductName;
            product.ShortName = model.ShortName;
            product.UnitPrice = model.UnitPrice;
            product.UnitName = model.UnitName;
            product.CostingPrice = model.CostingPrice;
            product.AVGPrice = model.AVGPrice;
            product.MRP = model.MRP;
            product.Weight = model.Weight;
            product.Rating = model.Rating;
            product.StockItems = model.StockItems;
            
            // Update Related Collections
            // ProductAboutItems
            _context.ProductAboutItem.RemoveRange(product.ProductAboutItems);
            if (model.ProductAboutItems != null && model.ProductAboutItems.Any())
            {
                foreach (var item in model.ProductAboutItems)
                {
                    item.Id = 0; // Reset ID for new insertion
                    item.ProductId = product.Id; // Ensure FK is correct
                    if (string.IsNullOrEmpty(item.AboutItem))
                    {
                        item.AboutItem = string.IsNullOrEmpty(item.Description) ? item.Name : $"{item.Name}: {item.Description}";
                    }
                    product.ProductAboutItems.Add(item);
                }
            }

            // ProductColors
            _context.ProductColor.RemoveRange(product.ProductColors);
            if (model.ProductColors != null && model.ProductColors.Any())
            {
                foreach (var item in model.ProductColors)
                {
                    item.Id = 0;
                    item.ProductId = product.Id;
                    if (string.IsNullOrEmpty(item.Color))
                    {
                        item.Color = string.IsNullOrEmpty(item.ColorCode) ? item.Name : $"{item.Name} ({item.ColorCode})";
                    }
                    product.ProductColors.Add(item);
                }
            }

            // ProductImages
            _context.ProductImage.RemoveRange(product.ProductImages);
            if (model.ProductImages != null && model.ProductImages.Any())
            {
                foreach (var item in model.ProductImages)
                {
                    item.Id = 0;
                    item.ProductId = product.Id;
                    product.ProductImages.Add(item);
                }
            }

            // ProductReviews
            _context.ProductReview.RemoveRange(product.ProductReviews);
            if (model.ProductReviews != null && model.ProductReviews.Any())
            {
                foreach (var item in model.ProductReviews)
                {
                    item.Id = 0;
                    item.ProductId = product.Id;
                    product.ProductReviews.Add(item);
                }
            }

            product.UpdatedBy = user.FullName;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);
            return 2; // Success
        }
    }
}
