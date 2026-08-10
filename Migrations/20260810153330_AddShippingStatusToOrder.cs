using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaturalShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingStatusToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShippingStatus",
                schema: "public",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TrackingNumber",
                schema: "public",
                table: "Orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingStatus",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TrackingNumber",
                schema: "public",
                table: "Orders");
        }
    }
}
