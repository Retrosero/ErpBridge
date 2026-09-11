using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Faz26MobileRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mobile_records",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Entity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RecordKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StockKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CustomerKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: true),
                    PayloadSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UpdatedSeq = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeenRunId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mobile_records", x => new { x.TenantId, x.Entity, x.RecordKey });
                    table.ForeignKey(
                        name: "FK_mobile_records_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_sync_counter",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastSeq = table.Column<long>(type: "bigint", nullable: false),
                    TombstoneHorizonSeq = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_sync_counter", x => x.TenantId);
                    table.ForeignKey(
                        name: "FK_tenant_sync_counter_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mobile_records_TenantId_CustomerKey",
                table: "mobile_records",
                columns: new[] { "TenantId", "CustomerKey" });

            migrationBuilder.CreateIndex(
                name: "IX_mobile_records_TenantId_IsDeleted_UpdatedAtUtc",
                table: "mobile_records",
                columns: new[] { "TenantId", "IsDeleted", "UpdatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_mobile_records_TenantId_StockKey",
                table: "mobile_records",
                columns: new[] { "TenantId", "StockKey" });

            migrationBuilder.CreateIndex(
                name: "IX_mobile_records_TenantId_UpdatedSeq",
                table: "mobile_records",
                columns: new[] { "TenantId", "UpdatedSeq" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mobile_records");

            migrationBuilder.DropTable(
                name: "tenant_sync_counter");
        }
    }
}
