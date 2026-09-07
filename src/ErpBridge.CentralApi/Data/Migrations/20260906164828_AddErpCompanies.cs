using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddErpCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "erp_companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    SourceDatabase = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CompanyNo = table.Column<int>(type: "integer", nullable: false),
                    BranchNo = table.Column<int>(type: "integer", nullable: false),
                    WarehouseNo = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_companies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_erp_companies_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agent_company_assignments",
                columns: table => new
                {
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ErpCompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_company_assignments", x => new { x.AgentId, x.ErpCompanyId });
                    table.ForeignKey(
                        name: "FK_agent_company_assignments_agents_AgentId",
                        column: x => x.AgentId,
                        principalTable: "agents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_agent_company_assignments_erp_companies_ErpCompanyId",
                        column: x => x.ErpCompanyId,
                        principalTable: "erp_companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agent_company_assignments_ErpCompanyId",
                table: "agent_company_assignments",
                column: "ErpCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_erp_companies_TenantId_Code",
                table: "erp_companies",
                columns: new[] { "TenantId", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_company_assignments");

            migrationBuilder.DropTable(
                name: "erp_companies");
        }
    }
}
