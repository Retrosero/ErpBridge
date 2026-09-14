using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Faz38ApprovalCentre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanApprove",
                table: "mobile_users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageApprovalRules",
                table: "mobile_users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "approval_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Kind = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CounterpartyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SummaryJson = table.Column<string>(type: "jsonb", nullable: false),
                    DocumentsJson = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ReplacesRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RequestedSeq = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedSeq = table.Column<long>(type: "bigint", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approval_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_approval_requests_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_approval_rules",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sale = table.Column<bool>(type: "boolean", nullable: false),
                    Purchase = table.Column<bool>(type: "boolean", nullable: false),
                    Return = table.Column<bool>(type: "boolean", nullable: false),
                    Collection = table.Column<bool>(type: "boolean", nullable: false),
                    Disbursement = table.Column<bool>(type: "boolean", nullable: false),
                    StockCount = table.Column<bool>(type: "boolean", nullable: false),
                    ProductCard = table.Column<bool>(type: "boolean", nullable: false),
                    CustomerCard = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_approval_rules", x => x.TenantId);
                    table.ForeignKey(
                        name: "FK_tenant_approval_rules_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "approval_request_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    ByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    AtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AtSeq = table.Column<long>(type: "bigint", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_approval_request_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_approval_request_events_approval_requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "approval_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_approval_request_events_RequestId_AtSeq",
                table: "approval_request_events",
                columns: new[] { "RequestId", "AtSeq" });

            migrationBuilder.CreateIndex(
                name: "IX_approval_requests_TenantId_ExternalId",
                table: "approval_requests",
                columns: new[] { "TenantId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_approval_requests_TenantId_Status_RequestedSeq",
                table: "approval_requests",
                columns: new[] { "TenantId", "Status", "RequestedSeq" });

            migrationBuilder.CreateIndex(
                name: "IX_approval_requests_TenantId_UpdatedSeq",
                table: "approval_requests",
                columns: new[] { "TenantId", "UpdatedSeq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "approval_request_events");

            migrationBuilder.DropTable(
                name: "tenant_approval_rules");

            migrationBuilder.DropTable(
                name: "approval_requests");

            migrationBuilder.DropColumn(
                name: "CanApprove",
                table: "mobile_users");

            migrationBuilder.DropColumn(
                name: "CanManageApprovalRules",
                table: "mobile_users");
        }
    }
}
