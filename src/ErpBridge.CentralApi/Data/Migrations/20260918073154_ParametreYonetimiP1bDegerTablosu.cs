using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class ParametreYonetimiP1bDegerTablosu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "parameter_values",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ErpCompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterCatalogEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    MobileUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Scope1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Scope2 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parameter_values", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parameter_values_erp_companies_ErpCompanyId",
                        column: x => x.ErpCompanyId,
                        principalTable: "erp_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_parameter_values_mobile_users_MobileUserId",
                        column: x => x.MobileUserId,
                        principalTable: "mobile_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_parameter_values_parameter_catalog_entries_ParameterCatalog~",
                        column: x => x.ParameterCatalogEntryId,
                        principalTable: "parameter_catalog_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_parameter_values_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_parameter_values_ErpCompanyId",
                table: "parameter_values",
                column: "ErpCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_parameter_values_MobileUserId",
                table: "parameter_values",
                column: "MobileUserId");

            migrationBuilder.CreateIndex(
                name: "IX_parameter_values_ParameterCatalogEntryId",
                table: "parameter_values",
                column: "ParameterCatalogEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_parameter_values_TenantId_ErpCompanyId",
                table: "parameter_values",
                columns: new[] { "TenantId", "ErpCompanyId" });

            migrationBuilder.CreateIndex(
                name: "IX_parameter_values_TenantId_ErpCompanyId_ParameterCatalogEntr~",
                table: "parameter_values",
                columns: new[] { "TenantId", "ErpCompanyId", "ParameterCatalogEntryId", "MobileUserId", "Scope1", "Scope2" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "parameter_values");
        }
    }
}
