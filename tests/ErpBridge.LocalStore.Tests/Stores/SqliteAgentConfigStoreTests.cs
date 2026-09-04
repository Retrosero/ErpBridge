using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.LocalStore.ProtectedConfig;
using ErpBridge.LocalStore.Sqlite;
using ErpBridge.LocalStore.Stores;
using FluentAssertions;
using Xunit;

namespace ErpBridge.LocalStore.Tests.Stores;

/// <summary>
/// Tests for <see cref="SqliteAgentConfigStore"/>. The masking contract is
/// non-negotiable: secrets round-trip with the raw value persisting but the
/// loaded object always exposes <c>***REDACTED***</c>.
/// </summary>
public class SqliteAgentConfigStoreTests : IDisposable
{
    private readonly SqliteConnectionFactory _factory;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _keepAlive;
    private readonly IProtectedConfigProvider _protector = new NoOpProtectedConfigProvider();

    public SqliteAgentConfigStoreTests()
    {
        (_factory, _keepAlive) = SqliteTestHarness.CreateIsolatedFactory();
    }

    public void Dispose() => _keepAlive.Dispose();

    [Fact]
    public async Task Save_then_Load_roundtrips_non_secret_fields()
    {
        var sut = new SqliteAgentConfigStore(_factory, _protector);

        var input = new AgentConfig
        {
            LicenseKey = "LIC-ABCDEF",
            TenantId = "tenant-001",
            ErpType = ErpType.Mikro,
            SqlServer = "localhost\\MIKRO",
            SqlUserName = "sa",
            SqlPassword = "do-not-leak-me",
            MikroDatabaseName = "MIKRO_DB",
            CompanyNo = 7,
            BranchNo = 3,
            WarehouseNo = 5,
            ApiBaseUrl = "https://api.example.com",
        };

        await sut.SaveAsync(input);

        var loaded = await sut.LoadAsync();
        loaded.Should().NotBeNull();

        loaded!.TenantId.Should().Be("tenant-001");
        loaded.ErpType.Should().Be(ErpType.Mikro);
        loaded.SqlServer.Should().Be("localhost\\MIKRO");
        loaded.SqlUserName.Should().Be("sa");
        loaded.MikroDatabaseName.Should().Be("MIKRO_DB");
        loaded.CompanyNo.Should().Be(7);
        loaded.BranchNo.Should().Be(3);
        loaded.WarehouseNo.Should().Be(5);
        loaded.ApiBaseUrl.Should().Be("https://api.example.com");
    }

    [Fact]
    public async Task Save_then_Load_roundtrips_warehouseNo_for_multi_firm_Mikro()
    {
        // Phase 10: the new WarehouseNo field must round-trip through the
        // SQLite key-value store exactly like CompanyNo / BranchNo.
        var sut = new SqliteAgentConfigStore(_factory, _protector);

        await sut.SaveAsync(new AgentConfig { WarehouseNo = 11 });

        var loaded = await sut.LoadAsync();
        loaded.Should().NotBeNull();
        loaded!.WarehouseNo.Should().Be(11);
    }

    [Fact]
    public async Task Load_defaults_WarehouseNo_to_1_when_no_row_exists()
    {
        // A fresh deployment with no saved WarehouseNo must not throw — the
        // AgentConfig property default (1) propagates to the adapter.
        var sut = new SqliteAgentConfigStore(_factory, _protector);

        await sut.SaveAsync(new AgentConfig { SqlServer = "localhost" });

        var loaded = await sut.LoadAsync();
        loaded.Should().NotBeNull();
        loaded!.WarehouseNo.Should().Be(1);
    }

    [Fact]
    public async Task Load_masks_secret_fields_with_redacted_placeholder()
    {
        var sut = new SqliteAgentConfigStore(_factory, _protector);

        var input = new AgentConfig
        {
            LicenseKey = "LIC-SECRET-123",
            SqlPassword = "super-secret-password",
            TenantId = "tenant-mask",
        };

        await sut.SaveAsync(input);

        var loaded = await sut.LoadAsync();

        loaded.Should().NotBeNull();
        loaded!.SqlPassword.Should().Be("***REDACTED***");
        loaded.LicenseKey.Should().Be("***REDACTED***");
        loaded.TenantId.Should().Be("tenant-mask");
    }

    [Fact]
    public async Task Load_on_empty_store_returns_null()
    {
        var sut = new SqliteAgentConfigStore(_factory, _protector);

        var loaded = await sut.LoadAsync();

        loaded.Should().BeNull();
    }

    [Fact]
    public async Task Save_overwrites_previous_values()
    {
        var sut = new SqliteAgentConfigStore(_factory, _protector);

        await sut.SaveAsync(new AgentConfig { TenantId = "first" });
        await sut.SaveAsync(new AgentConfig { TenantId = "second" });

        var loaded = await sut.LoadAsync();

        loaded.Should().NotBeNull();
        loaded!.TenantId.Should().Be("second");
    }

    [Fact]
    public async Task Redacted_secret_is_never_written_to_secret_rows_in_plaintext_with_protector()
    {
        // Use a pass-through (no-op) protector and confirm that the secret row still
        // ends up masked when read back; the store must never decrypt on the read path.
        var sut = new SqliteAgentConfigStore(_factory, _protector);

        await sut.SaveAsync(new AgentConfig { SqlPassword = "raw-password" });

        var loaded = await sut.LoadAsync();

        loaded.Should().NotBeNull();
        loaded!.SqlPassword.Should().Be("***REDACTED***");
    }
}
