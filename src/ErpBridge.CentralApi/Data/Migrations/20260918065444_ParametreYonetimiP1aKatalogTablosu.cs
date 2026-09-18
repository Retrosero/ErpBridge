using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class ParametreYonetimiP1aKatalogTablosu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parameter_catalog_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Program = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CatalogMethod = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ParametreId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DefaultValue = table.Column<string>(type: "text", nullable: false),
                    DefaultSource = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ScopeKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ScopeFields = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    User = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AnaGrubu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AltGrubu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Editor = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ReferenceKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    SecretSource = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Label = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TabPath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    EditorOrder = table.Column<int>(type: "integer", nullable: true),
                    OptionsJson = table.Column<string>(type: "text", nullable: true),
                    IsImplemented = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeprecated = table.Column<bool>(type: "boolean", nullable: false),
                    SourceBuild = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parameter_catalog_entries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parameter_catalog_entries_CatalogMethod_ParametreId",
                table: "parameter_catalog_entries",
                columns: new[] { "CatalogMethod", "ParametreId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parameter_catalog_entries_Program_EditorOrder",
                table: "parameter_catalog_entries",
                columns: new[] { "Program", "EditorOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parameter_catalog_entries");
        }
    }
}
