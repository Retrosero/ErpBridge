using Xunit;
using Xunit.Abstractions;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

public class DotEnvLoaderSmokeTests
{
    private readonly ITestOutputHelper _out;
    public DotEnvLoaderSmokeTests(ITestOutputHelper output) { _out = output; }

    /// <summary>
    /// Smoke test — confirms that the TULPAR environment variables end up set
    /// in the test process. The actual application is done by the
    /// <c>ModuleInitializer</c> in <c>AssemblyInit.cs</c> which runs before
    /// any test method, so by the time this test body executes the env vars
    /// are already populated. We assert the populated state, not the
    /// load-count, so the test passes whether the module initializer or the
    /// explicit call below did the work.
    /// </summary>
    [Fact]
    public void Loader_populates_TULPAR_env_vars_for_test_process()
    {
        // Force a load call too — cheap, idempotent (already-set env vars are
        // not overwritten), and surfaces a load error if the .env goes missing
        // between module-init time and now.
        _ = DotEnvLoader.Load();

        _out.WriteLine($"TULPAR_SERVER={Environment.GetEnvironmentVariable("ERPBridge_TULPAR_SERVER") ?? "<null>"}");
        _out.WriteLine($"TULPAR_DATABASE={Environment.GetEnvironmentVariable("ERPBridge_TULPAR_DATABASE") ?? "<null>"}");
        _out.WriteLine($"TULPAR_USER={Environment.GetEnvironmentVariable("ERPBridge_TULPAR_USER") ?? "<null>"}");
        var pwd = Environment.GetEnvironmentVariable("ERPBridge_TULPAR_PASSWORD");
        _out.WriteLine($"TULPAR_PASSWORD.Length={(pwd is null ? -1 : pwd.Length)}");

        Assert.Equal("TULPAR", Environment.GetEnvironmentVariable("ERPBridge_TULPAR_SERVER"));
        Assert.Equal("MikroDB_V15_02", Environment.GetEnvironmentVariable("ERPBridge_TULPAR_DATABASE"));
        Assert.Equal("mikro_sync_user", Environment.GetEnvironmentVariable("ERPBridge_TULPAR_USER"));
        Assert.False(string.IsNullOrEmpty(pwd), "TULPAR_PASSWORD should be set (length > 0).");
    }
}
