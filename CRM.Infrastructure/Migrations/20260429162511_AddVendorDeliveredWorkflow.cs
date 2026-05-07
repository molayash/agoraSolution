using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorDeliveredWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "OrderVendorForwards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "VendorDelivereds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    VendorId = table.Column<long>(type: "bigint", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShipmentCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, computedColumnSql: "[SubTotal] - [DiscountAmount] + [ShipmentCharge] + [VatAmount]", stored: true),
                    ShipmentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShipmentProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShipmentInfo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsFinalized = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorDelivereds", x => x.Id);
                    table.CheckConstraint("CHK_VendorDelivered_Discount", "[DiscountAmount] >= 0");
                    table.CheckConstraint("CHK_VendorDelivered_ShipmentCharge", "[ShipmentCharge] >= 0");
                    table.CheckConstraint("CHK_VendorDelivered_Vat", "[VatAmount] >= 0");
                    table.ForeignKey(
                        name: "FK_VendorDelivereds_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendorDelivereds_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VendorDeliveredDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorDeliveredId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false, computedColumnSql: "[Quantity] * [UnitPrice]", stored: true),
                    IsDelete = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorDeliveredDetails", x => x.Id);
                    table.CheckConstraint("CHK_VendorDeliveredDetail_Quantity", "[Quantity] > 0");
                    table.CheckConstraint("CHK_VendorDeliveredDetail_UnitPrice", "[UnitPrice] >= 0");
                    table.ForeignKey(
                        name: "FK_VendorDeliveredDetails_VendorDelivereds_VendorDeliveredId",
                        column: x => x.VendorDeliveredId,
                        principalTable: "VendorDelivereds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 4, 29, 16, 25, 8, 545, DateTimeKind.Utc).AddTicks(9658));

            migrationBuilder.UpdateData(
                table: "ContactInfo",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 29, 16, 25, 8, 545, DateTimeKind.Utc).AddTicks(9721));

            migrationBuilder.CreateIndex(
                name: "IX_VendorDeliveredDetails_VendorDeliveredId",
                table: "VendorDeliveredDetails",
                column: "VendorDeliveredId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorDelivereds_OrderId_VendorId",
                table: "VendorDelivereds",
                columns: new[] { "OrderId", "VendorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorDelivereds_VendorId",
                table: "VendorDelivereds",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VendorDeliveredDetails");

            migrationBuilder.DropTable(
                name: "VendorDelivereds");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "OrderVendorForwards");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 4, 26, 6, 21, 6, 711, DateTimeKind.Utc).AddTicks(3998));

            migrationBuilder.UpdateData(
                table: "ContactInfo",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 26, 6, 21, 6, 711, DateTimeKind.Utc).AddTicks(4076));
        }
    }
}
