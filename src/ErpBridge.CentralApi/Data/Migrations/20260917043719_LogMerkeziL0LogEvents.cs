using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class LogMerkeziL0LogEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "log_error_groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExceptionType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Operation = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Severity = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    SampleMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    TopFrame = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FirstSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FirstSeenMs = table.Column<long>(type: "bigint", nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeenMs = table.Column<long>(type: "bigint", nullable: false),
                    TotalCount = table.Column<long>(type: "bigint", nullable: false),
                    LastAppVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    StatusChangedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusChangedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReopenedAtMs = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_log_error_groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "log_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OccurredAtMs = table.Column<long>(type: "bigint", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    Severity = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Operation = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Screen = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ExceptionType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StackTrace = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    AppVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OsVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DeviceModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AgentId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    HttpMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    HttpRoute = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    HttpStatus = table.Column<int>(type: "integer", nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: true),
                    RepeatCount = table.Column<int>(type: "integer", nullable: false),
                    FingerprintId = table.Column<Guid>(type: "uuid", nullable: true),
                    PropertiesJson = table.Column<string>(type: "jsonb", nullable: false),
                    BreadcrumbsJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_log_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_log_error_groups_Fingerprint",
                table: "log_error_groups",
                column: "Fingerprint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_log_error_groups_LastSeenMs",
                table: "log_error_groups",
                column: "LastSeenMs");

            migrationBuilder.CreateIndex(
                name: "IX_log_error_groups_Status_LastSeenMs",
                table: "log_error_groups",
                columns: new[] { "Status", "LastSeenMs" });

            migrationBuilder.CreateIndex(
                name: "IX_log_events_AgentId_OccurredAtMs",
                table: "log_events",
                columns: new[] { "AgentId", "OccurredAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_log_events_CorrelationId",
                table: "log_events",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_log_events_DeviceId_OccurredAtMs",
                table: "log_events",
                columns: new[] { "DeviceId", "OccurredAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_log_events_FingerprintId_OccurredAtMs",
                table: "log_events",
                columns: new[] { "FingerprintId", "OccurredAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_log_events_OccurredAtMs",
                table: "log_events",
                column: "OccurredAtMs");

            migrationBuilder.CreateIndex(
                name: "IX_log_events_Severity_ReceivedAtMs",
                table: "log_events",
                columns: new[] { "Severity", "ReceivedAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_log_events_Source_EventId",
                table: "log_events",
                columns: new[] { "Source", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_log_events_Source_Severity_OccurredAtMs",
                table: "log_events",
                columns: new[] { "Source", "Severity", "OccurredAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_log_events_TenantId_OccurredAtMs",
                table: "log_events",
                columns: new[] { "TenantId", "OccurredAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_log_events_UserId_OccurredAtMs",
                table: "log_events",
                columns: new[] { "UserId", "OccurredAtMs" });

            // Log Merkezi D2 / D15: PostgreSQL only (relational tests run SQLite with EnsureCreated).
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.Sql(ErpBridge.CentralApi.LogCenter.LogCenterMigrationSql.CopyLegacyTelemetry);
                migrationBuilder.Sql(ErpBridge.CentralApi.LogCenter.LogCenterMigrationSql.MessageTrigramIndex);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "log_error_groups");

            migrationBuilder.DropTable(
                name: "log_events");
        }
    }
}
