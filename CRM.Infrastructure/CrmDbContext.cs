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
    public DbSet<Department> Departments { get; set; }
    public DbSet<Designation> Designations { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeDetail> EmployeeDetails { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply fixed seed data
        FixedData.Seed(builder);
    }
}
