using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class XmlGorselleri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ImageSyncFinishedAtMs",
                table: "tenant_xml_feed_settings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageSyncMessage",
                table: "tenant_xml_feed_settings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ImageSyncRequestedAtMs",
                table: "tenant_xml_feed_settings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ImageSyncStartedAtMs",
                table: "tenant_xml_feed_settings",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageSyncStatsJson",
                table: "tenant_xml_feed_settings",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageSyncStatus",
                table: "tenant_xml_feed_settings",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "xml_images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StockCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    SourceUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    SourceUrlHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ETag = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastModified = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ContentSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    StoredFileSmallId = table.Column<Guid>(type: "uuid", nullable: false),
                    StoredFileLargeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    CheckedAtMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xml_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_xml_images_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tenant_xml_feed_settings_ImageSyncRequestedAtMs",
                table: "tenant_xml_feed_settings",
                column: "ImageSyncRequestedAtMs");

            migrationBuilder.CreateIndex(
                name: "IX_xml_images_StoredFileLargeId",
                table: "xml_images",
                column: "StoredFileLargeId");

            migrationBuilder.CreateIndex(
                name: "IX_xml_images_StoredFileSmallId",
                table: "xml_images",
                column: "StoredFileSmallId");

            migrationBuilder.CreateIndex(
                name: "IX_xml_images_TenantId_StockCode",
                table: "xml_images",
                columns: new[] { "TenantId", "StockCode" });

            migrationBuilder.CreateIndex(
                name: "IX_xml_images_TenantId_StockCode_SourceUrlHash",
                table: "xml_images",
                columns: new[] { "TenantId", "StockCode", "SourceUrlHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "xml_images");

            migrationBuilder.DropIndex(
                name: "IX_tenant_xml_feed_settings_ImageSyncRequestedAtMs",
                table: "tenant_xml_feed_settings");

            migrationBuilder.DropColumn(
                name: "ImageSyncFinishedAtMs",
                table: "tenant_xml_feed_settings");

            migrationBuilder.DropColumn(
                name: "ImageSyncMessage",
                table: "tenant_xml_feed_settings");

            migrationBuilder.DropColumn(
                name: "ImageSyncRequestedAtMs",
                table: "tenant_xml_feed_settings");

            migrationBuilder.DropColumn(
                name: "ImageSyncStartedAtMs",
                table: "tenant_xml_feed_settings");

            migrationBuilder.DropColumn(
                name: "ImageSyncStatsJson",
                table: "tenant_xml_feed_settings");

            migrationBuilder.DropColumn(
                name: "ImageSyncStatus",
                table: "tenant_xml_feed_settings");
        }
    }
}
