using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class GorevZiyaretHatirlatma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "VisitReminder",
                table: "tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "VisitReminderFromMs",
                table: "tasks",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "VisitReminder",
                table: "task_series",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisitReminder",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "VisitReminderFromMs",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "VisitReminder",
                table: "task_series");
        }
    }
}
