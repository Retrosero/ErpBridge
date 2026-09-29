using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class YetkiMatrisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mobile_user_permission_overrides",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mobile_user_permission_overrides", x => new { x.UserId, x.Key });
                    table.ForeignKey(
                        name: "FK_mobile_user_permission_overrides_mobile_users_UserId",
                        column: x => x.UserId,
                        principalTable: "mobile_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "permission_changes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Client = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Scope = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    TargetUserName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OldValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permission_changes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_role_permissions",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_role_permissions", x => new { x.TenantId, x.Role, x.Key });
                    table.ForeignKey(
                        name: "FK_tenant_role_permissions_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_permission_changes_TenantId_CreatedAtUtc",
                table: "permission_changes",
                columns: new[] { "TenantId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mobile_user_permission_overrides");

            migrationBuilder.DropTable(
                name: "permission_changes");

            migrationBuilder.DropTable(
                name: "tenant_role_permissions");
        }
    }
}
