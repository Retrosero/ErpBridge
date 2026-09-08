using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChangeSetAuditLogAndParameterRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "change_set_audit_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDatabase = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TableName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TabloId = table.Column<int>(type: "integer", nullable: false),
                    Direction = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    FirstTriggerRecNo = table.Column<long>(type: "bigint", nullable: false),
                    LastTriggerRecNo = table.Column<long>(type: "bigint", nullable: false),
                    RowCount = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    PayloadSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PulledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AgentId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_change_set_audit_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_change_set_audit_log_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "change_sets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDatabase = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TableName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TabloId = table.Column<int>(type: "integer", nullable: false),
                    LastTriggerRecNo = table.Column<long>(type: "bigint", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                    PulledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_change_sets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_change_sets_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "parameter_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceDatabase = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ParametreProgram = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    ParametreUser = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    ParametreAnaGrubu = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ParametreAltGrubu = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ParametreID = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ParametreAdi = table.Column<string>(type: "character varying(127)", maxLength: 127, nullable: false),
                    ParametreDegeri = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parameter_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_parameter_records_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_change_set_audit_log_TenantId_IdempotencyKey_Direction",
                table: "change_set_audit_log",
                columns: new[] { "TenantId", "IdempotencyKey", "Direction" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_change_set_audit_log_TenantId_TableName_LastTriggerRecNo",
                table: "change_set_audit_log",
                columns: new[] { "TenantId", "TableName", "LastTriggerRecNo" });

            migrationBuilder.CreateIndex(
                name: "IX_change_set_audit_log_TenantId_TableName_ReceivedAtUtc",
                table: "change_set_audit_log",
                columns: new[] { "TenantId", "TableName", "ReceivedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_change_sets_TenantId_SourceDatabase_TableName_LastTriggerRe~",
                table: "change_sets",
                columns: new[] { "TenantId", "SourceDatabase", "TableName", "LastTriggerRecNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_change_sets_TenantId_TableName_PulledAtUtc",
                table: "change_sets",
                columns: new[] { "TenantId", "TableName", "PulledAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_parameter_records_TenantId_SourceDatabase_ParametreProgram_~",
                table: "parameter_records",
                columns: new[] { "TenantId", "SourceDatabase", "ParametreProgram", "ParametreUser", "ParametreID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "change_set_audit_log");

            migrationBuilder.DropTable(
                name: "change_sets");

            migrationBuilder.DropTable(
                name: "parameter_records");
        }
    }
}
