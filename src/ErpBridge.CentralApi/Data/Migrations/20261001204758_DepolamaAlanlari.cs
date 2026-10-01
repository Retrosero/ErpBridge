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
                name: "StoredFileLargeId",
                table: "catalog_images",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoredFileSmallId",
                table: "catalog_images",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_catalog_images_StoredFileLargeId",
                table: "catalog_images",
                column: "StoredFileLargeId");

            migrationBuilder.CreateIndex(
                name: "IX_catalog_images_StoredFileSmallId",
                table: "catalog_images",
                column: "StoredFileSmallId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_catalog_images_StoredFileLargeId",
                table: "catalog_images");

            migrationBuilder.DropIndex(
                name: "IX_catalog_images_StoredFileSmallId",
                table: "catalog_images");

            migrationBuilder.DropColumn(
                name: "StoredFileLargeId",
                table: "catalog_images");

            migrationBuilder.DropColumn(
                name: "StoredFileSmallId",
                table: "catalog_images");
        }
    }
}
