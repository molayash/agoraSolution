using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDeliveredWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerDelivereds",
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
                    table.PrimaryKey("PK_CustomerDelivereds", x => x.Id);
                    table.CheckConstraint("CHK_CustomerDelivered_Discount", "[DiscountAmount] >= 0");
                    table.CheckConstraint("CHK_CustomerDelivered_ShipmentCharge", "[ShipmentCharge] >= 0");
                    table.CheckConstraint("CHK_CustomerDelivered_Vat", "[VatAmount] >= 0");
                    table.ForeignKey(
                        name: "FK_CustomerDelivereds_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerDelivereds_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerDeliveredDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerDeliveredId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_CustomerDeliveredDetails", x => x.Id);
                    table.CheckConstraint("CHK_CustomerDeliveredDetail_Quantity", "[Quantity] > 0");
                    table.CheckConstraint("CHK_CustomerDeliveredDetail_UnitPrice", "[UnitPrice] >= 0");
                    table.ForeignKey(
                        name: "FK_CustomerDeliveredDetails_CustomerDelivereds_CustomerDeliveredId",
                        column: x => x.CustomerDeliveredId,
                        principalTable: "CustomerDelivereds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_CustomerDeliveredDetails_CustomerDeliveredId",
                table: "CustomerDeliveredDetails",
                column: "CustomerDeliveredId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDelivereds_OrderId_VendorId",
                table: "CustomerDelivereds",
                columns: new[] { "OrderId", "VendorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDelivereds_VendorId",
                table: "CustomerDelivereds",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerDeliveredDetails");

            migrationBuilder.DropTable(
                name: "CustomerDelivereds");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 4, 30, 6, 44, 37, 383, DateTimeKind.Utc).AddTicks(6392));

            migrationBuilder.UpdateData(
                table: "ContactInfo",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 30, 6, 44, 37, 383, DateTimeKind.Utc).AddTicks(6451));
        }
    }
}
