using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class ErpYazimY1aWriteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "erp_write_settings",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesDocumentKind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, defaultValue: "order"),
                    OrderApprovalMode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, defaultValue: "approved"),
                    OrderSeries = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false, defaultValue: ""),
                    DispatchSeries = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false, defaultValue: ""),
                    InvoiceSeries = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false, defaultValue: ""),
                    ReturnSeries = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false, defaultValue: ""),
                    CollectionSeries = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false, defaultValue: ""),
                    DefaultWarehouseNo = table.Column<int>(type: "integer", nullable: true),
                    DefaultCashCode = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    DefaultCardBankCode = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    DefaultTransferBankCode = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    DefaultErpUserNo = table.Column<int>(type: "integer", nullable: true),
                    DefaultSalespersonCode = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    DefaultPriceListNo = table.Column<int>(type: "integer", nullable: true),
                    ChequePortfolioCode = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false, defaultValue: "ÇEK"),
                    NotePortfolioCode = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false, defaultValue: "SENET"),
                    ResponsibilityCenterCode = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    ProjectCode = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    DeliveryDayOffset = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_write_settings", x => x.TenantId);
                    table.ForeignKey(
                        name: "FK_erp_write_settings_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mobile_user_erp_mappings",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalespersonCode = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    WarehouseNo = table.Column<int>(type: "integer", nullable: true),
                    CashCode = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    CardBankCode = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    TransferBankCode = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: true),
                    ErpUserNo = table.Column<int>(type: "integer", nullable: true),
                    OrderSeries = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    DispatchSeries = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    InvoiceSeries = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    ReturnSeries = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    CollectionSeries = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mobile_user_erp_mappings", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_mobile_user_erp_mappings_mobile_users_UserId",
                        column: x => x.UserId,
                        principalTable: "mobile_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mobile_user_erp_mappings_TenantId",
                table: "mobile_user_erp_mappings",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "erp_write_settings");

            migrationBuilder.DropTable(
                name: "mobile_user_erp_mappings");
        }
    }
}
