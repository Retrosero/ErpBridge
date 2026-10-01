using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class MerkeziDepolama : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stored_files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Area = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Bucket = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Variant = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OwnerType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    OwnerKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrashedAtMs = table.Column<long>(type: "bigint", nullable: true),
                    TrashedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stored_files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stored_files_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_storage",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuotaBytes = table.Column<long>(type: "bigint", nullable: true),
                    UsedBytes = table.Column<long>(type: "bigint", nullable: false),
                    ReservedBytes = table.Column<long>(type: "bigint", nullable: false),
                    RecountedAtMs = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAtMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_storage", x => x.TenantId);
                    table.ForeignKey(
                        name: "FK_tenant_storage_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_Bucket_ObjectKey",
                table: "stored_files",
                columns: new[] { "Bucket", "ObjectKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_TenantId_Area_Status",
                table: "stored_files",
                columns: new[] { "TenantId", "Area", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_stored_files_TenantId_OwnerType_OwnerKey",
                table: "stored_files",
                columns: new[] { "TenantId", "OwnerType", "OwnerKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stored_files");

            migrationBuilder.DropTable(
                name: "tenant_storage");
        }
    }
}
