using CRM.Application.Common.Pagination;
using CRM.Application.Common.Result;
using CRM.Application.DTOs.Product;
using CRM.Application.Interfaces.Medias;
using CRM.Application.Interfaces.Repositories;
using CRM.Application.Services.Work_Context;
using CRM.Domain.Constants;
using CRM.Domain.Entities;
using CRM.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services.Product_Service
{
    public class ProductService : IProductService
    {
        private const string AdminRoleName = "Admin";

        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediaService _mediaService;
        private readonly IPaginationService _paginationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductService(
            IWorkContext workContext,
            IUnitOfWork unitOfWork,
            IPaginationService paginationService,
            IMediaService mediaService,
            UserManager<ApplicationUser> userManager)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _paginationService = paginationService;
            _mediaService = mediaService;
            _userManager = userManager;
        }

        public async Task<ServiceResult> AddRecord(ProductViewModel model, CancellationToken ct)
        {
            var accessContext = await ResolveAccessContextAsync(ct);
            if (accessContext.User == null)
                return ServiceResult.Fail("Unauthorized request.");

            long vendorId;
            try
            {
                vendorId = await ResolveRequestedVendorIdAsync(model.VendorId, accessContext, ct);
            }
            catch (InvalidOperationException ex)
            {
                return ServiceResult.Fail(ex.Message);
            }

            if (vendorId == long.MinValue)
                return ServiceResult.Fail("Only admin or active vendors can manage products.");

            var exists = await ProductNameExistsAsync(model.ProductName, vendorId, null, ct);
            if (exists)
                return ServiceResult.Duplicate("Product already exists for this vendor.");

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
                IsPublish = accessContext.IsAdmin
                    ? vendorId <= 0
                    : model.IsPublish,
                VendorId = vendorId > 0 ? vendorId : null,
                ApprovalStatus = accessContext.IsAdmin
                    ? ProductApprovalStatuses.Normalize(model.ApprovalStatus, ProductApprovalStatuses.Approved)
                    : ProductApprovalStatuses.Pending,
                IsDelete = 0,
                CreatedBy = accessContext.User.FullName,
                CreatedAt = DateTime.UtcNow
            };

            ReplaceProductCollections(product, model);

            await _unitOfWork.Products.AddAsync(product, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return ServiceResult.Ok("Product created successfully.");
        }

        public async Task<ServiceResult> DeleteRecord(long id, CancellationToken ct)
        {
            var accessContext = await ResolveAccessContextAsync(ct);
            if (accessContext.User == null)
                return ServiceResult.Fail("Unauthorized request.");

            var product = await _unitOfWork.Products.Query()
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDelete == 0, ct);

            if (product == null)
                return ServiceResult.NotFound("Product not found.");

            if (!CanManageProduct(accessContext, product))
                return ServiceResult.Fail("You do not have permission to delete this product.");

            product.IsDelete = 1;
            product.UpdatedBy = accessContext.User.FullName;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);
            return ServiceResult.Ok("Product deleted successfully.");
        }

        public async Task<ProductViewModel> GetAll(CancellationToken ct)
        {
            var accessContext = await ResolveAccessContextAsync(ct);
            var query = BuildProductSummaryQuery(accessContext)
                .OrderByDescending(item => item.Id);

            return new ProductViewModel { ProductList = query };
        }

        public async Task<PaginatedResult<ProductViewModel>> GetPagination(PaginationRequest request, CancellationToken ct)
        {
            var accessContext = await ResolveAccessContextAsync(ct);
            var query = BuildProductSummaryQuery(accessContext);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLower();
                query = query.Where(item =>
                    item.ProductName.ToLower().Contains(searchTerm) ||
                    item.ProductCode.ToLower().Contains(searchTerm) ||
                    (item.ProductCategoryName ?? string.Empty).ToLower().Contains(searchTerm) ||
                    (item.ProductSubCategoryName ?? string.Empty).ToLower().Contains(searchTerm) ||
                    (item.BrandName ?? string.Empty).ToLower().Contains(searchTerm) ||
                    (item.VendorName ?? string.Empty).ToLower().Contains(searchTerm) ||
                    (item.VendorCompanyName ?? string.Empty).ToLower().Contains(searchTerm));
            }

            query = query.OrderByDescending(item => item.Id);
            return await _paginationService.PaginateAsync(query, request, ct);
        }

        public async Task<ProductViewModel> GetRecord(long id, CancellationToken ct)
        {
            var accessContext = await ResolveAccessContextAsync(ct);
            var filteredProducts = ApplyAccessFilter(_unitOfWork.Products.Query(), accessContext);

            var productModel = await (from p in filteredProducts
                                      join c in _unitOfWork.ProductCategories.Query() on p.ProductCategoryId equals c.Id
                                      join s in _unitOfWork.ProductSubCategories.Query() on p.ProductSubCategoryId equals s.Id
                                      join b in _unitOfWork.Brands.Query() on p.BrandId equals b.Id into brandGroup
                                      from b in brandGroup.DefaultIfEmpty()
                                      join v in _unitOfWork.Vendors.Query().Where(vendor => vendor.IsDelete == 0) on p.VendorId equals (long?)v.Id into vendorGroup
                                      from v in vendorGroup.DefaultIfEmpty()
                                      where p.Id == id
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
                                          IsPublish = p.IsPublish,
                                          VendorId = p.VendorId,
                                          VendorName = v != null ? v.Name : null,
                                          VendorEmail = v != null ? v.Email : null,
                                          VendorCompanyName = v != null ? v.CompanyName : null,
                                          ApprovalStatus = p.ApprovalStatus,
                                          CreatedBy = p.CreatedBy,
                                          CreatedDate = p.CreatedAt,
                                          ProductImageUrl = p.ProductImages
                                              .Where(x => x.IsDelete == 0 || x.IsDelete == null)
                                              .Select(x => x.ImageUrl)
                                              .FirstOrDefault(),
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
            var accessContext = await ResolveAccessContextAsync(ct);
            if (accessContext.User == null)
                return ServiceResult.Fail("Unauthorized request.");

            var product = await _unitOfWork.Products.Query()
                .Include(x => x.ProductAboutItems)
                .Include(x => x.ProductColors)
                .Include(x => x.ProductImages)
                .Include(x => x.ProductReviews)
                .FirstOrDefaultAsync(x => x.Id == model.Id && x.IsDelete == 0, ct);

            if (product == null)
                return ServiceResult.NotFound("Product not found.");

            if (!CanManageProduct(accessContext, product))
                return ServiceResult.Fail("You do not have permission to update this product.");

            long vendorId;
            try
            {
                vendorId = await ResolveRequestedVendorIdAsync(model.VendorId, accessContext, ct);
            }
            catch (InvalidOperationException ex)
            {
                return ServiceResult.Fail(ex.Message);
            }

            if (vendorId == long.MinValue)
                return ServiceResult.Fail("Only admin or active vendors can manage products.");

            var exists = await ProductNameExistsAsync(model.ProductName, vendorId, model.Id, ct);
            if (exists)
                return ServiceResult.Duplicate("Another product with same name exists for this vendor.");

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
            product.IsPublish = accessContext.IsAdmin
                ? product.IsPublish
                : model.IsPublish;
            product.VendorId = vendorId > 0 ? vendorId : null;
            product.ApprovalStatus = accessContext.IsAdmin
                ? ProductApprovalStatuses.Normalize(model.ApprovalStatus, product.ApprovalStatus)
                : ProductApprovalStatuses.Pending;

            _unitOfWork.ProductAboutItems.RemoveRange(product.ProductAboutItems);
            _unitOfWork.ProductColors.RemoveRange(product.ProductColors);
            _unitOfWork.ProductImages.RemoveRange(product.ProductImages);
            _unitOfWork.ProductReviews.RemoveRange(product.ProductReviews);
            ReplaceProductCollections(product, model);

            product.UpdatedBy = accessContext.User.FullName;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);
            return ServiceResult.Ok(accessContext.IsAdmin
                ? "Product updated successfully."
                : "Product updated and sent for admin approval.");
        }

        public async Task<ServiceResult> UpdateApprovalStatus(long id, UpdateProductApprovalStatusViewModel model, CancellationToken ct)
        {
            var accessContext = await ResolveAccessContextAsync(ct);
            if (!accessContext.IsAdmin || accessContext.User == null)
                return ServiceResult.Fail("Only admins can approve vendor products.");

            var product = await _unitOfWork.Products.Query()
                .FirstOrDefaultAsync(item => item.Id == id && item.IsDelete == 0, ct);

            if (product == null)
                return ServiceResult.NotFound("Product not found.");

            product.ApprovalStatus = ProductApprovalStatuses.Normalize(model.ApprovalStatus, ProductApprovalStatuses.Approved);
            product.UpdatedBy = accessContext.User.FullName;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(ct);
            return ServiceResult.Ok("Product approval updated successfully.");
        }

        private IQueryable<ProductViewModel> BuildProductSummaryQuery(ProductAccessContext accessContext)
        {
            var filteredProducts = ApplyAccessFilter(_unitOfWork.Products.Query(), accessContext);

            return from p in filteredProducts
                   join c in _unitOfWork.ProductCategories.Query() on p.ProductCategoryId equals c.Id
                   join s in _unitOfWork.ProductSubCategories.Query() on p.ProductSubCategoryId equals s.Id
                   join b in _unitOfWork.Brands.Query() on p.BrandId equals b.Id into brandGroup
                   from b in brandGroup.DefaultIfEmpty()
                   join v in _unitOfWork.Vendors.Query().Where(vendor => vendor.IsDelete == 0) on p.VendorId equals (long?)v.Id into vendorGroup
                   from v in vendorGroup.DefaultIfEmpty()
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
                       ProductImageUrl = p.ProductImages
                           .Where(x => x.IsDelete == 0 || x.IsDelete == null)
                           .Select(x => x.ImageUrl)
                           .FirstOrDefault(),
                       UnitPrice = p.UnitPrice,
                       UnitName = p.UnitName,
                       CostingPrice = p.CostingPrice,
                       AVGPrice = p.AVGPrice,
                       MRP = p.MRP,
                       Weight = p.Weight,
                       Rating = p.Rating,
                       StockItems = p.StockItems,
                       IsPublish = p.IsPublish,
                       VendorId = p.VendorId,
                       VendorName = v != null ? v.Name : null,
                       VendorEmail = v != null ? v.Email : null,
                       VendorCompanyName = v != null ? v.CompanyName : null,
                       ApprovalStatus = p.ApprovalStatus,
                       CreatedBy = p.CreatedBy,
                       CreatedDate = p.CreatedAt
                   };
        }

        private IQueryable<Product> ApplyAccessFilter(IQueryable<Product> query, ProductAccessContext accessContext)
        {
            query = query.Where(item => item.IsDelete == 0);

            if (accessContext.IsAdmin)
                return query.Where(item => item.VendorId == null || item.IsPublish);

            if (accessContext.Vendor != null)
                return query.Where(item => item.VendorId == accessContext.Vendor.Id);

            return query.Where(item =>
                item.IsPublish &&
                item.ApprovalStatus == ProductApprovalStatuses.Approved);
        }

        private async Task<ProductAccessContext> ResolveAccessContextAsync(CancellationToken ct)
        {
            var user = await _workContext.CurrentUserAsync();
            if (user == null)
                return new ProductAccessContext();

            var isAdmin = await _userManager.IsInRoleAsync(user, AdminRoleName);
            var vendor = isAdmin
                ? null
                : await _unitOfWork.Vendors.Query()
                    .Where(item => item.IsDelete == 0 && item.IsActive && item.UserId == user.Id)
                    .FirstOrDefaultAsync(ct);

            return new ProductAccessContext
            {
                User = user,
                IsAdmin = isAdmin,
                Vendor = vendor
            };
        }

        private async Task<long> ResolveRequestedVendorIdAsync(long? requestedVendorId, ProductAccessContext accessContext, CancellationToken ct)
        {
            if (accessContext.IsAdmin)
            {
                if (!requestedVendorId.HasValue || requestedVendorId.Value <= 0)
                    return 0;

                var vendorExists = await _unitOfWork.Vendors.AnyAsync(
                    item => item.Id == requestedVendorId.Value && item.IsDelete == 0 && item.IsActive,
                    ct);

                if (!vendorExists)
                    throw new InvalidOperationException("Selected vendor is not active.");

                return requestedVendorId.Value;
            }

            if (accessContext.Vendor != null)
                return accessContext.Vendor.Id;

            return long.MinValue;
        }

        private async Task<bool> ProductNameExistsAsync(string productName, long vendorId, long? excludingProductId, CancellationToken ct)
        {
            var normalizedName = productName.Trim().ToLower();
            return await _unitOfWork.Products.AnyAsync(
                item => item.IsDelete == 0 &&
                        item.ProductName.Trim().ToLower() == normalizedName &&
                        (item.VendorId ?? 0) == vendorId &&
                        (!excludingProductId.HasValue || item.Id != excludingProductId.Value),
                ct);
        }

        private static bool CanManageProduct(ProductAccessContext accessContext, Product product)
        {
            if (accessContext.IsAdmin)
                return true;

            return accessContext.Vendor != null && product.VendorId == accessContext.Vendor.Id;
        }

        private static void ReplaceProductCollections(Product product, ProductViewModel model)
        {
            product.ProductAboutItems = model.ProductAboutItems?
                .Select(item => new ProductAboutItem
                {
                    ProductId = product.Id,
                    AboutItem = string.IsNullOrEmpty(item.AboutItem)
                        ? (string.IsNullOrEmpty(item.Description) ? item.Name : $"{item.Name}: {item.Description}")
                        : item.AboutItem
                })
                .ToList() ?? new List<ProductAboutItem>();

            product.ProductColors = model.ProductColors?
                .Select(item => new ProductColor
                {
                    ProductId = product.Id,
                    Color = string.IsNullOrEmpty(item.Color)
                        ? (string.IsNullOrEmpty(item.ColorCode) ? item.Name : $"{item.Name} ({item.ColorCode})")
                        : item.Color
                })
                .ToList() ?? new List<ProductColor>();

            product.ProductImages = model.ProductImages?
                .Where(item => !string.IsNullOrWhiteSpace(item.ImageUrl))
                .Select(item => new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = item.ImageUrl
                })
                .ToList() ?? new List<ProductImage>();

            product.ProductReviews = model.ProductReviews?
                .Select(item => new ProductReview
                {
                    ProductId = product.Id,
                    UserId = item.UserId,
                    UserName = item.UserName,
                    Rating = item.Rating,
                    Comment = item.Comment,
                    ReviewDate = item.ReviewDate
                })
                .ToList() ?? new List<ProductReview>();
        }

        private sealed class ProductAccessContext
        {
            public ApplicationUser? User { get; init; }
            public bool IsAdmin { get; init; }
            public Vendor? Vendor { get; init; }
        }
    }
}
