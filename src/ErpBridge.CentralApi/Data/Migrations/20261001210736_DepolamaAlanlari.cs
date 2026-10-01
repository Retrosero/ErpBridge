using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class DepolamaAlanlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StoredFileId",
                table: "task_attachments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoredFileLargeId",
                table: "catalog_images",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoredFileSmallId",
                table: "catalog_images",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "expense_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentExternalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Kind = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    StoredFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SizeBytes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtMs = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_expense_attachments_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_task_attachments_StoredFileId",
                table: "task_attachments",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_images_StoredFileLargeId",
                table: "catalog_images",
                column: "StoredFileLargeId");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_images_StoredFileSmallId",
                table: "catalog_images",
                column: "StoredFileSmallId");

            migrationBuilder.CreateIndex(
                name: "IX_expense_attachments_StoredFileId",
                table: "expense_attachments",
                column: "StoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_expense_attachments_TenantId_CreatedAtMs",
                table: "expense_attachments",
                columns: new[] { "TenantId", "CreatedAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_expense_attachments_TenantId_DocumentExternalId",
                table: "expense_attachments",
                columns: new[] { "TenantId", "DocumentExternalId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expense_attachments");

            migrationBuilder.DropIndex(
                name: "IX_task_attachments_StoredFileId",
                table: "task_attachments");

            migrationBuilder.DropIndex(
                name: "IX_catalog_images_StoredFileLargeId",
                table: "catalog_images");

            migrationBuilder.DropIndex(
                name: "IX_catalog_images_StoredFileSmallId",
                table: "catalog_images");

            migrationBuilder.DropColumn(
                name: "StoredFileId",
                table: "task_attachments");

            migrationBuilder.DropColumn(
                name: "StoredFileLargeId",
                table: "catalog_images");

            migrationBuilder.DropColumn(
                name: "StoredFileSmallId",
                table: "catalog_images");
        }
    }
}
