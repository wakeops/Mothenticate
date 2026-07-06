using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mothenticate.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowEmailLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowEmailLogin",
                table: "AppSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowEmailLogin",
                table: "AppSettings");
        }
    }
}
