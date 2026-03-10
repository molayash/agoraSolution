using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addtableupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "OrderVendorForwards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 3, 10, 4, 56, 59, 382, DateTimeKind.Utc).AddTicks(7031));

            migrationBuilder.UpdateData(
                table: "ContactInfo",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 4, 56, 59, 382, DateTimeKind.Utc).AddTicks(7213));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "OrderVendorForwards");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 3, 10, 4, 38, 29, 174, DateTimeKind.Utc).AddTicks(7106));

            migrationBuilder.UpdateData(
                table: "ContactInfo",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 4, 38, 29, 174, DateTimeKind.Utc).AddTicks(7203));
        }
    }
}
