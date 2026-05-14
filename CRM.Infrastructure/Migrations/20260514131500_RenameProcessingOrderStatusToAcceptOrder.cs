using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CrmDbContext))]
    [Migration("20260514131500_RenameProcessingOrderStatusToAcceptOrder")]
    public partial class RenameProcessingOrderStatusToAcceptOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Orders
                SET Status = 'accept order'
                WHERE LOWER(ISNULL(Status, '')) = 'processing'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Orders
                SET Status = 'processing'
                WHERE LOWER(ISNULL(Status, '')) = 'accept order'
            ");
        }
    }
}
