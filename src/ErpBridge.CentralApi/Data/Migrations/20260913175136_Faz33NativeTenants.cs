using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Faz33NativeTenants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataSource",
                table: "tenants",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "erp");

            migrationBuilder.AddColumn<long>(
                name: "NativeLockVersion",
                table: "tenants",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "native_customer_balances",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_native_customer_balances", x => new { x.TenantId, x.CustomerCode });
                    table.ForeignKey(
                        name: "FK_native_customer_balances_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "native_stock_levels",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    WarehouseNo = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_native_stock_levels", x => new { x.TenantId, x.StockCode, x.WarehouseNo });
                    table.ForeignKey(
                        name: "FK_native_stock_levels_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "native_customer_balances");

            migrationBuilder.DropTable(
                name: "native_stock_levels");

            migrationBuilder.DropColumn(
                name: "DataSource",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "NativeLockVersion",
                table: "tenants");
        }
    }
}
