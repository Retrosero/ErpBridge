using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class BekleyenSiparisler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "suspended_sale_ops_applied",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppliedAtMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suspended_sale_ops_applied", x => new { x.TenantId, x.OpId });
                });

            migrationBuilder.CreateTable(
                name: "suspended_sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocNo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CustomerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Warehouse = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LinesJson = table.Column<string>(type: "jsonb", nullable: false),
                    LineCount = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    ClosedReason = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ClosedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedSeq = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suspended_sales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_suspended_sales_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_suspended_sale_ops_applied_AppliedAtMs",
                table: "suspended_sale_ops_applied",
                column: "AppliedAtMs");

            migrationBuilder.CreateIndex(
                name: "IX_suspended_sales_TenantId_UpdatedSeq",
                table: "suspended_sales",
                columns: new[] { "TenantId", "UpdatedSeq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "suspended_sale_ops_applied");

            migrationBuilder.DropTable(
                name: "suspended_sales");
        }
    }
}
