using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAndroidTelemetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "telemetry_issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Severity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Title = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FirstSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OccurrenceCount = table.Column<int>(type: "integer", nullable: false),
                    LastAppVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LastDeviceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telemetry_issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_telemetry_issues_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "telemetry_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MobileDeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TelemetryIssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Severity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    AppVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    AndroidVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    DeviceModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Screen = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Operation = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    ExceptionType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    StackTrace = table.Column<string>(type: "text", nullable: true),
                    HttpMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    HttpRoute = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    HttpStatus = table.Column<int>(type: "integer", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BreadcrumbsJson = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telemetry_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_telemetry_events_mobile_devices_MobileDeviceId",
                        column: x => x.MobileDeviceId,
                        principalTable: "mobile_devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_telemetry_events_telemetry_issues_TelemetryIssueId",
                        column: x => x.TelemetryIssueId,
                        principalTable: "telemetry_issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_telemetry_events_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_events_EventId",
                table: "telemetry_events",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_events_MobileDeviceId_OccurredAtUtc",
                table: "telemetry_events",
                columns: new[] { "MobileDeviceId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_events_TelemetryIssueId_OccurredAtUtc",
                table: "telemetry_events",
                columns: new[] { "TelemetryIssueId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_events_TenantId_OccurredAtUtc",
                table: "telemetry_events",
                columns: new[] { "TenantId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_issues_TenantId_Fingerprint",
                table: "telemetry_issues",
                columns: new[] { "TenantId", "Fingerprint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_issues_TenantId_Severity_LastSeenAtUtc",
                table: "telemetry_issues",
                columns: new[] { "TenantId", "Severity", "LastSeenAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_telemetry_issues_TenantId_Status_LastSeenAtUtc",
                table: "telemetry_issues",
                columns: new[] { "TenantId", "Status", "LastSeenAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "telemetry_events");

            migrationBuilder.DropTable(
                name: "telemetry_issues");
        }
    }
}
