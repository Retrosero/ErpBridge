using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class SktKayitlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stock_expiry_ops_applied",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppliedAtMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_expiry_ops_applied", x => new { x.TenantId, x.OpId });
                });

            migrationBuilder.CreateTable(
                name: "stock_expiry_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Barcode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Warehouse = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedSeq = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_expiry_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_expiry_records_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stock_expiry_ops_applied_AppliedAtMs",
                table: "stock_expiry_ops_applied",
                column: "AppliedAtMs");

            migrationBuilder.CreateIndex(
                name: "IX_stock_expiry_records_TenantId_StockCode",
                table: "stock_expiry_records",
                columns: new[] { "TenantId", "StockCode" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_expiry_records_TenantId_UpdatedSeq",
                table: "stock_expiry_records",
                columns: new[] { "TenantId", "UpdatedSeq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_expiry_ops_applied");

            migrationBuilder.DropTable(
                name: "stock_expiry_records");
        }
    }
}
