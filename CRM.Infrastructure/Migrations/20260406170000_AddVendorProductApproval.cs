using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    [DbContext(typeof(CrmDbContext))]
    [Migration("20260406170000_AddVendorProductApproval")]
    public partial class AddVendorProductApproval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Product",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Approved");

            migrationBuilder.AddColumn<long>(
                name: "VendorId",
                table: "Product",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_VendorId_ApprovalStatus",
                table: "Product",
                columns: new[] { "VendorId", "ApprovalStatus" });

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Vendors_VendorId",
                table: "Product",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Vendors_VendorId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_VendorId_ApprovalStatus",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "Product");
        }
    }
}
