using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaturalShop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderShippingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "VerificationCodes",
                newName: "VerificationCodes",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Products",
                newName: "Products",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "Orders",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                newName: "OrderItems",
                newSchema: "public");

            migrationBuilder.AddColumn<string>(
                name: "RecipientName",
                schema: "public",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecipientPhone",
                schema: "public",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                schema: "public",
                table: "Orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecipientName",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RecipientPhone",
                schema: "public",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                schema: "public",
                table: "Orders");

            migrationBuilder.RenameTable(
                name: "VerificationCodes",
                schema: "public",
                newName: "VerificationCodes");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "public",
                newName: "Products");

            migrationBuilder.RenameTable(
                name: "Orders",
                schema: "public",
                newName: "Orders");

            migrationBuilder.RenameTable(
                name: "OrderItems",
                schema: "public",
                newName: "OrderItems");
        }
    }
}
