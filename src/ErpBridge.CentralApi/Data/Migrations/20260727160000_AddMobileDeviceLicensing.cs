using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace ErpBridge.CentralApi.Data.Migrations;

[Migration("20260727160000_AddMobileDeviceLicensing")]
public partial class AddMobileDeviceLicensing : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name: "DeviceSeatLimit", table: "tenants", type: "integer", nullable: false, defaultValue: 5);
        migrationBuilder.CreateTable(name: "mobile_devices", columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false), TenantId = table.Column<Guid>(type: "uuid", nullable: false), InstallationId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false), DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false), Platform = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true), AppVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true), ActivatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false), LastSeenAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false), RevokedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true), IsActive = table.Column<bool>(type: "boolean", nullable: false)
        }, constraints: table => { table.PrimaryKey("PK_mobile_devices", x => x.Id); table.ForeignKey("FK_mobile_devices_tenants_TenantId", x => x.TenantId, "tenants", "Id", onDelete: ReferentialAction.Cascade); });
        migrationBuilder.CreateTable(name: "device_activation_codes", columns: table => new
        {
            Id = table.Column<Guid>(type: "uuid", nullable: false), TenantId = table.Column<Guid>(type: "uuid", nullable: false), CodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false), CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false), ExpiresAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false), ConsumedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true), DeviceId = table.Column<Guid>(type: "uuid", nullable: true)
        }, constraints: table => { table.PrimaryKey("PK_device_activation_codes", x => x.Id); table.ForeignKey("FK_device_activation_codes_tenants_TenantId", x => x.TenantId, "tenants", "Id", onDelete: ReferentialAction.Cascade); });
        migrationBuilder.CreateIndex(name: "IX_mobile_devices_TenantId_InstallationId", table: "mobile_devices", columns: new[] { "TenantId", "InstallationId" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_mobile_devices_TenantId_IsActive", table: "mobile_devices", columns: new[] { "TenantId", "IsActive" });
        migrationBuilder.CreateIndex(name: "IX_device_activation_codes_CodeHash", table: "device_activation_codes", column: "CodeHash", unique: true);
        migrationBuilder.CreateIndex(name: "IX_device_activation_codes_TenantId_ExpiresAtUtc", table: "device_activation_codes", columns: new[] { "TenantId", "ExpiresAtUtc" });
    }
    protected override void Down(MigrationBuilder migrationBuilder) { migrationBuilder.DropTable("device_activation_codes"); migrationBuilder.DropTable("mobile_devices"); migrationBuilder.DropColumn("DeviceSeatLimit", "tenants"); }
}
