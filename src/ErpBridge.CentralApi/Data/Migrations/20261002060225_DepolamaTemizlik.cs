using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class DepolamaTemizlik : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "QuarantineRequestedAtMs",
                table: "tenant_storage",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuarantinedFromKey",
                table: "stored_files",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "storage_trash_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Area = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    SnapshotJson = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    TrashedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    TrashedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storage_trash_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_storage_trash_items_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storage_trash_item_files",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storage_trash_item_files", x => new { x.ItemId, x.FileId });
                    table.ForeignKey(
                        name: "FK_storage_trash_item_files_storage_trash_items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "storage_trash_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_storage_trash_item_files_FileId",
                table: "storage_trash_item_files",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_storage_trash_items_TenantId_TrashedAtMs",
                table: "storage_trash_items",
                columns: new[] { "TenantId", "TrashedAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_storage_trash_items_TrashedAtMs",
                table: "storage_trash_items",
                column: "TrashedAtMs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "storage_trash_item_files");

            migrationBuilder.DropTable(
                name: "storage_trash_items");

            migrationBuilder.DropColumn(
                name: "QuarantineRequestedAtMs",
                table: "tenant_storage");

            migrationBuilder.DropColumn(
                name: "QuarantinedFromKey",
                table: "stored_files");
        }
    }
}
