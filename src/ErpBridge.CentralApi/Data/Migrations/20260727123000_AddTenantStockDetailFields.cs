using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(CentralApiDbContext))]
[Migration("20260727123000_AddTenantStockDetailFields")]
public partial class AddTenantStockDetailFields : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "StockDetailFieldsJson",
            table: "tenants",
            type: "jsonb",
            nullable: false,
            defaultValue: "[]");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "StockDetailFieldsJson", table: "tenants");
    }
}
