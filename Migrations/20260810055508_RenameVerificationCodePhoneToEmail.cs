using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NaturalShop.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameVerificationCodePhoneToEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "public",
                table: "VerificationCodes");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "public",
                table: "VerificationCodes",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                schema: "public",
                table: "VerificationCodes");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "public",
                table: "VerificationCodes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
