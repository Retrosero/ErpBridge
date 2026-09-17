using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class LogMerkeziL3gApprovalCorrelationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "approval_requests",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "approval_requests");
        }
    }
}
