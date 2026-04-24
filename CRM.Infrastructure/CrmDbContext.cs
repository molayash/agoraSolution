using CRM.Domain.Entities;
using CRM.Domain.Entities.Auth;
using CRM.Infrastructure.data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure;

public class CrmDbContext:IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public CrmDbContext()
    {
    }

    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }



    #region Auth
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

    public DbSet<UserModule> UserModules { get; set; }
    public DbSet<ModuleMenu> ModuleMenus { get; set; }
    public DbSet<MenuPermission> MenuPermissions { get; set; }
    public DbSet<MenuRolePermissions> MenuRolePermissions { get; set; }
    public DbSet<DefultMenuRolePermissions> DefultMenuRolePermissions { get; set; }
    #endregion

    #region Basic
    public virtual DbSet<Brand> Brands { get; set; }
    public virtual DbSet<ProductCategory> ProductCategory { get; set; }
    public virtual DbSet<ProductSubCategory> ProductSubCategory { get; set; }
    public virtual DbSet<Product> Product { get; set; }
    public virtual DbSet<ProductAboutItem> ProductAboutItem { get; set; }
    public virtual DbSet<ProductColor> ProductColor { get; set; }
    public virtual DbSet<ProductImage> ProductImage { get; set; }
    public virtual DbSet<ProductReview> ProductReview { get; set; }
    public virtual DbSet<Banner> Banner { get; set; }
    public virtual DbSet<HomeCategoryCollection> HomeCategoryCollections { get; set; }
    public virtual DbSet<HomeCategoryProduct> HomeCategoryProducts { get; set; }
    public virtual DbSet<ContactInfo> ContactInfo { get; set; }
    public virtual DbSet<ContactMessage> ContactMessages { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<OrderVendorForward> OrderVendorForwards { get; set; }
    public virtual DbSet<OrderVendorComment> OrderVendorComments { get; set; }
    public virtual DbSet<Vendor> Vendors { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply fixed seed data
        FixedData.Seed(builder);

        builder.Entity<Product>()
            .HasOne(p => p.SubCategory)
            .WithMany()
            .HasForeignKey(p => p.ProductSubCategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Product>()
           .HasOne(p => p.Category)
           .WithMany()
           .HasForeignKey(p => p.ProductCategoryId)
           .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Product>()
            .HasOne(p => p.Vendor)
            .WithMany()
            .HasForeignKey(p => p.VendorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Product>()
            .HasIndex(p => new { p.VendorId, p.ApprovalStatus });

        builder.Entity<HomeCategoryCollection>()
            .HasOne(h => h.Category)
            .WithMany()
            .HasForeignKey(h => h.ProductCategoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<HomeCategoryProduct>()
            .HasOne(hp => hp.HomeCategoryCollection)
            .WithMany(h => h.HomeCategoryProducts)
            .HasForeignKey(hp => hp.HomeCategoryCollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<HomeCategoryProduct>()
            .HasOne(hp => hp.Product)
            .WithMany()
            .HasForeignKey(hp => hp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // Order Configuration
        builder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Customer>()
            .HasOne(customer => customer.User)
            .WithMany()
            .HasForeignKey(customer => customer.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Customer>()
            .HasIndex(customer => customer.UserId)
            .IsUnique();

        builder.Entity<Customer>()
            .HasIndex(customer => customer.Email)
            .IsUnique();

        builder.Entity<Customer>()
            .HasIndex(customer => customer.Phone)
            .IsUnique();

        builder.Entity<Order>()
            .HasOne(order => order.Customer)
            .WithMany(customer => customer.Orders)
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Order>()
            .HasIndex(order => order.CustomerId);

        builder.Entity<OrderVendorForward>()
            .HasOne(ovf => ovf.Order)
            .WithMany()
            .HasForeignKey(ovf => ovf.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrderVendorForward>()
            .HasOne(ovf => ovf.Vendor)
            .WithMany()
            .HasForeignKey(ovf => ovf.VendorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<OrderVendorForward>()
            .HasIndex(ovf => new { ovf.OrderId, ovf.VendorId });

        builder.Entity<OrderVendorComment>()
            .HasOne(ovc => ovc.Order)
            .WithMany()
            .HasForeignKey(ovc => ovc.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrderVendorComment>()
            .HasOne(ovc => ovc.Vendor)
            .WithMany()
            .HasForeignKey(ovc => ovc.VendorId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<OrderVendorComment>()
            .HasOne(ovc => ovc.OrderVendorForward)
            .WithMany()
            .HasForeignKey(ovc => ovc.OrderVendorForwardId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<OrderVendorComment>()
            .HasIndex(ovc => new { ovc.OrderId, ovc.VendorId, ovc.CreatedAt });
    }
}
