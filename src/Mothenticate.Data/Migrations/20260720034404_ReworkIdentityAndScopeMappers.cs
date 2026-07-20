using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mothenticate.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReworkIdentityAndScopeMappers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPropertyValues");

            migrationBuilder.DropTable(
                name: "UserProperties");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "UserNameIndex",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AvatarContentType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AvatarData",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AvatarsEnabled",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GitHubClientId",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GitHubClientSecret",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GitHubSsoEnabled",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GoogleClientId",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GoogleClientSecret",
                table: "AppSettings");

            migrationBuilder.DropColumn(
                name: "GoogleSsoEnabled",
                table: "AppSettings");

            migrationBuilder.CreateTable(
                name: "ClientScopes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AssignedType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Protocol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DisplayOnConsentScreen = table.Column<bool>(type: "boolean", nullable: false),
                    ConsentText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IncludeInTokenScope = table.Column<bool>(type: "boolean", nullable: false),
                    IncludeInMetadata = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientScopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IdentityProviderConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Properties = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityProviderConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InputType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsMultivalued = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EnabledWhen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    RequiredFor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RequiredWhen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CanEditUser = table.Column<bool>(type: "boolean", nullable: false),
                    CanEditAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    CanViewUser = table.Column<bool>(type: "boolean", nullable: false),
                    CanViewAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    IsBuiltIn = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAttributes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientScopeMappers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientScopeId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MapperType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Config = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientScopeMappers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientScopeMappers_ClientScopes_ClientScopeId",
                        column: x => x.ClientScopeId,
                        principalTable: "ClientScopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdentityProviderTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProtocolType = table.Column<string>(type: "text", nullable: false),
                    DefaultConfigurationId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityProviderTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityProviderTypes_IdentityProviderConfigurations_Defaul~",
                        column: x => x.DefaultConfigurationId,
                        principalTable: "IdentityProviderConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserAttributeScopes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserAttributeId = table.Column<int>(type: "integer", nullable: false),
                    ScopeId = table.Column<int>(type: "integer", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAttributeScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAttributeScopes_ClientScopes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "ClientScopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAttributeScopes_UserAttributes_UserAttributeId",
                        column: x => x.UserAttributeId,
                        principalTable: "UserAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAttributeValidators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserAttributeId = table.Column<int>(type: "integer", nullable: false),
                    ValidatorType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConfigJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAttributeValidators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAttributeValidators_UserAttributes_UserAttributeId",
                        column: x => x.UserAttributeId,
                        principalTable: "UserAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAttributeValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UserAttributeId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Ordinal = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAttributeValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAttributeValues_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAttributeValues_UserAttributes_UserAttributeId",
                        column: x => x.UserAttributeId,
                        principalTable: "UserAttributes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IdentityProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProviderTypeId = table.Column<int>(type: "integer", nullable: false),
                    ConfigurationId = table.Column<int>(type: "integer", nullable: true),
                    Alias = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AccountLinkingOnly = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    HideOnLoginPage = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ShowInAccountConsole = table.Column<string>(type: "text", nullable: false),
                    SyncMode = table.Column<string>(type: "text", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityProviders_IdentityProviderConfigurations_Configurat~",
                        column: x => x.ConfigurationId,
                        principalTable: "IdentityProviderConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_IdentityProviders_IdentityProviderTypes_ProviderTypeId",
                        column: x => x.ProviderTypeId,
                        principalTable: "IdentityProviderTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IdentityProviderMappers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdentityProviderId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SyncMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MapperType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Config = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityProviderMappers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityProviderMappers_IdentityProviders_IdentityProviderId",
                        column: x => x.IdentityProviderId,
                        principalTable: "IdentityProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientScopeMappers_ClientScopeId_Name",
                table: "ClientScopeMappers",
                columns: new[] { "ClientScopeId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientScopes_Name",
                table: "ClientScopes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityProviderMappers_IdentityProviderId_Name",
                table: "IdentityProviderMappers",
                columns: new[] { "IdentityProviderId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityProviders_Alias",
                table: "IdentityProviders",
                column: "Alias",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdentityProviders_ConfigurationId",
                table: "IdentityProviders",
                column: "ConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityProviders_ProviderTypeId",
                table: "IdentityProviders",
                column: "ProviderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityProviderTypes_DefaultConfigurationId",
                table: "IdentityProviderTypes",
                column: "DefaultConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityProviderTypes_Name",
                table: "IdentityProviderTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAttributes_Name",
                table: "UserAttributes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAttributeScopes_ScopeId",
                table: "UserAttributeScopes",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAttributeScopes_UserAttributeId_ScopeId_Purpose",
                table: "UserAttributeScopes",
                columns: new[] { "UserAttributeId", "ScopeId", "Purpose" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAttributeValidators_UserAttributeId",
                table: "UserAttributeValidators",
                column: "UserAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAttributeValues_UserAttributeId",
                table: "UserAttributeValues",
                column: "UserAttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAttributeValues_UserId_UserAttributeId_Ordinal",
                table: "UserAttributeValues",
                columns: new[] { "UserId", "UserAttributeId", "Ordinal" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientScopeMappers");

            migrationBuilder.DropTable(
                name: "IdentityProviderMappers");

            migrationBuilder.DropTable(
                name: "UserAttributeScopes");

            migrationBuilder.DropTable(
                name: "UserAttributeValidators");

            migrationBuilder.DropTable(
                name: "UserAttributeValues");

            migrationBuilder.DropTable(
                name: "IdentityProviders");

            migrationBuilder.DropTable(
                name: "ClientScopes");

            migrationBuilder.DropTable(
                name: "UserAttributes");

            migrationBuilder.DropTable(
                name: "IdentityProviderTypes");

            migrationBuilder.DropTable(
                name: "IdentityProviderConfigurations");

            migrationBuilder.AddColumn<string>(
                name: "AvatarContentType",
                table: "AspNetUsers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "AvatarData",
                table: "AspNetUsers",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "AspNetUsers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "AspNetUsers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AvatarsEnabled",
                table: "AppSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GitHubClientId",
                table: "AppSettings",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GitHubClientSecret",
                table: "AppSettings",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GitHubSsoEnabled",
                table: "AppSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GoogleClientId",
                table: "AppSettings",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleClientSecret",
                table: "AppSettings",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GoogleSsoEnabled",
                table: "AppSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    IsReadOnly = table.Column<bool>(type: "boolean", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProperties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPropertyValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPropertyValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPropertyValues_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPropertyValues_UserProperties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "UserProperties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProperties_Name",
                table: "UserProperties",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPropertyValues_PropertyId",
                table: "UserPropertyValues",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPropertyValues_UserId_PropertyId",
                table: "UserPropertyValues",
                columns: new[] { "UserId", "PropertyId" },
                unique: true);
        }
    }
}
