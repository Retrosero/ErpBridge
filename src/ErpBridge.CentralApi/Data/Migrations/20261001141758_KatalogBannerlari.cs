using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class KatalogBannerlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalog_banners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Text = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ImageId = table.Column<Guid>(type: "uuid", nullable: true),
                    LinkType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    LinkValue = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    StartsAtMs = table.Column<long>(type: "bigint", nullable: true),
                    EndsAtMs = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalog_banners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_catalog_banners_catalog_images_ImageId",
                        column: x => x.ImageId,
                        principalTable: "catalog_images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_catalog_banners_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_catalog_banners_ImageId",
                table: "catalog_banners",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_banners_TenantId_SortOrder",
                table: "catalog_banners",
                columns: new[] { "TenantId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "catalog_banners");
        }
    }
}
