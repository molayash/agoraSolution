using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerFeedbacks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDelete = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFeedbacks", x => x.Id);
                    table.CheckConstraint("CHK_CustomerFeedback_Rating", "[Rating] >= 1 AND [Rating] <= 5");
                    table.ForeignKey(
                        name: "FK_CustomerFeedbacks_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerFeedbacks_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
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
                name: "IX_CustomerFeedbacks_CustomerId_CreatedAt",
                table: "CustomerFeedbacks",
                columns: new[] { "CustomerId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_OrderId",
                table: "CustomerFeedbacks",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerFeedbacks");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 5, 7, 9, 48, 18, 109, DateTimeKind.Utc).AddTicks(8657));

            migrationBuilder.UpdateData(
                table: "ContactInfo",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 7, 9, 48, 18, 109, DateTimeKind.Utc).AddTicks(8732));
        }
    }
}
