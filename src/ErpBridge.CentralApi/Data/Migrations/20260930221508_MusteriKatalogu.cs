using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class MusteriKatalogu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalog_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    PriceListNo = table.Column<int>(type: "integer", nullable: true),
                    VisibilityJson = table.Column<string>(type: "jsonb", nullable: false),
                    ShowStatement = table.Column<bool>(type: "boolean", nullable: false),
                    ShowInvoices = table.Column<bool>(type: "boolean", nullable: false),
                    ShowPurchased = table.Column<bool>(type: "boolean", nullable: false),
                    CanOrder = table.Column<bool>(type: "boolean", nullable: false),
                    ResponsibleUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TokenVersion = table.Column<int>(type: "integer", nullable: false),
                    LastLoginAtMs = table.Column<long>(type: "bigint", nullable: true),
                    PasswordChangedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAtMs = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_catalog_accounts_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_category_settings",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryKey = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: true),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedAtMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_category_settings", x => new { x.TenantId, x.CategoryKey });
                    table.ForeignKey(
                        name: "FK_catalog_category_settings_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Kind = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    SourceHash = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Source = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    SizeBytes = table.Column<int>(type: "integer", nullable: false),
                    HasSmall = table.Column<bool>(type: "boolean", nullable: false),
                    HasLarge = table.Column<bool>(type: "boolean", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Sha256Small = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Sha256Large = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CreatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_catalog_images_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AccountUsername = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    No = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RejectReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DocumentRef = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PriceListNo = table.Column<int>(type: "integer", nullable: false),
                    PriceIncludesVat = table.Column<bool>(type: "boolean", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineCount = table.Column<int>(type: "integer", nullable: false),
                    LinesJson = table.Column<string>(type: "jsonb", nullable: false),
                    AssignedUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClaimedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ClaimedAtMs = table.Column<long>(type: "bigint", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    ClosedAtMs = table.Column<long>(type: "bigint", nullable: true),
                    SubmittedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_catalog_orders_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_product_settings",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: true),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false),
                    NoDiscount = table.Column<bool>(type: "boolean", nullable: false),
                    CartonOnly = table.Column<bool>(type: "boolean", nullable: false),
                    CartonQuantity = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAtMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_product_settings", x => new { x.TenantId, x.StockCode });
                    table.ForeignKey(
                        name: "FK_catalog_product_settings_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_settings",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultPriceListNo = table.Column<int>(type: "integer", nullable: true),
                    Revision = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_settings", x => x.TenantId);
                    table.ForeignKey(
                        name: "FK_catalog_settings_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "catalog_image_blobs",
                columns: table => new
                {
                    ImageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Variant = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_image_blobs", x => new { x.ImageId, x.Variant });
                    table.ForeignKey(
                        name: "FK_catalog_image_blobs_catalog_images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "catalog_images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_accounts_TenantId_CustomerCode",
                table: "catalog_accounts",
                columns: new[] { "TenantId", "CustomerCode" },
                unique: true,
                filter: "\"DeletedAtMs\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_accounts_TenantId_Username",
                table: "catalog_accounts",
                columns: new[] { "TenantId", "Username" },
                unique: true,
                filter: "\"DeletedAtMs\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_images_TenantId_StockCode_SourceHash",
                table: "catalog_images",
                columns: new[] { "TenantId", "StockCode", "SourceHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_orders_TenantId_AccountId_SubmittedAtMs",
                table: "catalog_orders",
                columns: new[] { "TenantId", "AccountId", "SubmittedAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_orders_TenantId_DocumentRef",
                table: "catalog_orders",
                columns: new[] { "TenantId", "DocumentRef" });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_orders_TenantId_No",
                table: "catalog_orders",
                columns: new[] { "TenantId", "No" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_orders_TenantId_Status_SubmittedAtMs",
                table: "catalog_orders",
                columns: new[] { "TenantId", "Status", "SubmittedAtMs" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "catalog_accounts");

            migrationBuilder.DropTable(
                name: "catalog_category_settings");

            migrationBuilder.DropTable(
                name: "catalog_image_blobs");

            migrationBuilder.DropTable(
                name: "catalog_orders");

            migrationBuilder.DropTable(
                name: "catalog_product_settings");

            migrationBuilder.DropTable(
                name: "catalog_settings");

            migrationBuilder.DropTable(
                name: "catalog_images");
        }
    }
}
