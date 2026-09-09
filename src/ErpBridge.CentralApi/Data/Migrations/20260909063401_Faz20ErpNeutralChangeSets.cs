using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Faz20ErpNeutralChangeSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TabloId",
                table: "change_sets");

            migrationBuilder.DropColumn(
                name: "TabloId",
                table: "change_set_audit_log");

            migrationBuilder.AddColumn<string>(
                name: "SourceRecordKey",
                table: "mobile_sync_queue",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErpType",
                table: "jobs",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Mikro");

            migrationBuilder.AddColumn<string>(
                name: "ErpType",
                table: "change_sets",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Mikro");

            migrationBuilder.AddColumn<string>(
                name: "TableKey",
                table: "change_sets",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ErpType",
                table: "change_set_audit_log",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Mikro");

            migrationBuilder.AddColumn<string>(
                name: "TableKey",
                table: "change_set_audit_log",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_mobile_sync_queue_TenantId_SourceDatabase_TableName_SourceR~",
                table: "mobile_sync_queue",
                columns: new[] { "TenantId", "SourceDatabase", "TableName", "SourceRecordKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_mobile_sync_queue_TenantId_SourceDatabase_TableName_SourceR~",
                table: "mobile_sync_queue");

            migrationBuilder.DropColumn(
                name: "SourceRecordKey",
                table: "mobile_sync_queue");

            migrationBuilder.DropColumn(
                name: "ErpType",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "ErpType",
                table: "change_sets");

            migrationBuilder.DropColumn(
                name: "TableKey",
                table: "change_sets");

            migrationBuilder.DropColumn(
                name: "ErpType",
                table: "change_set_audit_log");

            migrationBuilder.DropColumn(
                name: "TableKey",
                table: "change_set_audit_log");

            migrationBuilder.AddColumn<int>(
                name: "TabloId",
                table: "change_sets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TabloId",
                table: "change_set_audit_log",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
