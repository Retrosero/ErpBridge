using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class ParametreYonetimiP1dSurumVeDenetim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parameter_audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ErpCompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterCatalogEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    MobileUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Scope1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Scope2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    IsMasked = table.Column<bool>(type: "boolean", nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Actor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parameter_audit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parameter_audit_parameter_catalog_entries_ParameterCatalogE~",
                        column: x => x.ParameterCatalogEntryId,
                        principalTable: "parameter_catalog_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_parameter_audit_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parameter_revisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ErpCompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    MobileUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Scope1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Scope2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parameter_revisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parameter_revisions_erp_companies_ErpCompanyId",
                        column: x => x.ErpCompanyId,
                        principalTable: "erp_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_parameter_revisions_mobile_users_MobileUserId",
                        column: x => x.MobileUserId,
                        principalTable: "mobile_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_parameter_revisions_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parameter_audit_ParameterCatalogEntryId",
                table: "parameter_audit",
                column: "ParameterCatalogEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_parameter_audit_TenantId_AtUtc",
                table: "parameter_audit",
                columns: new[] { "TenantId", "AtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_parameter_audit_TenantId_ErpCompanyId_ParameterCatalogEntry~",
                table: "parameter_audit",
                columns: new[] { "TenantId", "ErpCompanyId", "ParameterCatalogEntryId", "AtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_parameter_revisions_ErpCompanyId",
                table: "parameter_revisions",
                column: "ErpCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_parameter_revisions_MobileUserId",
                table: "parameter_revisions",
                column: "MobileUserId");

            migrationBuilder.CreateIndex(
                name: "IX_parameter_revisions_TenantId_ErpCompanyId_MobileUserId_Scop~",
                table: "parameter_revisions",
                columns: new[] { "TenantId", "ErpCompanyId", "MobileUserId", "Scope1", "Scope2" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parameter_audit");

            migrationBuilder.DropTable(
                name: "parameter_revisions");
        }
    }
}
