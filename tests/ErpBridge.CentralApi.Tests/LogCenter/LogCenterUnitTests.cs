using ErpBridge.CentralApi.LogCenter;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.LogCenter;

public sealed class LogScrubberTests
{
    [Theory]
    [InlineData("Server=erp;Database=MikroDB_V16;User Id=sa;Password=Gizli123;", "Gizli123")]
    [InlineData("connection failed: Pwd=hunter2", "hunter2")]
    [InlineData("Authorization: Bearer abc.def-ghi_jkl", "abc.def-ghi_jkl")]
    [InlineData("token eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjMifQ.c2lnbmF0dXJl rejected", "eyJhbGciOiJIUzI1NiJ9")]
    [InlineData("license AK-7F3K-99QX-ABCD invalid", "7F3K-99QX-ABCD")]
    [InlineData("{\"apiKey\":\"s3cr3t-value\",\"x\":1}", "s3cr3t-value")]
    [InlineData("mail ali.veli@firma.com.tr bounced", "ali.veli@firma.com.tr")]
    [InlineData("müşteri tel 0532 123 45 67 aranamadı", "123 45 67")]
    [InlineData("müşteri tel +905321234567", "5321234567")]
    [InlineData("TCKN 12345678901 geçersiz", "12345678901")]
    [InlineData("IBAN TR33 0006 1005 1978 6457 8413 26", "1978 6457")]
    [InlineData("Password=correct horse battery staple", "horse battery staple")]
    [InlineData("{\"password\":\"correct horse battery staple\",\"user\":\"x\"}", "horse battery staple")]
    [InlineData("{\"token\":\"a \\\"quoted\\\" secret\"}", "quoted")]
    [InlineData("Server=x;Pwd='with space';Database=y", "with space")]
    public void Scrub_removes_secrets_and_personal_data(string input, string secret)
    {
        var scrubbed = LogScrubber.Scrub(input);

        scrubbed.Should().NotContain(secret);
        scrubbed.Should().Contain("***");
    }

    [Theory]
    [InlineData("Sync of 1250 rows took 3400 ms")]
    [InlineData("HTTP 500 on /api/v1/android/sync/pull")]
    [InlineData("at ErpBridge.Core.Sync.AgentSyncLoop.RunAsync() in C:\\src\\AgentSyncLoop.cs:line 114")]
    [InlineData("job 1b4e28ba-2fa1-11d2-883f-12345678901a failed")]
    [InlineData("op-a53212345678901234567890abcdef00")]
    [InlineData("device 5321234567abc restarted")]
    public void Scrub_leaves_ordinary_diagnostics_alone(string input)
    {
        LogScrubber.Scrub(input).Should().Be(input);
    }

    [Fact]
    public void Masked_json_stays_valid_json()
    {
        var scrubbed = LogScrubber.Scrub("{\"password\":\"correct horse\",\"apiKey\":\"k 1\",\"n\":2}");

        System.Text.Json.JsonDocument.Parse(scrubbed).RootElement.GetProperty("n").GetInt32().Should().Be(2);
    }

    [Fact]
    public void Scrub_handles_null_and_empty()
    {
        LogScrubber.Scrub(null).Should().BeEmpty();
        LogScrubber.Scrub(string.Empty).Should().BeEmpty();
    }
}

public sealed class LogSeverityTests
{
    [Theory]
    [InlineData("WARNING", "WARN")]
    [InlineData("warn", "WARN")]
    [InlineData("Information", "INFO")]
    [InlineData("critical", "FATAL")]
    [InlineData("trace", "DEBUG")]
    [InlineData("error", "ERROR")]
    [InlineData(null, "INFO")]
    [InlineData("nonsense", "INFO")]
    public void Normalize_folds_producer_spellings(string? raw, string expected)
    {
        LogSeverity.Normalize(raw).Should().Be(expected);
    }

    [Fact]
    public void AtLeast_returns_the_severity_and_everything_above()
    {
        LogSeverity.AtLeast("warning").Should().Equal("WARN", "ERROR", "FATAL");
        LogSeverity.AtLeast("anything").Should().Equal("INFO", "WARN", "ERROR", "FATAL");
    }

    [Theory]
    [InlineData("desktop_exception", "DESKTOP_EXCEPTION")]
    [InlineData("sync round", "SYNC_ROUND")]
    [InlineData("şifre-hatası", "_IFRE_HATAS_")]
    [InlineData("  ", "UNKNOWN")]
    public void NormalizeKind_is_upper_case_ascii(string raw, string expected)
    {
        // ToUpperInvariant on a Turkish server would still turn "i" into "I"; the fold never depends on culture.
        LogEventWriter.NormalizeKind(raw).Should().Be(expected);
    }
}

public sealed class ErrorFingerprintTests
{
    private const string DotNetStack = """
        System.InvalidOperationException: boom
           at System.Linq.Enumerable.First[TSource](IEnumerable`1 source)
           at ErpBridge.Core.Sync.AgentSyncLoop.RunOnceAsync(CancellationToken ct) in C:\src\AgentSyncLoop.cs:line 114
           at ErpBridge.Core.Sync.AgentSyncLoop.RunAsync(CancellationToken ct) in C:\src\AgentSyncLoop.cs:line 90
        """;

    [Fact]
    public void Same_error_with_different_ids_and_numbers_shares_a_fingerprint()
    {
        var a = ErrorFingerprint.Compute("windows_agent", "SYNC_FAILED", "SqlException", "sync", "Timeout after 30000 ms for job 1b4e28ba-2fa1-11d2-883f-0016d3cca427 on 'MikroDB_V16_FIRMA1'", DotNetStack);
        var b = ErrorFingerprint.Compute("windows_agent", "SYNC_FAILED", "SqlException", "sync", "Timeout after 15000 ms for job 7d444840-9dc0-11d1-b245-5ffdce74fad2 on 'MikroDB_V15_BASKA'",
            DotNetStack.Replace("line 114", "line 120"));

        a.Fingerprint.Should().Be(b.Fingerprint);
        a.TopFrame.Should().Be("ErpBridge.Core.Sync.AgentSyncLoop.RunOnceAsync");
        a.NormalizedMessage.Should().Be("timeout after <n> ms for job <id> on <v>");
    }

    [Fact]
    public void Different_exception_type_or_source_is_a_different_group()
    {
        var baseline = ErrorFingerprint.Compute("android", "CRASH", "IllegalStateException", "load", "boom", string.Empty);

        ErrorFingerprint.Compute("android", "CRASH", "NullPointerException", "load", "boom", string.Empty).Fingerprint.Should().NotBe(baseline.Fingerprint);
        ErrorFingerprint.Compute("windows_agent", "CRASH", "IllegalStateException", "load", "boom", string.Empty).Fingerprint.Should().NotBe(baseline.Fingerprint);
    }

    [Fact]
    public void Jvm_frames_skip_framework_packages()
    {
        const string stack = """
            java.lang.IllegalStateException: boom
                at kotlinx.coroutines.DispatchedTask.run(DispatchedTask.kt:108)
                at com.example.data.sync.BridgeDeltaSync.applyPage(BridgeDeltaSync.kt:161)
            """;

        ErrorFingerprint.TopApplicationFrame(stack).Should().Be("com.example.data.sync.BridgeDeltaSync.applyPage");
    }
}

public sealed class LogCenterMigrationTests
{
    [Fact]
    public void Migration_exists_and_model_enforces_source_event_uniqueness()
    {
        var assembly = typeof(ErpBridge.CentralApi.Data.CentralApiDbContext).Assembly;
        assembly.GetTypes().Should().Contain(t => t.Name == "LogMerkeziL0LogEvents");

        var options = new DbContextOptionsBuilder<ErpBridge.CentralApi.Data.CentralApiDbContext>()
            .UseInMemoryDatabase("LogCenterModel_" + Guid.NewGuid().ToString("N")).Options;
        using var db = new ErpBridge.CentralApi.Data.CentralApiDbContext(options);
        var entity = db.Model.FindEntityType(typeof(ErpBridge.CentralApi.Domain.LogEvent))!;
        entity.GetIndexes().Should().Contain(i => i.IsUnique
            && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "Source", "EventId" }));
        db.Model.FindEntityType(typeof(ErpBridge.CentralApi.Domain.LogErrorGroup))!.GetIndexes()
            .Should().Contain(i => i.IsUnique && i.Properties.Single().Name == "Fingerprint");
    }

    [Fact]
    public void Trigram_index_creates_the_extension_first_and_cannot_fail_the_migration()
    {
        var sql = LogCenterMigrationSql.MessageTrigramIndex;
        sql.IndexOf("CREATE EXTENSION IF NOT EXISTS pg_trgm", StringComparison.Ordinal)
            .Should().BeLessThan(sql.IndexOf("gin_trgm_ops", StringComparison.Ordinal));
        sql.Should().Contain("EXCEPTION WHEN OTHERS");
        LogCenterMigrationSql.CopyLegacyTelemetry.Should().Contain("ON CONFLICT DO NOTHING");
    }
}
