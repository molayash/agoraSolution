using CRM.Domain.Entities;
using CRM.Domain.Entities.Auth;
using CRM.Infrastructure.data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
