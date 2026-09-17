namespace ErpBridge.CentralApi.LogCenter;

/// <summary>
/// PostgreSQL-only statements of the Log Merkezi L0 migration, kept outside the generated file so the
/// migration can be regenerated without losing them.
/// </summary>
public static class LogCenterMigrationSql
{
    /// <summary>
    /// D2: copy the last 90 days of the legacy telemetry table once. The legacy table itself is not touched.
    /// Severity and kind are folded the way <see cref="LogEventWriter"/> folds them.
    /// </summary>
    public const string CopyLegacyTelemetry = """
        INSERT INTO log_events ("Id", "EventId", "Source", "TenantId", "OccurredAtUtc", "OccurredAtMs",
            "ReceivedAtUtc", "ReceivedAtMs", "Severity", "Kind", "Operation", "Screen", "Message", "ExceptionType",
            "StackTrace", "AppVersion", "OsVersion", "DeviceModel", "HttpMethod", "HttpRoute", "HttpStatus",
            "CorrelationId", "RepeatCount", "PropertiesJson", "BreadcrumbsJson")
        SELECT gen_random_uuid(), t."EventId",
            CASE WHEN lower(t."Kind") LIKE 'desktop%' THEN 'windows_agent' ELSE 'android' END,
            t."TenantId", t."OccurredAtUtc", floor(extract(epoch FROM t."OccurredAtUtc") * 1000)::bigint,
            t."ReceivedAtUtc", floor(extract(epoch FROM t."ReceivedAtUtc") * 1000)::bigint,
            CASE upper(t."Severity")
                WHEN 'TRACE' THEN 'DEBUG' WHEN 'VERBOSE' THEN 'DEBUG' WHEN 'DEBUG' THEN 'DEBUG'
                WHEN 'WARN' THEN 'WARN' WHEN 'WARNING' THEN 'WARN'
                WHEN 'ERROR' THEN 'ERROR'
                WHEN 'FATAL' THEN 'FATAL' WHEN 'CRITICAL' THEN 'FATAL'
                ELSE 'INFO' END,
            left(regexp_replace(upper(t."Kind"), '[^A-Z0-9_]', '_', 'g'), 64),
            t."Operation", t."Screen", t."Message", t."ExceptionType", t."StackTrace", t."AppVersion",
            t."AndroidVersion", t."DeviceModel", t."HttpMethod", t."HttpRoute", t."HttpStatus", t."CorrelationId",
            1, '{}'::jsonb, COALESCE(t."BreadcrumbsJson", '[]'::jsonb)
        FROM mobile_telemetry_events t
        WHERE t."ReceivedAtUtc" >= now() - interval '90 days'
        ON CONFLICT DO NOTHING;
        """;

    /// <summary>
    /// D15: trigram index for message search. pg_trgm is a trusted extension (PG 13+), but a server that
    /// refuses it must not fail the whole migration — search then scans without the index.
    /// </summary>
    public const string MessageTrigramIndex = """
        DO $$
        BEGIN
            CREATE EXTENSION IF NOT EXISTS pg_trgm;
            CREATE INDEX IF NOT EXISTS "IX_log_events_Message_trgm" ON log_events USING gin ("Message" gin_trgm_ops);
        EXCEPTION WHEN OTHERS THEN
            RAISE NOTICE 'pg_trgm unavailable, log message search index skipped: %', SQLERRM;
        END
        $$;
        """;
}
