using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class ErpYazimY1eJobLease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LeasedUntilMs",
                table: "jobs",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NextAttemptAtMs",
                table: "jobs",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeasedUntilMs",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "NextAttemptAtMs",
                table: "jobs");
        }
    }
}
