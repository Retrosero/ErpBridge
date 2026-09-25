using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class GorevlerVeBildirimler : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "task_ops_applied",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppliedAtMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_ops_applied", x => new { x.TenantId, x.OpId });
                });

            migrationBuilder.CreateTable(
                name: "task_series",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Priority = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RequiresPhoto = table.Column<bool>(type: "boolean", nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AssigneesJson = table.Column<string>(type: "text", nullable: false),
                    FollowersJson = table.Column<string>(type: "text", nullable: false),
                    SubtasksJson = table.Column<string>(type: "text", nullable: false),
                    Frequency = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Interval = table.Column<int>(type: "integer", nullable: false),
                    Weekdays = table.Column<int>(type: "integer", nullable: false),
                    MonthDay = table.Column<int>(type: "integer", nullable: false),
                    TimeOfDayMinutes = table.Column<int>(type: "integer", nullable: false),
                    DueAfterMinutes = table.Column<int>(type: "integer", nullable: true),
                    NextRunAtMs = table.Column<long>(type: "bigint", nullable: false),
                    EndsAtMs = table.Column<long>(type: "bigint", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedSeq = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_series", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_series_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Priority = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    CreatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    StartAtMs = table.Column<long>(type: "bigint", nullable: true),
                    DueAtMs = table.Column<long>(type: "bigint", nullable: true),
                    CompletedAtMs = table.Column<long>(type: "bigint", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CompletedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    RequiresPhoto = table.Column<bool>(type: "boolean", nullable: false),
                    CustomerCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SeriesId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartNotifiedAtMs = table.Column<long>(type: "bigint", nullable: true),
                    DueSoonNotifiedAtMs = table.Column<long>(type: "bigint", nullable: true),
                    OverdueNotifiedAtMs = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtMs = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedSeq = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tasks_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    ReadAtMs = table.Column<long>(type: "bigint", nullable: true),
                    Seq = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_notifications_tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SizeBytes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAtMs = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_attachments_tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_comments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAtMs = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_comments_tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_events",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Detail = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OccurredAtMs = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_events_tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_members",
                columns: table => new
                {
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    UserName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_members", x => new { x.TaskId, x.UserId, x.Role });
                    table.ForeignKey(
                        name: "FK_task_members_tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_subtasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    IsDone = table.Column<bool>(type: "boolean", nullable: false),
                    DoneByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoneByName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    DoneAtMs = table.Column<long>(type: "bigint", nullable: true),
                    AssigneeUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssigneeName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    DueAtMs = table.Column<long>(type: "bigint", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_subtasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_task_subtasks_tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_attachment_blobs",
                columns: table => new
                {
                    AttachmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_task_attachment_blobs", x => x.AttachmentId);
                    table.ForeignKey(
                        name: "FK_task_attachment_blobs_task_attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "task_attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_task_attachments_TaskId_CreatedAtMs",
                table: "task_attachments",
                columns: new[] { "TaskId", "CreatedAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_task_attachments_TenantId_IsDeleted",
                table: "task_attachments",
                columns: new[] { "TenantId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_task_comments_TaskId_CreatedAtMs",
                table: "task_comments",
                columns: new[] { "TaskId", "CreatedAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_task_events_TaskId_OccurredAtMs",
                table: "task_events",
                columns: new[] { "TaskId", "OccurredAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_task_members_UserId",
                table: "task_members",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_task_ops_applied_AppliedAtMs",
                table: "task_ops_applied",
                column: "AppliedAtMs");

            migrationBuilder.CreateIndex(
                name: "IX_task_series_IsActive_NextRunAtMs",
                table: "task_series",
                columns: new[] { "IsActive", "NextRunAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_task_series_TenantId_UpdatedSeq",
                table: "task_series",
                columns: new[] { "TenantId", "UpdatedSeq" });

            migrationBuilder.CreateIndex(
                name: "IX_task_subtasks_AssigneeUserId",
                table: "task_subtasks",
                column: "AssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_task_subtasks_TaskId_SortOrder",
                table: "task_subtasks",
                columns: new[] { "TaskId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_tasks_Status_DueAtMs",
                table: "tasks",
                columns: new[] { "Status", "DueAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_tasks_Status_StartAtMs",
                table: "tasks",
                columns: new[] { "Status", "StartAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_tasks_TenantId_CustomerCode",
                table: "tasks",
                columns: new[] { "TenantId", "CustomerCode" });

            migrationBuilder.CreateIndex(
                name: "IX_tasks_TenantId_UpdatedSeq",
                table: "tasks",
                columns: new[] { "TenantId", "UpdatedSeq" });

            migrationBuilder.CreateIndex(
                name: "IX_user_notifications_TenantId_UserId_ReadAtMs",
                table: "user_notifications",
                columns: new[] { "TenantId", "UserId", "ReadAtMs" });

            migrationBuilder.CreateIndex(
                name: "IX_user_notifications_TenantId_UserId_Seq",
                table: "user_notifications",
                columns: new[] { "TenantId", "UserId", "Seq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_attachment_blobs");

            migrationBuilder.DropTable(
                name: "task_comments");

            migrationBuilder.DropTable(
                name: "task_events");

            migrationBuilder.DropTable(
                name: "task_members");

            migrationBuilder.DropTable(
                name: "task_ops_applied");

            migrationBuilder.DropTable(
                name: "task_series");

            migrationBuilder.DropTable(
                name: "task_subtasks");

            migrationBuilder.DropTable(
                name: "user_notifications");

            migrationBuilder.DropTable(
                name: "task_attachments");

            migrationBuilder.DropTable(
                name: "tasks");
        }
    }
}
