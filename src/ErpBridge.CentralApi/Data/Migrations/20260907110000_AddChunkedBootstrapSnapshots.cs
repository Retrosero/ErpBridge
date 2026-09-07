using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations;

public partial class AddChunkedBootstrapSnapshots : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "bootstrap_snapshots",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                SourceDatabase = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                PulledAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ActivatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                IsIncremental = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_bootstrap_snapshots", x => x.Id);
                table.ForeignKey("FK_bootstrap_snapshots_tenants_TenantId", x => x.TenantId, "tenants", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "bootstrap_snapshot_chunks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                SnapshotId = table.Column<Guid>(type: "uuid", nullable: false),
                Section = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                ChunkIndex = table.Column<int>(type: "integer", nullable: false),
                ItemCount = table.Column<int>(type: "integer", nullable: false),
                PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
                ReceivedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_bootstrap_snapshot_chunks", x => x.Id);
                table.ForeignKey("FK_bootstrap_snapshot_chunks_bootstrap_snapshots_SnapshotId", x => x.SnapshotId, "bootstrap_snapshots", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_bootstrap_snapshots_TenantId_PulledAtUtc", "bootstrap_snapshots", new[] { "TenantId", "PulledAtUtc" });
        migrationBuilder.CreateIndex("IX_bootstrap_snapshots_TenantId_Active", "bootstrap_snapshots", "TenantId", unique: true, filter: "\"IsActive\" = true");
        migrationBuilder.CreateIndex("IX_bootstrap_snapshot_chunks_SnapshotId_Section_ChunkIndex", "bootstrap_snapshot_chunks", new[] { "SnapshotId", "Section", "ChunkIndex" }, unique: true);

        // Best-effort, server-side migration of the newest legacy package per tenant.
        // Each top-level JSON array becomes one bounded section row.
        migrationBuilder.Sql(@"
INSERT INTO bootstrap_snapshots (""Id"", ""TenantId"", ""SourceDatabase"", ""PulledAtUtc"", ""ReceivedAtUtc"", ""ActivatedAtUtc"", ""IsActive"")
SELECT DISTINCT ON (p.""TenantId"") gen_random_uuid(), p.""TenantId"", p.""SourceDatabase"", p.""PulledAtUtc"", p.""ReceivedAtUtc"", p.""ReceivedAtUtc"", true, false
FROM bootstrap_packages p
ORDER BY p.""TenantId"", p.""PulledAtUtc"" DESC, p.""ReceivedAtUtc"" DESC;

INSERT INTO bootstrap_snapshot_chunks (""Id"", ""SnapshotId"", ""Section"", ""ChunkIndex"", ""ItemCount"", ""PayloadJson"", ""ReceivedAtUtc"")
SELECT gen_random_uuid(), s.""Id"", e.key, 0,
       CASE WHEN jsonb_typeof(e.value) = 'array' THEN jsonb_array_length(e.value) ELSE 0 END,
       CASE WHEN jsonb_typeof(e.value) = 'array' THEN e.value ELSE '[]'::jsonb END,
       s.""ReceivedAtUtc""
FROM bootstrap_snapshots s
JOIN LATERAL (SELECT p.""PayloadJson"" FROM bootstrap_packages p WHERE p.""TenantId"" = s.""TenantId"" ORDER BY p.""PulledAtUtc"" DESC, p.""ReceivedAtUtc"" DESC LIMIT 1) p ON true
CROSS JOIN LATERAL jsonb_each(p.""PayloadJson"") e
WHERE jsonb_typeof(e.value) = 'array';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("bootstrap_snapshot_chunks");
        migrationBuilder.DropTable("bootstrap_snapshots");
    }
}
