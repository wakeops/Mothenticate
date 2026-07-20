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
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeIdToken",
                table: "ClientScopeMappers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeIntrospectionToken",
                table: "ClientScopeMappers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeUserInfo",
                table: "ClientScopeMappers",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            // Backfill the new columns from the existing free-form Config JSON, then drop those keys —
            // Config now holds only mapper-specific settings, not generic destination routing.
            migrationBuilder.Sql(
                """
                UPDATE "ClientScopeMappers" SET
                    "IncludeAccessToken" = COALESCE(("Config"->>'IncludeAccessToken')::boolean, TRUE),
                    "IncludeIdToken" = COALESCE(("Config"->>'IncludeIdToken')::boolean, TRUE),
                    "IncludeIntrospectionToken" = COALESCE(("Config"->>'IncludeIntrospectionToken')::boolean, TRUE),
                    "IncludeUserInfo" = COALESCE(("Config"->>'IncludeUserInfo')::boolean, TRUE);

                UPDATE "ClientScopeMappers"
                SET "Config" = "Config" - 'IncludeAccessToken' - 'IncludeIdToken' - 'IncludeIntrospectionToken' - 'IncludeUserInfo';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "ClientScopeMappers" SET "Config" = "Config"
                    || jsonb_build_object('IncludeAccessToken', "IncludeAccessToken"::text)
                    || jsonb_build_object('IncludeIdToken', "IncludeIdToken"::text)
                    || jsonb_build_object('IncludeIntrospectionToken', "IncludeIntrospectionToken"::text)
                    || jsonb_build_object('IncludeUserInfo', "IncludeUserInfo"::text);
                """);

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
