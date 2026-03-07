using CRM.Application.Common.Pagination;
using CRM.Application.Common.Result;
using CRM.Application.DTOs.Product;
using CRM.Application.Interfaces.Medias;
using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.Product_Service
{
    public class ProductService : IProductService
    {
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediaService _mediaService;
        private readonly IPaginationService _paginationService;

        public ProductService(
            IWorkContext workContext,
            IUnitOfWork unitOfWork,
            IPaginationService paginationService,
            IMediaService mediaService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _paginationService = paginationService;
            _mediaService = mediaService;
        }

        public async Task<ServiceResult> AddRecord(ProductViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();
            var exists = await _unitOfWork.Products.AnyAsync(
                x => x.ProductName.Trim().ToLower() == model.ProductName.Trim().ToLower() && x.IsDelete == 0, ct);

            if (exists) return ServiceResult.Duplicate("Product already exists.");

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

            if (model.ProductAboutItems != null && model.ProductAboutItems.Any())
            {
                product.ProductAboutItems = model.ProductAboutItems.Select(item => new ProductAboutItem
                {
                    AboutItem = string.IsNullOrEmpty(item.AboutItem)
                        ? (string.IsNullOrEmpty(item.Description) ? item.Name : $"{item.Name}: {item.Description}")
                        : item.AboutItem
                }).ToList();
            }

            if (model.ProductColors != null && model.ProductColors.Any())
            {
                product.ProductColors = model.ProductColors.Select(item => new ProductColor
                {
                    Color = string.IsNullOrEmpty(item.Color)
                        ? (string.IsNullOrEmpty(item.ColorCode) ? item.Name : $"{item.Name} ({item.ColorCode})")
                        : item.Color
                }).ToList();
            }

            if (model.ProductImages != null && model.ProductImages.Any())
            {
                product.ProductImages = model.ProductImages.Select(item => new ProductImage
                {
                    ImageUrl = item.ImageUrl
                }).ToList();
            }

            if (model.ProductReviews != null && model.ProductReviews.Any())
            {
                product.ProductReviews = model.ProductReviews.Select(item => new ProductReview
                {
                    UserId = item.UserId,
                    UserName = item.UserName,
                    Rating = item.Rating,
                    Comment = item.Comment,
                    ReviewDate = item.ReviewDate
                }).ToList();
            }

            await _unitOfWork.Products.AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok("Product created successfully.");
        }

        public async Task<ServiceResult> DeleteRecord(long id, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();
            var product = await _unitOfWork.Products.Query()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == 0, ct);

            if (product == null) return ServiceResult.NotFound("Product not found.");

            product.IsDelete = 1;
            product.UpdatedBy = user.FullName;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);
            return ServiceResult.Ok("Product deleted successfully.");
        }

        public async Task<ProductViewModel> GetAll(CancellationToken ct)
        {
            var query = from p in _unitOfWork.Products.Query().Include(p => p.ProductImages)
                        join c in _unitOfWork.ProductCategories.Query() on p.ProductCategoryId equals c.Id
                        join s in _unitOfWork.ProductSubCategories.Query() on p.ProductSubCategoryId equals s.Id
                        join b in _unitOfWork.Brands.Query() on p.BrandId equals b.Id into brandGroup
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
                            ProductImageUrl = p.ProductImages.Select(i => i.ImageUrl).FirstOrDefault(),
                        };

            return new ProductViewModel { ProductList = query.AsQueryable() };
        }

        public async Task<PaginatedResult<ProductViewModel>> GetPagination(PaginationRequest request, CancellationToken ct)
        {
            var query = from p in _unitOfWork.Products.Query()
                        join c in _unitOfWork.ProductCategories.Query() on p.ProductCategoryId equals c.Id
                        join s in _unitOfWork.ProductSubCategories.Query() on p.ProductSubCategoryId equals s.Id
                        join b in _unitOfWork.Brands.Query() on p.BrandId equals b.Id into brandGroup
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
            var productModel = await (from p in _unitOfWork.Products.Query()
                                      join c in _unitOfWork.ProductCategories.Query() on p.ProductCategoryId equals c.Id
                                      join s in _unitOfWork.ProductSubCategories.Query() on p.ProductSubCategoryId equals s.Id
                                      join b in _unitOfWork.Brands.Query() on p.BrandId equals b.Id into brandGroup
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
                                          ProductAboutItems = p.ProductAboutItems
                                              .Where(x => x.IsDelete == 0 || x.IsDelete == null)
                                              .Select(x => new ProductAboutItemDto { Id = x.Id, ProductId = x.ProductId, AboutItem = x.AboutItem })
                                              .ToList(),
                                          ProductColors = p.ProductColors
                                              .Where(x => x.IsDelete == 0 || x.IsDelete == null)
                                              .Select(x => new ProductColorDto { Id = x.Id, ProductId = x.ProductId, Color = x.Color })
                                              .ToList(),
                                          ProductImages = p.ProductImages
                                              .Where(x => x.IsDelete == 0 || x.IsDelete == null)
                                              .Select(x => new ProductImageDto { Id = x.Id, ProductId = x.ProductId, ImageUrl = x.ImageUrl })
                                              .ToList(),
                                          ProductReviews = p.ProductReviews
                                              .Where(x => x.IsDelete == 0 || x.IsDelete == null)
                                              .Select(x => new ProductReviewDto { Id = x.Id, ProductId = x.ProductId, UserId = x.UserId, UserName = x.UserName, Rating = x.Rating, Comment = x.Comment, ReviewDate = x.ReviewDate })
                                              .ToList()
                                      }).FirstOrDefaultAsync(ct);

            if (productModel != null)
            {
                foreach (var item in productModel.ProductAboutItems)
                {
                    if (string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.AboutItem))
                    {
                        var parts = item.AboutItem.Split(": ");
                        if (parts.Length > 1) { item.Name = parts[0]; item.Description = string.Join(": ", parts.Skip(1)); }
                        else item.Name = item.AboutItem;
                    }
                }

                foreach (var item in productModel.ProductColors)
                {
                    if (string.IsNullOrEmpty(item.Name) && !string.IsNullOrEmpty(item.Color))
                    {
                        var parts = item.Color.Split(" (");
                        if (parts.Length > 1) { item.Name = parts[0]; item.ColorCode = parts[1].TrimEnd(')'); }
                        else item.Name = item.Color;
                    }
                }
            }

            return productModel;
        }

        public async Task<ServiceResult> UpdateRecord(ProductViewModel model, CancellationToken ct)
        {
            var user = await _workContext.GetCurrentUserAsync();
            var product = await _unitOfWork.Products.Query()
                .Include(x => x.ProductAboutItems)
                .Include(x => x.ProductColors)
                .Include(x => x.ProductImages)
                .Include(x => x.ProductReviews)
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0, ct);

            if (product == null) return ServiceResult.NotFound("Product not found.");

            var exists = await _unitOfWork.Products.AnyAsync(
                x => x.ProductName.Trim().ToLower() == model.ProductName.Trim().ToLower()
                  && x.Id != model.Id && x.IsDelete == 0, ct);

            if (exists) return ServiceResult.Duplicate("Another product with same name exists.");

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

            _unitOfWork.ProductAboutItems.RemoveRange(product.ProductAboutItems);
            if (model.ProductAboutItems != null && model.ProductAboutItems.Any())
            {
                product.ProductAboutItems = model.ProductAboutItems.Select(item => new ProductAboutItem
                {
                    ProductId = product.Id,
                    AboutItem = string.IsNullOrEmpty(item.AboutItem)
                        ? (string.IsNullOrEmpty(item.Description) ? item.Name : $"{item.Name}: {item.Description}")
                        : item.AboutItem
                }).ToList();
            }

            _unitOfWork.ProductColors.RemoveRange(product.ProductColors);
            if (model.ProductColors != null && model.ProductColors.Any())
            {
                product.ProductColors = model.ProductColors.Select(item => new ProductColor
                {
                    ProductId = product.Id,
                    Color = string.IsNullOrEmpty(item.Color)
                        ? (string.IsNullOrEmpty(item.ColorCode) ? item.Name : $"{item.Name} ({item.ColorCode})")
                        : item.Color
                }).ToList();
            }

            _unitOfWork.ProductImages.RemoveRange(product.ProductImages);
            if (model.ProductImages != null && model.ProductImages.Any())
            {
                product.ProductImages = model.ProductImages.Select(item => new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = item.ImageUrl
                }).ToList();
            }

            _unitOfWork.ProductReviews.RemoveRange(product.ProductReviews);
            if (model.ProductReviews != null && model.ProductReviews.Any())
            {
                product.ProductReviews = model.ProductReviews.Select(item => new ProductReview
                {
                    ProductId = product.Id,
                    UserId = item.UserId,
                    UserName = item.UserName,
                    Rating = item.Rating,
                    Comment = item.Comment,
                    ReviewDate = item.ReviewDate
                }).ToList();
            }

            product.UpdatedBy = user.FullName;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);
            return ServiceResult.Ok("Product updated successfully.");
        }
    }
}
