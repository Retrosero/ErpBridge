using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Faz47OrderFulfillment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_fulfillments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceJobId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovalRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    OrderNo = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SalespersonUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SalespersonName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LineCount = table.Column<int>(type: "integer", nullable: false),
                    ItemQuantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    ItemsJson = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    QueuedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    QueuedSeq = table.Column<long>(type: "bigint", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PackedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LoadedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AssigneeUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssigneeName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    VehiclePlate = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ErpState = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    UpdatedSeq = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_fulfillments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_fulfillments_jobs_SourceJobId",
                        column: x => x.SourceJobId,
                        principalTable: "jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_order_fulfillments_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_warehouse_settings",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    PendingWarnMinutes = table.Column<int>(type: "integer", nullable: false),
                    PendingCriticalMinutes = table.Column<int>(type: "integer", nullable: false),
                    PreparingWarnMinutes = table.Column<int>(type: "integer", nullable: false),
                    PreparingCriticalMinutes = table.Column<int>(type: "integer", nullable: false),
                    PackedWarnMinutes = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenant_warehouse_settings", x => x.TenantId);
                    table.ForeignKey(
                        name: "FK_tenant_warehouse_settings_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_fulfillment_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FulfillmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromStatus = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    ToStatus = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Action = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OccurredAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_fulfillment_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_fulfillment_events_order_fulfillments_FulfillmentId",
                        column: x => x.FulfillmentId,
                        principalTable: "order_fulfillments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_order_fulfillment_events_FulfillmentId",
                table: "order_fulfillment_events",
                column: "FulfillmentId");

            migrationBuilder.CreateIndex(
                name: "IX_order_fulfillment_events_TenantId_ActorUserId_OccurredAtUtc",
                table: "order_fulfillment_events",
                columns: new[] { "TenantId", "ActorUserId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_order_fulfillment_events_TenantId_FulfillmentId_OccurredAtU~",
                table: "order_fulfillment_events",
                columns: new[] { "TenantId", "FulfillmentId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_order_fulfillments_SourceJobId",
                table: "order_fulfillments",
                column: "SourceJobId");

            migrationBuilder.CreateIndex(
                name: "IX_order_fulfillments_TenantId_AssigneeUserId_PackedAtUtc",
                table: "order_fulfillments",
                columns: new[] { "TenantId", "AssigneeUserId", "PackedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_order_fulfillments_TenantId_SourceJobId",
                table: "order_fulfillments",
                columns: new[] { "TenantId", "SourceJobId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_order_fulfillments_TenantId_Status_QueuedSeq",
                table: "order_fulfillments",
                columns: new[] { "TenantId", "Status", "QueuedSeq" });

            migrationBuilder.CreateIndex(
                name: "IX_order_fulfillments_TenantId_UpdatedSeq",
                table: "order_fulfillments",
                columns: new[] { "TenantId", "UpdatedSeq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_fulfillment_events");

            migrationBuilder.DropTable(
                name: "tenant_warehouse_settings");

            migrationBuilder.DropTable(
                name: "order_fulfillments");
        }
    }
}
