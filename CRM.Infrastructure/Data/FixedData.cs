using CRM.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.data
{
    public static class FixedData
    {
        public static void Seed(ModelBuilder builder)
        {
            // ============================
            // ROLES
            // ============================
            var adminRoleId = "ROLE-ADMIN-001";
            var userRoleId = "ROLE-USER-001";

            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = adminRoleId,
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    IsSystem = true,
                    ConcurrencyStamp = "ROLE_ADMIN_STAMP"
                },
                new ApplicationRole
                {
                    Id = userRoleId,
                    Name = "User",
                    NormalizedName = "USER",
                    IsSystem = false,
                    ConcurrencyStamp = "ROLE_USER_STAMP"
                }
            );

            // ============================
            // USERS
            // ============================
            var adminUserId = "ADMIN-USER-001";

            builder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = adminUserId,
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Email = "admin@crm.com",
                    NormalizedEmail = "ADMIN@CRM.COM",
                    EmailConfirmed = true,

                    FullName = "System Administrator",
                    EntryBy = "SYSTEM",
                    CreatedDate = DateTime.UtcNow,

                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false,
                    TwoFactorEnabled = false,

                    PasswordHash = "AQAAAAEAACcQAAAAEMGdj0vCOmHELE7NchpkGNDqXdzQogb6k2E53QrdBTGm4eLTSs+RNr9k+QkG71ZQcg==", // pre-generated
                    SecurityStamp = "ADMIN_SECURITY_STAMP",
                    ConcurrencyStamp = "ADMIN_CONCURRENCY_STAMP",
                    AccessFailedCount = 0
                }
            );

            // ============================
            // USER ↔ ROLE
            // ============================
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    UserId = adminUserId,
                    RoleId = adminRoleId
                }
            );

            // ============================
            // CONTACT INFO
            // ============================
            builder.Entity<CRM.Domain.Entities.ContactInfo>().HasData(
                new CRM.Domain.Entities.ContactInfo
                {
                    Id = 1,
                    Phone1 = "+88 01771528299",
                    Phone2 = "+45 60818181",
                    Email1 = "mf@plan365.dk",
                    Email2 = "mmfaruk@mfcon.dk",
                    Website1 = "www.mfcon.dk",
                    Website2 = "www.plan365.dk",
                    HeadOffice = "Vognmandsmarken 45, 2mf, 2100 Copenhagen, Denmark",
                    BangladeshOffice = "59/4/2 North Basabo, Dhaka-1214, Bangladesh",
                    CreatedAt = DateTime.UtcNow,
                    IsDelete = 0
                }
            );
        }
    }
}
