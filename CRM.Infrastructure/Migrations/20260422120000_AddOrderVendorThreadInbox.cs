using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    [DbContext(typeof(CrmDbContext))]
    [Migration("20260422120000_AddOrderVendorThreadInbox")]
    public class AddOrderVendorThreadInbox : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AdminReadAt",
                table: "OrderVendorComments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VendorReadAt",
                table: "OrderVendorComments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AdminLastViewedAt",
                table: "OrderVendorForwards",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FulfillmentStatus",
                table: "OrderVendorForwards",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusUpdatedAt",
                table: "OrderVendorForwards",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusUpdatedByName",
                table: "OrderVendorForwards",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusUpdatedByUserId",
                table: "OrderVendorForwards",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VendorLastViewedAt",
                table: "OrderVendorForwards",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminReadAt",
                table: "OrderVendorComments");

            migrationBuilder.DropColumn(
                name: "VendorReadAt",
                table: "OrderVendorComments");

            migrationBuilder.DropColumn(
                name: "AdminLastViewedAt",
                table: "OrderVendorForwards");

            migrationBuilder.DropColumn(
                name: "FulfillmentStatus",
                table: "OrderVendorForwards");

            migrationBuilder.DropColumn(
                name: "StatusUpdatedAt",
                table: "OrderVendorForwards");

            migrationBuilder.DropColumn(
                name: "StatusUpdatedByName",
                table: "OrderVendorForwards");

            migrationBuilder.DropColumn(
                name: "StatusUpdatedByUserId",
                table: "OrderVendorForwards");

            migrationBuilder.DropColumn(
                name: "VendorLastViewedAt",
                table: "OrderVendorForwards");
        }
    }
}
