using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class gggggg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerDelivereds_Vendors_VendorId",
                table: "CustomerDelivereds");

            migrationBuilder.DropIndex(
                name: "IX_CustomerDelivereds_OrderId_VendorId",
                table: "CustomerDelivereds");

            migrationBuilder.DropIndex(
                name: "IX_CustomerDelivereds_VendorId",
                table: "CustomerDelivereds");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "CustomerDelivereds");

            migrationBuilder.AddColumn<long>(
                name: "CustomerId",
                table: "CustomerDelivereds",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "VendorDeliveredId",
                table: "CustomerDeliveredDetails",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "VendorId",
                table: "CustomerDeliveredDetails",
                type: "bigint",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 5, 7, 9, 38, 38, 99, DateTimeKind.Utc).AddTicks(154));

            migrationBuilder.UpdateData(
                table: "ContactInfo",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 9, 38, 38, 99, DateTimeKind.Utc).AddTicks(284));

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDelivereds_CustomerId",
                table: "CustomerDelivereds",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDelivereds_OrderId",
                table: "CustomerDelivereds",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDeliveredDetails_VendorDeliveredId",
                table: "CustomerDeliveredDetails",
                column: "VendorDeliveredId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDeliveredDetails_VendorId",
                table: "CustomerDeliveredDetails",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerDelivereds_Customers_CustomerId",
                table: "CustomerDelivereds",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerDelivereds_Customers_CustomerId",
                table: "CustomerDelivereds");

            migrationBuilder.DropIndex(
                name: "IX_CustomerDelivereds_CustomerId",
                table: "CustomerDelivereds");

            migrationBuilder.DropIndex(
                name: "IX_CustomerDelivereds_OrderId",
                table: "CustomerDelivereds");

            migrationBuilder.DropIndex(
                name: "IX_CustomerDeliveredDetails_VendorDeliveredId",
                table: "CustomerDeliveredDetails");

            migrationBuilder.DropIndex(
                name: "IX_CustomerDeliveredDetails_VendorId",
                table: "CustomerDeliveredDetails");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "CustomerDelivereds");

            migrationBuilder.DropColumn(
                name: "VendorDeliveredId",
                table: "CustomerDeliveredDetails");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "CustomerDeliveredDetails");

            migrationBuilder.AddColumn<long>(
                name: "VendorId",
                table: "CustomerDelivereds",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 5, 7, 5, 15, 22, 115, DateTimeKind.Utc).AddTicks(4521));

            migrationBuilder.UpdateData(
                table: "ContactInfo",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 5, 15, 22, 115, DateTimeKind.Utc).AddTicks(4578));

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDelivereds_OrderId_VendorId",
                table: "CustomerDelivereds",
                columns: new[] { "OrderId", "VendorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDelivereds_VendorId",
                table: "CustomerDelivereds",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerDelivereds_Vendors_VendorId",
                table: "CustomerDelivereds",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id");
        }
    }
}
