using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMobileTelemetryEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mobile_telemetry_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Severity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    AppVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AndroidVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DeviceModel = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Screen = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Operation = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ExceptionType = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StackTrace = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    HttpMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    HttpRoute = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    HttpStatus = table.Column<int>(type: "integer", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    BreadcrumbsJson = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mobile_telemetry_events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mobile_telemetry_events_Severity_ReceivedAtUtc",
                table: "mobile_telemetry_events",
                columns: new[] { "Severity", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_mobile_telemetry_events_TenantId_EventId",
                table: "mobile_telemetry_events",
                columns: new[] { "TenantId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mobile_telemetry_events_TenantId_OccurredAtUtc",
                table: "mobile_telemetry_events",
                columns: new[] { "TenantId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mobile_telemetry_events");
        }
    }
}
