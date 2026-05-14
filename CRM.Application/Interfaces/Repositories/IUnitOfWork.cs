using CRM.Domain.Entities;
using CRM.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore.Storage;

namespace CRM.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    // Auth
    IGenericRepository<UserRefreshToken> UserRefreshTokens { get; }
    IGenericRepository<UserModule> UserModules { get; }
    IGenericRepository<ModuleMenu> ModuleMenus { get; }
    IGenericRepository<MenuPermission> MenuPermissions { get; }
    IGenericRepository<MenuRolePermissions> MenuRolePermissions { get; }
    IGenericRepository<DefultMenuRolePermissions> DefaultMenuRolePermissions { get; }

    // Product
    IGenericRepository<Brand> Brands { get; }
    IGenericRepository<ProductCategory> ProductCategories { get; }
    IGenericRepository<ProductSubCategory> ProductSubCategories { get; }
    IGenericRepository<Product> Products { get; }
    IGenericRepository<ProductAboutItem> ProductAboutItems { get; }
    IGenericRepository<ProductColor> ProductColors { get; }
    IGenericRepository<ProductImage> ProductImages { get; }
    IGenericRepository<ProductReview> ProductReviews { get; }

    // Content
    IGenericRepository<Banner> Banners { get; }
    IGenericRepository<HomeCategoryCollection> HomeCategoryCollections { get; }
    IGenericRepository<HomeCategoryProduct> HomeCategoryProducts { get; }
    IGenericRepository<ContactInfo> ContactInfos { get; }
    IGenericRepository<ContactMessage> ContactMessages { get; }

    // Order
    IGenericRepository<Order> Orders { get; }
    IGenericRepository<OrderItem> OrderItems { get; }
    IGenericRepository<OrderVendorForward> OrderVendorForwards { get; }
    IGenericRepository<OrderVendorComment> OrderVendorComments { get; }
    IGenericRepository<CustomerFeedback> CustomerFeedbacks { get; }
    IGenericRepository<VendorDelivered> VendorDelivereds { get; }
    IGenericRepository<VendorDeliveredDetail> VendorDeliveredDetails { get; }

    // Vendor
    IGenericRepository<Vendor> Vendors { get; }
    IGenericRepository<Customer> Customers { get; }

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
