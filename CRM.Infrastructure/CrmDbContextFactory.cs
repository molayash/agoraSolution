using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CRM.Infrastructure;

public class CrmDbContextFactory : IDesignTimeDbContextFactory<CrmDbContext>
{
    public CrmDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CrmDbContext>();

        // EF migration scaffolding only needs a configured provider at design time.
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=AgoraFoodDesignTime;Trusted_Connection=True;TrustServerCertificate=True;");

        return new CrmDbContext(optionsBuilder.Options);
    }
}
