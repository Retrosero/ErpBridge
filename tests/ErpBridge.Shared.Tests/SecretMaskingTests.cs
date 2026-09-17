using ErpBridge.Shared;
using FluentAssertions;

namespace ErpBridge.Shared.Tests;

/// <summary>Log Merkezi L3b: <see cref="ConnectionStringMasker.MaskSecrets"/> — the agent's single secret list.</summary>
public class SecretMaskingTests
{
    [Theory]
    [InlineData("Server=erp;Database=MikroDB_V16;User Id=sa;Password=Gizli 123;", "Gizli 123")]
    [InlineData("Authorization: Bearer abc.def-ghi", "abc.def-ghi")]
    [InlineData("jwt eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjMifQ.c2lnbmF0dXJl expired", "eyJzdWIiOiIxMjMifQ")]
    [InlineData("license AK-7F3K-99QX rejected", "7F3K-99QX")]
    [InlineData("{\"licenseKey\":\"LIC 42 with space\",\"x\":1}", "LIC 42 with space")]
    [InlineData("token=abc123&tenant=5", "abc123")]
    [InlineData("apiKey: s3cr3t", "s3cr3t")]
    [InlineData("Server=erp;Password=\"Top;Secret\";Database=Mikro", "Secret")]
    [InlineData("Server=erp;Pwd='Top;Secret';Database=Mikro", "Secret")]
    public void Secrets_are_masked(string input, string secret)
    {
        var masked = ConnectionStringMasker.MaskSecrets(input);

        masked.Should().NotContain(secret).And.Contain(ConnectionStringMasker.RedactedMarker);
    }

    [Theory]
    [InlineData("Sync of STOKLAR finished: 1250 rows in 3400 ms")]
    [InlineData(@"at ErpBridge.Core.Sync.AgentSyncLoop.RunAsync() in C:\src\AgentSyncLoop.cs:line 114")]
    [InlineData("CancellationToken was cancelled")]
    public void Ordinary_lines_are_left_alone(string input)
    {
        ConnectionStringMasker.MaskSecrets(input).Should().Be(input);
    }

    [Fact]
    public void Masked_json_stays_parseable()
    {
        var masked = ConnectionStringMasker.MaskSecrets("{\"token\":\"a \\\"b\\\" c\",\"n\":2}");

        System.Text.Json.JsonDocument.Parse(masked).RootElement.GetProperty("n").GetInt32().Should().Be(2);
    }
}
