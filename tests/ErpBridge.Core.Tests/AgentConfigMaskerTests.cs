using ErpBridge.Core.Domain;
using ErpBridge.Shared;
using FluentAssertions;

namespace ErpBridge.Core.Tests;

/// <summary>
/// Unit tests for <see cref="AgentConfigMasker"/> — guarantees the SQL password
/// is never exposed through a structured log scope or diagnostic dump.
/// </summary>
public class AgentConfigMaskerTests
{
    [Fact]
    public void Mask_replaces_SqlPassword_with_redacted_marker()
    {
        var config = new AgentConfig
        {
            SqlServer = "MIKROSQL\\MIKRO",
            SqlUserName = "sa",
            SqlPassword = "topsecret",
            MikroDatabaseName = "MIKRO16",
        };

        var masked = AgentConfigMasker.Mask(config);

        masked.SqlPassword.Should().Be(ConnectionStringMasker.RedactedMarker);
        masked.SqlServer.Should().Be(config.SqlServer);
        masked.SqlUserName.Should().Be(config.SqlUserName);
        masked.MikroDatabaseName.Should().Be(config.MikroDatabaseName);
    }

    [Fact]
    public void Mask_does_not_mutate_the_input_instance()
    {
        var config = new AgentConfig { SqlPassword = "topsecret" };

        AgentConfigMasker.Mask(config);

        config.SqlPassword.Should().Be("topsecret");
    }

    [Fact]
    public void Mask_returns_a_fresh_default_instance_for_null_input()
    {
        var masked = AgentConfigMasker.Mask(null);

        masked.Should().NotBeNull();
        // A fresh AgentConfig() leaves SqlPassword as null — the masker's job is
        // only to substitute when the source field is non-null. Callers that need
        // a non-null sentinel can do `masked.SqlPassword ?? "***"`.
        masked.SqlPassword.Should().BeNull();
        masked.SqlServer.Should().BeNull();
    }

    [Fact]
    public void Mask_preserves_unrelated_fields()
    {
        var config = new AgentConfig
        {
            LicenseKey = "LIC-12345",
            TenantId = "tenant-7",
            ErpType = ErpType.Mikro,
            SqlServer = "srv",
            SqlUserName = "u",
            SqlPassword = "secret",
            MikroDatabaseName = "MIKRO16",
            CompanyNo = 5,
            BranchNo = 2,
            ApiBaseUrl = "https://api.erpbridge.local",
        };

        var masked = AgentConfigMasker.Mask(config);

        masked.LicenseKey.Should().Be(config.LicenseKey);
        masked.TenantId.Should().Be(config.TenantId);
        masked.ErpType.Should().Be(config.ErpType);
        masked.CompanyNo.Should().Be(config.CompanyNo);
        masked.BranchNo.Should().Be(config.BranchNo);
        masked.ApiBaseUrl.Should().Be(config.ApiBaseUrl);
        masked.SqlPassword.Should().Be(ConnectionStringMasker.RedactedMarker);
    }
}