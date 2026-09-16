using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Faz45MobileUserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mobile_user_roles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    GrantedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GrantedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mobile_user_roles", x => new { x.UserId, x.Role });
                    table.ForeignKey(
                        name: "FK_mobile_user_roles_mobile_users_UserId",
                        column: x => x.UserId,
                        principalTable: "mobile_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mobile_user_roles_Role",
                table: "mobile_user_roles",
                column: "Role");

            // Every existing user keeps exactly the role they had. Deleted users too: their
            // documents stay attributable and a reactivation must not leave them role-less.
            migrationBuilder.Sql(
                """
                INSERT INTO mobile_user_roles ("UserId", "Role", "GrantedAtUtc")
                SELECT "Id", "Role", "CreatedAtUtc" FROM mobile_users
                ON CONFLICT DO NOTHING;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mobile_user_roles");
        }
    }
}
