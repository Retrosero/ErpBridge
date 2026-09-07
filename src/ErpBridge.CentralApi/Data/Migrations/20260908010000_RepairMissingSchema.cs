using ErpBridge.CentralApi.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations;

/// <summary>Repairs production databases whose migration history was recorded before the tables were created.</summary>
[DbContext(typeof(CentralApiDbContext))]
[Migration("20260908010000_RepairMissingSchema")]
public partial class RepairMissingSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
ALTER TABLE api_keys ADD COLUMN IF NOT EXISTS ""VaultCiphertext"" bytea NULL;
ALTER TABLE api_keys ADD COLUMN IF NOT EXISTS ""VaultNonce"" bytea NULL;
ALTER TABLE api_keys ADD COLUMN IF NOT EXISTS ""VaultTag"" bytea NULL;

CREATE TABLE IF NOT EXISTS api_key_secret_access_audits (
    ""Id"" uuid NOT NULL PRIMARY KEY,
    ""ApiKeyId"" uuid NOT NULL REFERENCES api_keys(""Id"") ON DELETE CASCADE,
    ""AdminUserId"" uuid NOT NULL,
    ""AccessedAtUtc"" timestamp with time zone NOT NULL,
    ""Action"" character varying(32) NOT NULL,
    ""RemoteIp"" character varying(64) NULL);
CREATE INDEX IF NOT EXISTS ""IX_api_key_secret_access_audits_AdminUserId"" ON api_key_secret_access_audits (""AdminUserId"");
CREATE INDEX IF NOT EXISTS ""IX_api_key_secret_access_audits_ApiKeyId_AccessedAtUtc"" ON api_key_secret_access_audits (""ApiKeyId"", ""AccessedAtUtc"");

CREATE TABLE IF NOT EXISTS bootstrap_snapshots (
    ""Id"" uuid NOT NULL PRIMARY KEY,
    ""TenantId"" uuid NOT NULL REFERENCES tenants(""Id"") ON DELETE CASCADE,
    ""SourceDatabase"" character varying(128) NOT NULL,
    ""PulledAtUtc"" timestamp with time zone NOT NULL,
    ""ReceivedAtUtc"" timestamp with time zone NOT NULL,
    ""ActivatedAtUtc"" timestamp with time zone NOT NULL,
    ""IsActive"" boolean NOT NULL,
    ""IsIncremental"" boolean NOT NULL);
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_bootstrap_snapshots_TenantId_Active"" ON bootstrap_snapshots (""TenantId"") WHERE ""IsActive"" = true;
CREATE INDEX IF NOT EXISTS ""IX_bootstrap_snapshots_TenantId_PulledAtUtc"" ON bootstrap_snapshots (""TenantId"", ""PulledAtUtc"");

CREATE TABLE IF NOT EXISTS bootstrap_snapshot_chunks (
    ""Id"" uuid NOT NULL PRIMARY KEY,
    ""SnapshotId"" uuid NOT NULL REFERENCES bootstrap_snapshots(""Id"") ON DELETE CASCADE,
    ""Section"" character varying(64) NOT NULL,
    ""ChunkIndex"" integer NOT NULL,
    ""ItemCount"" integer NOT NULL,
    ""PayloadJson"" jsonb NOT NULL,
    ""ReceivedAtUtc"" timestamp with time zone NOT NULL);
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_bootstrap_snapshot_chunks_SnapshotId_Section_ChunkIndex"" ON bootstrap_snapshot_chunks (""SnapshotId"", ""Section"", ""ChunkIndex"");");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Recovery migration: existing production data must never be dropped by rollback.
    }
}
