using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mothenticate.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientScopeMapperDestinationFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IncludeAccessToken",
                table: "ClientScopeMappers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeIdToken",
                table: "ClientScopeMappers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeIntrospectionToken",
                table: "ClientScopeMappers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeUserInfo",
                table: "ClientScopeMappers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludeAccessToken",
                table: "ClientScopeMappers");

            migrationBuilder.DropColumn(
                name: "IncludeIdToken",
                table: "ClientScopeMappers");

            migrationBuilder.DropColumn(
                name: "IncludeIntrospectionToken",
                table: "ClientScopeMappers");

            migrationBuilder.DropColumn(
                name: "IncludeUserInfo",
                table: "ClientScopeMappers");
        }
    }
}
