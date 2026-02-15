using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dfg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsShow",
                table: "ProductSubCategory",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShow",
                table: "ProductCategory",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 2, 13, 12, 25, 59, 310, DateTimeKind.Utc).AddTicks(2439));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsShow",
                table: "ProductSubCategory");

            migrationBuilder.DropColumn(
                name: "IsShow",
                table: "ProductCategory");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 2, 13, 4, 35, 45, 70, DateTimeKind.Utc).AddTicks(1594));
        }
    }
}
