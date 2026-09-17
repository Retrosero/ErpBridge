using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class LogMerkeziL3fHeartbeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastAppVersion",
                table: "agents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastErpKind",
                table: "agents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastErpVersion",
                table: "agents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "agents",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastErrorCode",
                table: "agents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastHostKind",
                table: "agents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncAtUtc",
                table: "agents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastSyncResult",
                table: "agents",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "agent_heartbeat_log",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    QueueDepth = table.Column<int>(type: "integer", nullable: false),
                    LastSyncAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSyncResult = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    LastErrorCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LastError = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    AppVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    HostKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ErpKind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    ErpVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_heartbeat_log", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agent_heartbeat_log_AgentId_ReceivedAtUtc",
                table: "agent_heartbeat_log",
                columns: new[] { "AgentId", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_agent_heartbeat_log_TenantId_ReceivedAtUtc",
                table: "agent_heartbeat_log",
                columns: new[] { "TenantId", "ReceivedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_heartbeat_log");

            migrationBuilder.DropColumn(
                name: "LastAppVersion",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "LastErpKind",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "LastErpVersion",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "LastErrorCode",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "LastHostKind",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "LastSyncAtUtc",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "LastSyncResult",
                table: "agents");
        }
    }
}
