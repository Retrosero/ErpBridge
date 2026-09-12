using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class Faz29MobileRecordSourceKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceRecordKey",
                table: "mobile_records",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_mobile_records_TenantId_Entity_SourceRecordKey",
                table: "mobile_records",
                columns: new[] { "TenantId", "Entity", "SourceRecordKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_mobile_records_TenantId_Entity_SourceRecordKey",
                table: "mobile_records");

            migrationBuilder.DropColumn(
                name: "SourceRecordKey",
                table: "mobile_records");
        }
    }
}
