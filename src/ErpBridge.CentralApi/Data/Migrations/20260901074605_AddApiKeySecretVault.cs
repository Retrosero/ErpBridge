using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeySecretVault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "VaultCiphertext",
                table: "api_keys",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "VaultNonce",
                table: "api_keys",
                type: "bytea",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "VaultTag",
                table: "api_keys",
                type: "bytea",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "api_key_secret_access_audits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiKeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RemoteIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_key_secret_access_audits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_api_key_secret_access_audits_api_keys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "api_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_api_key_secret_access_audits_AdminUserId",
                table: "api_key_secret_access_audits",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_api_key_secret_access_audits_ApiKeyId_AccessedAtUtc",
                table: "api_key_secret_access_audits",
                columns: new[] { "ApiKeyId", "AccessedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_key_secret_access_audits");

            migrationBuilder.DropColumn(
                name: "VaultCiphertext",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "VaultNonce",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "VaultTag",
                table: "api_keys");
        }
    }
}
