using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatuure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HomeCategoryCollections",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductCategoryId = table.Column<long>(type: "bigint", nullable: false),
                    CustomTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsDelete = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeCategoryCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeCategoryCollections_ProductCategory_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ProductCategory",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HomeCategoryProducts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HomeCategoryCollectionId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    IsDelete = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeCategoryProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeCategoryProducts_HomeCategoryCollections_HomeCategoryCollectionId",
                        column: x => x.HomeCategoryCollectionId,
                        principalTable: "HomeCategoryCollections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeCategoryProducts_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 2, 14, 15, 24, 43, 536, DateTimeKind.Utc).AddTicks(1226));

            migrationBuilder.CreateIndex(
                name: "IX_HomeCategoryCollections_ProductCategoryId",
                table: "HomeCategoryCollections",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeCategoryProducts_HomeCategoryCollectionId",
                table: "HomeCategoryProducts",
                column: "HomeCategoryCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeCategoryProducts_ProductId",
                table: "HomeCategoryProducts",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HomeCategoryProducts");

            migrationBuilder.DropTable(
                name: "HomeCategoryCollections");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 2, 14, 12, 26, 9, 900, DateTimeKind.Utc).AddTicks(1315));
        }
    }
}
