using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactInfo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Phone1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Website2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadOffice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BangladeshOffice = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDelete = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSeen = table.Column<bool>(type: "bit", nullable: false),
                    IsDelete = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 2, 15, 12, 20, 23, 761, DateTimeKind.Utc).AddTicks(4265));

            migrationBuilder.InsertData(
                table: "ContactInfo",
                columns: new[] { "Id", "BangladeshOffice", "CreatedAt", "CreatedBy", "Email1", "Email2", "HeadOffice", "IsDelete", "Phone1", "Phone2", "UpdatedAt", "UpdatedBy", "Website1", "Website2" },
                values: new object[] { 1L, "59/4/2 North Basabo, Dhaka-1214, Bangladesh", new DateTime(2026, 2, 15, 12, 20, 23, 761, DateTimeKind.Utc).AddTicks(4334), null, "mf@plan365.dk", "mmfaruk@mfcon.dk", "Vognmandsmarken 45, 2mf, 2100 Copenhagen, Denmark", 0, "+88 01771528299", "+45 60818181", null, null, "www.mfcon.dk", "www.plan365.dk" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactInfo");

            migrationBuilder.DropTable(
                name: "ContactMessages");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "ADMIN-USER-001",
                column: "CreatedDate",
                value: new DateTime(2026, 2, 14, 15, 24, 43, 536, DateTimeKind.Utc).AddTicks(1226));
        }
    }
}
