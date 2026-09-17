using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class LogMerkeziL3AgentHealth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "jobs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AppVersion",
                table: "agents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HostKind",
                table: "agents",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastError",
                table: "agents",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastHeartbeatLoggedAtUtc",
                table: "agents",
                type: "timestamp with time zone",
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    QueueDepth = table.Column<int>(type: "integer", nullable: false),
                    AppVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    HostKind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    LastSyncAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastSyncResult = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    LastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_heartbeat_log", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_jobs_CorrelationId",
                table: "jobs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_agent_heartbeat_log_AgentId_RecordedAtMs",
                table: "agent_heartbeat_log",
                columns: new[] { "AgentId", "RecordedAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_agent_heartbeat_log_RecordedAtMs",
                table: "agent_heartbeat_log",
                column: "RecordedAtMs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_heartbeat_log");

            migrationBuilder.DropIndex(
                name: "IX_jobs_CorrelationId",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "AppVersion",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "HostKind",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "LastError",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "LastHeartbeatLoggedAtUtc",
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
