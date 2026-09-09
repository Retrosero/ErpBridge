using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Faz20DeleteHighWaterMark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_change_sets_TenantId_SourceDatabase_TableName_LastTriggerRe~",
                table: "change_sets");

            migrationBuilder.AddColumn<long>(
                name: "LastDeleteRecNo",
                table: "change_sets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_change_sets_TenantId_SourceDatabase_TableName_LastTriggerRe~",
                table: "change_sets",
                columns: new[] { "TenantId", "SourceDatabase", "TableName", "LastTriggerRecNo", "LastDeleteRecNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_change_sets_TenantId_SourceDatabase_TableName_LastTriggerRe~",
                table: "change_sets");

            migrationBuilder.DropColumn(
                name: "LastDeleteRecNo",
                table: "change_sets");

            migrationBuilder.CreateIndex(
                name: "IX_change_sets_TenantId_SourceDatabase_TableName_LastTriggerRe~",
                table: "change_sets",
                columns: new[] { "TenantId", "SourceDatabase", "TableName", "LastTriggerRecNo" },
                unique: true);
        }
    }
}
