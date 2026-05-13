using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    CustomerId = table.Column<long>(type: "bigint", nullable: true),
                    VendorId = table.Column<long>(type: "bigint", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ShipmentCarrier = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ShipmentReference = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CustomerCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdminCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShipmentNote = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    InternalNote = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ShipmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequiresCustomerConfirmation = table.Column<bool>(type: "bit", nullable: false),
                    CustomerConfirmationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CustomerConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerConfirmationNote = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsDelete = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deliveries_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Deliveries_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Deliveries_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeliveryDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: true),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDelete = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryDetails_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryDetails_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeliveryDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 4, 27, 5, 5, 22, 314, DateTimeKind.Utc).AddTicks(282));

            migrationBuilder.UpdateData(
                table: "ContactInfo",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 4, 27, 5, 5, 22, 314, DateTimeKind.Utc).AddTicks(348));

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_CustomerId",
                table: "Deliveries",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_OrderId_VendorId",
                table: "Deliveries",
                columns: new[] { "OrderId", "VendorId" });

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_VendorId",
                table: "Deliveries",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryDetails_DeliveryId_OrderItemId",
                table: "DeliveryDetails",
                columns: new[] { "DeliveryId", "OrderItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryDetails_OrderId",
                table: "DeliveryDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryDetails_OrderItemId",
                table: "DeliveryDetails",
                column: "OrderItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryDetails");

            migrationBuilder.DropTable(
                name: "Deliveries");

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
