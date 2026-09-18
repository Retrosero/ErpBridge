using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class ParametreYonetimiP1gAynaRaporu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parameter_mirror_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ErpCompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppliedRevision = table.Column<long>(type: "bigint", nullable: false),
                    Inserted = table.Column<int>(type: "integer", nullable: false),
                    Updated = table.Column<int>(type: "integer", nullable: false),
                    Deleted = table.Column<int>(type: "integer", nullable: false),
                    Drifted = table.Column<int>(type: "integer", nullable: false),
                    Failed = table.Column<int>(type: "integer", nullable: false),
                    ErrorText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parameter_mirror_reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parameter_mirror_reports_agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_parameter_mirror_reports_erp_companies_ErpCompanyId",
                        column: x => x.ErpCompanyId,
                        principalTable: "erp_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_parameter_mirror_reports_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parameter_mirror_drifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterMirrorReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterCatalogEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    MobileUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Scope1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Scope2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExpectedValue = table.Column<string>(type: "text", nullable: false),
                    FoundValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parameter_mirror_drifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parameter_mirror_drifts_mobile_users_MobileUserId",
                        column: x => x.MobileUserId,
                        principalTable: "mobile_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_parameter_mirror_drifts_parameter_catalog_entries_Parameter~",
                        column: x => x.ParameterCatalogEntryId,
                        principalTable: "parameter_catalog_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_parameter_mirror_drifts_parameter_mirror_reports_ParameterM~",
                        column: x => x.ParameterMirrorReportId,
                        principalTable: "parameter_mirror_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parameter_mirror_drifts_MobileUserId",
                table: "parameter_mirror_drifts",
                column: "MobileUserId");

            migrationBuilder.CreateIndex(
                name: "IX_parameter_mirror_drifts_ParameterCatalogEntryId",
                table: "parameter_mirror_drifts",
                column: "ParameterCatalogEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_parameter_mirror_drifts_ParameterMirrorReportId",
                table: "parameter_mirror_drifts",
                column: "ParameterMirrorReportId");

            migrationBuilder.CreateIndex(
                name: "IX_parameter_mirror_reports_AgentId",
                table: "parameter_mirror_reports",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_parameter_mirror_reports_ErpCompanyId",
                table: "parameter_mirror_reports",
                column: "ErpCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_parameter_mirror_reports_TenantId_ErpCompanyId_AtUtc",
                table: "parameter_mirror_reports",
                columns: new[] { "TenantId", "ErpCompanyId", "AtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parameter_mirror_drifts");

            migrationBuilder.DropTable(
                name: "parameter_mirror_reports");
        }
    }
}
