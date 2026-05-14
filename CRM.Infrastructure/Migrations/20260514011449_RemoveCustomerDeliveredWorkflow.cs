using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCustomerDeliveredWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[dbo].[CustomerDeliveredDetails]', N'U') IS NOT NULL
                    DROP TABLE [dbo].[CustomerDeliveredDetails];
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[dbo].[CustomerDelivereds]', N'U') IS NOT NULL
                    DROP TABLE [dbo].[CustomerDelivereds];
            ");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 5, 14, 1, 14, 48, 867, DateTimeKind.Utc).AddTicks(8886));

            migrationBuilder.UpdateData(
                table: "ContactInfo",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 14, 1, 14, 48, 867, DateTimeKind.Utc).AddTicks(8940));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerDelivereds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDelete = table.Column<int>(type: "int", nullable: true),
                    IsFinalized = table.Column<bool>(type: "bit", nullable: false),
                    ShipmentCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShipmentInfo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ShipmentProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShipmentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, computedColumnSql: "[SubTotal] - [DiscountAmount] + [ShipmentCharge] + [VatAmount]", stored: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDelivereds", x => x.Id);
                    table.CheckConstraint("CHK_CustomerDelivered_Discount", "[DiscountAmount] >= 0");
                    table.CheckConstraint("CHK_CustomerDelivered_ShipmentCharge", "[ShipmentCharge] >= 0");
                    table.CheckConstraint("CHK_CustomerDelivered_Vat", "[VatAmount] >= 0");
                    table.ForeignKey(
                        name: "FK_CustomerDelivereds_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CustomerDelivereds_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerDeliveredDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerDeliveredId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsDelete = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false, computedColumnSql: "[Quantity] * [UnitPrice]", stored: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    VendorDeliveredId = table.Column<long>(type: "bigint", nullable: true),
                    VendorId = table.Column<long>(type: "bigint", nullable: true)
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
                value: new DateTime(2026, 5, 12, 12, 6, 31, 959, DateTimeKind.Utc).AddTicks(3273));

            migrationBuilder.UpdateData(
                table: "ContactInfo",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 12, 12, 6, 31, 959, DateTimeKind.Utc).AddTicks(3351));

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDeliveredDetails_CustomerDeliveredId",
                table: "CustomerDeliveredDetails",
                column: "CustomerDeliveredId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDeliveredDetails_VendorDeliveredId",
                table: "CustomerDeliveredDetails",
                column: "VendorDeliveredId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDeliveredDetails_VendorId",
                table: "CustomerDeliveredDetails",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDelivereds_CustomerId",
                table: "CustomerDelivereds",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDelivereds_OrderId",
                table: "CustomerDelivereds",
                column: "OrderId",
                unique: true);
        }
    }
}
