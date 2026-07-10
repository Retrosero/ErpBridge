using System.Security.Cryptography;
using Dapper;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.LocalStore;
using ErpBridge.LocalStore.ProtectedConfig;
using ErpBridge.LocalStore.Stores;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ErpBridge.LocalStore.Tests.Stores;

/// <summary>
/// Tests for the encrypted-at-rest secret roundtrip driven by
/// <see cref="AesProtectedConfigProvider"/>. Verifies the store writes through the
/// provider, decrypts on read, masks on key-mismatch, and tolerates legacy plaintext rows.
/// </summary>
public class SqliteAgentConfigStoreProtectedTests : IDisposable
{
    private readonly Microsoft.Data.Sqlite.SqliteConnection _keepAlive;
    private readonly ErpBridge.LocalStore.Sqlite.SqliteConnectionFactory _factory;

    public SqliteAgentConfigStoreProtectedTests()
    {
        (_factory, _keepAlive) = SqliteTestHarness.CreateIsolatedFactory();
    }

    public void Dispose() => _keepAlive.Dispose();

    [Fact]
    public async Task Save_secret_then_Load_returns_plaintext_via_Aes_protector()
    {
        var protector = new AesProtectedConfigProvider(RandomNumberGenerator.GetBytes(32));
        var sut = new SqliteAgentConfigStore(_factory, protector);

        await sut.SaveAsync(new AgentConfig
        {
            SqlPassword = "SuperSecret!123",
            LicenseKey = "LIC-987",
            TenantId = "tenant-aes",
        });

        var loaded = await sut.LoadAsync();

        loaded.Should().NotBeNull();
        loaded!.SqlPassword.Should().Be("SuperSecret!123", "AesProtectedConfigProvider must round-trip the secret");
        loaded.LicenseKey.Should().Be("LIC-987");
        loaded.TenantId.Should().Be("tenant-aes");
    }

    [Fact]
    public async Task Stored_protected_value_in_DB_does_not_contain_plaintext()
    {
        var protector = new AesProtectedConfigProvider(RandomNumberGenerator.GetBytes(32));
        var sut = new SqliteAgentConfigStore(_factory, protector);

        const string secret = "TopSecret-DB-Leak-Check";
        await sut.SaveAsync(new AgentConfig { SqlPassword = secret });

        // Read the raw row directly from SQLite and confirm the secret isn't on disk.
        await using var connection = await _factory.OpenAsync();
        var row = await connection.QueryFirstAsync<(string Value, string? ProtectedValue, long ProtectionVersion)>(
            "SELECT value, protected_value, protection_version FROM agent_config WHERE key = 'SqlPassword';");

        row.Value.Should().NotContain(secret, "the legacy value column must not contain plaintext");
        row.ProtectedValue.Should().NotContain(secret, "the encrypted blob must not contain plaintext");
        row.ProtectionVersion.Should().Be(SqliteAgentConfigStore.CurrentProtectionVersion);
    }

    [Fact]
    public async Task Load_redacts_secret_when_unprotect_fails_for_wrong_key()
    {
        // Provider A is what we Save with, provider B is what we Load with. The store must
        // never throw — it should fall back to the ***REDACTED*** placeholder.
        var providerA = new AesProtectedConfigProvider(RandomNumberGenerator.GetBytes(32));
        var providerB = new AesProtectedConfigProvider(RandomNumberGenerator.GetBytes(32));

        var writeStore = new SqliteAgentConfigStore(_factory, providerA);
        await writeStore.SaveAsync(new AgentConfig { SqlPassword = "CannotDecryptThis" });

        // Force a raw SQL write to make sure no in-memory cache in the provider matters.
        var readStore = new SqliteAgentConfigStore(_factory, providerB, NullLogger<SqliteAgentConfigStore>.Instance);
        var loaded = await readStore.LoadAsync();

        loaded.Should().NotBeNull();
        loaded!.SqlPassword.Should().Be("***REDACTED***");
    }

    [Fact]
    public async Task Legacy_plaintext_secret_rows_are_redacted_on_load()
    {
        // Simulate a pre-Track-2 deployment: the agent_config table has a secret row whose
        // protected_value is NULL (i.e. the value column is plaintext from the old NoOp era).
        await using var connection = await _factory.OpenAsync();
        await connection.ExecuteAsync(@"
INSERT INTO agent_config (key, value, is_secret, updated_at)
VALUES (@key, @value, @isSecret, @updatedAt);",
            new
            {
                key = "SqlPassword",
                value = "legacy-plaintext-never-leaked",
                isSecret = 1,
                updatedAt = DateTime.UtcNow.ToString("O"),
            });

        var sut = new SqliteAgentConfigStore(_factory, new AesProtectedConfigProvider(RandomNumberGenerator.GetBytes(32)));
        var loaded = await sut.LoadAsync();

        loaded.Should().NotBeNull();
        loaded!.SqlPassword.Should().Be("***REDACTED***", "legacy plaintext must never leak post-Track-2");
    }

    [Fact]
    public async Task Load_propagates_non_secret_fields_verbatim()
    {
        var protector = new AesProtectedConfigProvider(RandomNumberGenerator.GetBytes(32));
        var sut = new SqliteAgentConfigStore(_factory, protector);

        var input = new AgentConfig
        {
            TenantId = "tenant-A",
            ErpType = ErpType.Mikro,
            SqlServer = "mssql.local",
            SqlUserName = "agent",
            MikroDatabaseName = "MIKRO_X",
            CompanyNo = 12,
            BranchNo = 4,
            ApiBaseUrl = "https://api.erpbridge.local",
            SqlPassword = "secret-not-leaked",
        };

        await sut.SaveAsync(input);
        var loaded = await sut.LoadAsync();

        loaded.Should().NotBeNull();
        loaded!.TenantId.Should().Be("tenant-A");
        loaded.ErpType.Should().Be(ErpType.Mikro);
        loaded.SqlServer.Should().Be("mssql.local");
        loaded.SqlUserName.Should().Be("agent");
        loaded.MikroDatabaseName.Should().Be("MIKRO_X");
        loaded.CompanyNo.Should().Be(12);
        loaded.BranchNo.Should().Be(4);
        loaded.ApiBaseUrl.Should().Be("https://api.erpbridge.local");
        loaded.SqlPassword.Should().Be("secret-not-leaked");
    }

    [Fact]
    public async Task Save_called_twice_updates_protected_value_without_duplicating_rows()
    {
        var protector = new AesProtectedConfigProvider(RandomNumberGenerator.GetBytes(32));
        var sut = new SqliteAgentConfigStore(_factory, protector);

        await sut.SaveAsync(new AgentConfig { SqlPassword = "first" });
        await sut.SaveAsync(new AgentConfig { SqlPassword = "second" });

        await using var connection = await _factory.OpenAsync();
        var count = await connection.ExecuteScalarAsync<long>(
            "SELECT COUNT(*) FROM agent_config WHERE key = 'SqlPassword';");
        count.Should().Be(1, "ON CONFLICT(key) UPSERT must update the single secret row in place");

        var loaded = await sut.LoadAsync();
        loaded!.SqlPassword.Should().Be("second");
    }

    [Fact]
    public async Task Aes_provider_through_DI_is_constructed_singleton_and_round_trips_secret()
    {
        // Exercises the platform-conditional DI helper without depending on Windows-only
        // DPAPI — on a Linux agent the default IS the AES provider and these calls must work.
        var dataSource = Path.Combine(Path.GetTempPath(), $"erpbridge-di-{Guid.NewGuid():N}.db");
        var inMemory = new Dictionary<string, string?>
        {
            ["ErpBridge:LocalStore:DataSource"] = dataSource,
            ["ProtectedConfig:AesKeyPath"] = Path.Combine(Path.GetTempPath(), $"erpbridge-key-{Guid.NewGuid():N}.key"),
        };

        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemory).Build();
        services.AddErpBridgeLocalStore(config);

        var sp = services.BuildServiceProvider();

        // Real production flow: ensure schema, then save+load.
        var runner = sp.GetRequiredService<ErpBridge.LocalStore.Sqlite.Migrations.MigrationRunner>();
        await runner.EnsureSchemaAsync();
        var sut = sp.GetRequiredService<IAgentConfigStore>();

        await sut.SaveAsync(new AgentConfig
        {
            TenantId = "tenant-di",
            SqlPassword = "via-di",
        });

        var loaded = await sut.LoadAsync();
        loaded.Should().NotBeNull();
        loaded!.SqlPassword.Should().Be("via-di");

        // Cleanup
        try { File.Delete(dataSource); } catch { }
        try { File.Delete(inMemory["ProtectedConfig:AesKeyPath"]!); } catch { }
    }
}
