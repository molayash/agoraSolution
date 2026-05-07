using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    public partial class AddVendorDeliveredStringId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VendorDeliveredId",
                table: "VendorDelivereds",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                defaultValueSql: "CONCAT('VDL-', REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))");

            migrationBuilder.Sql(
                """
                UPDATE VendorDelivereds
                SET VendorDeliveredId = CONCAT('VDL-', FORMAT(COALESCE(CreatedAt, GETUTCDATE()), 'yyyyMMddHHmmss'), '-', RIGHT('0000' + CAST(Id % 10000 AS varchar(4)), 4))
                WHERE VendorDeliveredId IS NULL OR LTRIM(RTRIM(VendorDeliveredId)) = ''
                """);

            migrationBuilder.AlterColumn<string>(
                name: "VendorDeliveredId",
                table: "VendorDelivereds",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValueSql: "CONCAT('VDL-', REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldDefaultValueSql: "CONCAT('VDL-', REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))");

            migrationBuilder.CreateIndex(
                name: "IX_VendorDelivereds_VendorDeliveredId",
                table: "VendorDelivereds",
                column: "VendorDeliveredId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VendorDelivereds_VendorDeliveredId",
                table: "VendorDelivereds");

            migrationBuilder.DropColumn(
                name: "VendorDeliveredId",
                table: "VendorDelivereds");
        }
    }
}
