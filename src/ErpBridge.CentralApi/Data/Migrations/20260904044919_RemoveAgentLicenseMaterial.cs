using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAgentLicenseMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicenseKey",
                table: "agents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LicenseKey",
                table: "agents",
                type: "text",
                nullable: true);
        }
    }
}
