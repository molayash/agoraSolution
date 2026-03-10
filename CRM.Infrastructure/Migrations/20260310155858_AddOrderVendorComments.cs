using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderVendorComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "OrderVendorForwards");

            migrationBuilder.CreateTable(
                name: "OrderVendorComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    VendorId = table.Column<long>(type: "bigint", nullable: false),
                    OrderVendorForwardId = table.Column<long>(type: "bigint", nullable: true),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    SenderRole = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsDelete = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderVendorComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderVendorComments_OrderVendorForwards_OrderVendorForwardId",
                        column: x => x.OrderVendorForwardId,
                        principalTable: "OrderVendorForwards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderVendorComments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderVendorComments_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 3, 10, 15, 58, 57, 651, DateTimeKind.Utc).AddTicks(1504));

            migrationBuilder.UpdateData(
                table: "ContactInfo",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 10, 15, 58, 57, 651, DateTimeKind.Utc).AddTicks(1573));

            migrationBuilder.CreateIndex(
                name: "IX_OrderVendorComments_OrderId_VendorId_CreatedAt",
                table: "OrderVendorComments",
                columns: new[] { "OrderId", "VendorId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderVendorComments_OrderVendorForwardId",
                table: "OrderVendorComments",
                column: "OrderVendorForwardId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderVendorComments_VendorId",
                table: "OrderVendorComments",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderVendorComments");

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
    }
}
