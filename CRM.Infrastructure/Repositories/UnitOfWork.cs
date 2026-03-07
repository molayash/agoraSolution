using CRM.Application.Interfaces.Repositories;
using CRM.Domain.Entities;
using CRM.Domain.Entities.Auth;
using MenuRolePermissionsEntity = CRM.Domain.Entities.Auth.MenuRolePermissions;
using DefultMenuRolePermissionsEntity = CRM.Domain.Entities.Auth.DefultMenuRolePermissions;

namespace CRM.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CrmDbContext _context;

    public UnitOfWork(CrmDbContext context)
    {
        _context = context;

        UserRefreshTokens = new GenericRepository<UserRefreshToken>(context);
        UserModules = new GenericRepository<UserModule>(context);
        ModuleMenus = new GenericRepository<ModuleMenu>(context);
        MenuPermissions = new GenericRepository<MenuPermission>(context);
        MenuRolePermissions = new GenericRepository<MenuRolePermissionsEntity>(context);
        DefaultMenuRolePermissions = new GenericRepository<DefultMenuRolePermissionsEntity>(context);

        Brands = new GenericRepository<Brand>(context);
        ProductCategories = new GenericRepository<ProductCategory>(context);
        ProductSubCategories = new GenericRepository<ProductSubCategory>(context);
        Products = new GenericRepository<Product>(context);
        ProductAboutItems = new GenericRepository<ProductAboutItem>(context);
        ProductColors = new GenericRepository<ProductColor>(context);
        ProductImages = new GenericRepository<ProductImage>(context);
        ProductReviews = new GenericRepository<ProductReview>(context);

        Banners = new GenericRepository<Banner>(context);
        HomeCategoryCollections = new GenericRepository<HomeCategoryCollection>(context);
        HomeCategoryProducts = new GenericRepository<HomeCategoryProduct>(context);
        ContactInfos = new GenericRepository<ContactInfo>(context);
        ContactMessages = new GenericRepository<ContactMessage>(context);

        Orders = new GenericRepository<Order>(context);
        OrderItems = new GenericRepository<OrderItem>(context);

        Vendors = new GenericRepository<Vendor>(context);
    }

    public IGenericRepository<UserRefreshToken> UserRefreshTokens { get; }
    public IGenericRepository<UserModule> UserModules { get; }
    public IGenericRepository<ModuleMenu> ModuleMenus { get; }
    public IGenericRepository<MenuPermission> MenuPermissions { get; }
    public IGenericRepository<MenuRolePermissionsEntity> MenuRolePermissions { get; }
    public IGenericRepository<DefultMenuRolePermissionsEntity> DefaultMenuRolePermissions { get; }

    public IGenericRepository<Brand> Brands { get; }
    public IGenericRepository<ProductCategory> ProductCategories { get; }
    public IGenericRepository<ProductSubCategory> ProductSubCategories { get; }
    public IGenericRepository<Product> Products { get; }
    public IGenericRepository<ProductAboutItem> ProductAboutItems { get; }
    public IGenericRepository<ProductColor> ProductColors { get; }
    public IGenericRepository<ProductImage> ProductImages { get; }
    public IGenericRepository<ProductReview> ProductReviews { get; }

    public IGenericRepository<Banner> Banners { get; }
    public IGenericRepository<HomeCategoryCollection> HomeCategoryCollections { get; }
    public IGenericRepository<HomeCategoryProduct> HomeCategoryProducts { get; }
    public IGenericRepository<ContactInfo> ContactInfos { get; }
    public IGenericRepository<ContactMessage> ContactMessages { get; }

    public IGenericRepository<Order> Orders { get; }
    public IGenericRepository<OrderItem> OrderItems { get; }

    public IGenericRepository<Vendor> Vendors { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public void Dispose() => _context.Dispose();
}
