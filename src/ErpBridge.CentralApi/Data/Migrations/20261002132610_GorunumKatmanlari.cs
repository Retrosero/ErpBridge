using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class GorunumKatmanlari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LocksJson",
                table: "mobile_user_preferences",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<long>(
                name: "LocksVersion",
                table: "mobile_user_preferences",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedByClient",
                table: "mobile_user_preferences",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedByUserId",
                table: "mobile_user_preferences",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tenant_role_view_preferences",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Json = table.Column<string>(type: "jsonb", nullable: false),
                    LocksJson = table.Column<string>(type: "jsonb", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_role_view_preferences", x => new { x.TenantId, x.Role });
                    table.ForeignKey(
                        name: "FK_tenant_role_view_preferences_tenants_TenantId",
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
                name: "tenant_role_view_preferences");

            migrationBuilder.DropColumn(
                name: "LocksJson",
                table: "mobile_user_preferences");

            migrationBuilder.DropColumn(
                name: "LocksVersion",
                table: "mobile_user_preferences");

            migrationBuilder.DropColumn(
                name: "UpdatedByClient",
                table: "mobile_user_preferences");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "mobile_user_preferences");
        }
    }
}
