using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBridge.CentralApi.Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(CentralApiDbContext))]
[Migration("20260720164000_EnsureMobileReadOnApiKeys")]
public partial class EnsureMobileReadOnApiKeys : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE api_keys
            SET "Scopes" = array_append(COALESCE("Scopes", '{}'::text[]), 'mobile:read')
            WHERE NOT ('mobile:read' = ANY(COALESCE("Scopes", '{}'::text[])));

            CREATE OR REPLACE FUNCTION ensure_mobile_read_scope()
            RETURNS trigger
            LANGUAGE plpgsql
            AS $$
            BEGIN
              IF NEW."Scopes" IS NULL THEN
                NEW."Scopes" := ARRAY['mobile:read']::text[];
              ELSIF NOT ('mobile:read' = ANY(NEW."Scopes")) THEN
                NEW."Scopes" := array_append(NEW."Scopes", 'mobile:read');
              END IF;
              RETURN NEW;
            END;
            $$;

            DROP TRIGGER IF EXISTS trg_api_keys_mobile_read ON api_keys;
            CREATE TRIGGER trg_api_keys_mobile_read
            BEFORE INSERT OR UPDATE OF "Scopes" ON api_keys
            FOR EACH ROW
            EXECUTE FUNCTION ensure_mobile_read_scope();
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP TRIGGER IF EXISTS trg_api_keys_mobile_read ON api_keys;
            DROP FUNCTION IF EXISTS ensure_mobile_read_scope();
            """);
    }
}
