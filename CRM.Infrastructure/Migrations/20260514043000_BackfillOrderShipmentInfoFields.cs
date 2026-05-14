using CRM.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CrmDbContext))]
    [Migration("20260514043000_BackfillOrderShipmentInfoFields")]
    public partial class BackfillOrderShipmentInfoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Orders
                SET
                    ShipmentInfo = ISNULL(ShipmentInfo, ''),
                    ShipmentLiveTrackLink = ISNULL(ShipmentLiveTrackLink, ''),
                    ShipmentProvider = ISNULL(ShipmentProvider, '')
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE Orders
                SET
                    ShipmentInfo = NULLIF(ShipmentInfo, ''),
                    ShipmentLiveTrackLink = NULLIF(ShipmentLiveTrackLink, ''),
                    ShipmentProvider = NULLIF(ShipmentProvider, '')
            ");
        }
    }
}
