using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    public partial class RenameVendorDeliveredStringId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VendorDeliveredId",
                table: "VendorDelivereds",
                newName: "VendorDeliveredStringId");

            migrationBuilder.RenameIndex(
                name: "IX_VendorDelivereds_VendorDeliveredId",
                table: "VendorDelivereds",
                newName: "IX_VendorDelivereds_VendorDeliveredStringId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_VendorDelivereds_VendorDeliveredStringId",
                table: "VendorDelivereds",
                newName: "IX_VendorDelivereds_VendorDeliveredId");

            migrationBuilder.RenameColumn(
                name: "VendorDeliveredStringId",
                table: "VendorDelivereds",
                newName: "VendorDeliveredId");
        }
    }
}
