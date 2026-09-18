using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class ParametreYonetimiP3cForaIceAktarim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fora_import_batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ErpCompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: true),
                    State = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ScannedRows = table.Column<int>(type: "integer", nullable: false),
                    MatchedRows = table.Column<int>(type: "integer", nullable: false),
                    ScannedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AppliedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AppliedByAdminUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fora_import_batches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fora_import_batches_agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_fora_import_batches_erp_companies_ErpCompanyId",
                        column: x => x.ErpCompanyId,
                        principalTable: "erp_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fora_import_batches_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fora_import_rows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ForaImportBatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParametreProgram = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ParametreUser = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    AnaGrubu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AltGrubu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParametreId = table.Column<int>(type: "integer", nullable: false),
                    ParametreAdi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParametreDegeri = table.Column<string>(type: "text", nullable: false),
                    ParameterCatalogEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    MobileUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDefaultValue = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fora_import_rows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fora_import_rows_fora_import_batches_ForaImportBatchId",
                        column: x => x.ForaImportBatchId,
                        principalTable: "fora_import_batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fora_import_rows_mobile_users_MobileUserId",
                        column: x => x.MobileUserId,
                        principalTable: "mobile_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fora_import_rows_parameter_catalog_entries_ParameterCatalog~",
                        column: x => x.ParameterCatalogEntryId,
                        principalTable: "parameter_catalog_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fora_import_batches_AgentId",
                table: "fora_import_batches",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_fora_import_batches_ErpCompanyId",
                table: "fora_import_batches",
                column: "ErpCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_fora_import_batches_TenantId_ErpCompanyId_ScannedAtUtc",
                table: "fora_import_batches",
                columns: new[] { "TenantId", "ErpCompanyId", "ScannedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_fora_import_rows_ForaImportBatchId",
                table: "fora_import_rows",
                column: "ForaImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_fora_import_rows_MobileUserId",
                table: "fora_import_rows",
                column: "MobileUserId");

            migrationBuilder.CreateIndex(
                name: "IX_fora_import_rows_ParameterCatalogEntryId",
                table: "fora_import_rows",
                column: "ParameterCatalogEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fora_import_rows");

            migrationBuilder.DropTable(
                name: "fora_import_batches");
        }
    }
}
