using System.Security.Cryptography;
using ErpBridge.Portal.Session;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// A browser keeps its session encrypted with Data Protection. Two service providers stand in for
/// the container before and after a redeploy.
/// </summary>
public sealed class PortalDataProtectionTests : IDisposable
{
    private readonly string _keys = Path.Combine(Path.GetTempPath(), "portal-keys-" + Guid.NewGuid().ToString("N"));

    private static IDataProtector Protector(string keysPath)
    {
        var settings = new Dictionary<string, string?> { [PortalDataProtection.KeysPathSetting] = keysPath };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPortalDataProtection(configuration);
        return services.BuildServiceProvider().GetRequiredService<IDataProtectionProvider>().CreateProtector("portal-session");
    }

    [Fact]
    public void A_session_saved_before_a_redeploy_is_readable_after_it()
    {
        var saved = Protector(_keys).Protect("tok-patron");

        Protector(_keys).Unprotect(saved).Should().Be("tok-patron");
        Directory.EnumerateFiles(_keys, "key-*.xml").Should().NotBeEmpty();
    }

    [Fact]
    public void A_new_container_with_its_own_keys_cannot_read_the_old_session()
    {
        // What a redeploy did before: the default key folder is inside the container's own file
        // system, so the new container starts with keys the browser's session was not sealed with.
        var saved = Protector(_keys).Protect("tok-patron");

        var read = () => Protector(_otherContainerKeys).Unprotect(saved);

        read.Should().Throw<CryptographicException>();
    }

    private readonly string _otherContainerKeys = Path.Combine(Path.GetTempPath(), "portal-keys-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        foreach (var dir in new[] { _keys, _otherContainerKeys })
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
    }
}
