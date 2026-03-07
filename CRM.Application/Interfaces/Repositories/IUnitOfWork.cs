using CRM.Domain.Entities;
using CRM.Domain.Entities.Auth;

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

    // Vendor
    IGenericRepository<Vendor> Vendors { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
