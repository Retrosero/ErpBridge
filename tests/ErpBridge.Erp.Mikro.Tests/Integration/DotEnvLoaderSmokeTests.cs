using Xunit;
using Xunit.Abstractions;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

public class DotEnvLoaderSmokeTests
{
    private readonly ITestOutputHelper _out;
    public DotEnvLoaderSmokeTests(ITestOutputHelper output) { _out = output; }

    /// <summary>
    /// Smoke test for the loader itself. Live TULPAR credentials are optional
    /// operator configuration, so this test must never require a local .env
    /// file or assert real environment values.
    /// </summary>
    [Fact]
    public void Loader_populates_values_from_an_explicit_dotenv_file()
    {
        var key = "ERPBRIDGE_DOTENV_SMOKE_" + Guid.NewGuid().ToString("N");
        var directory = Path.Combine(Path.GetTempPath(), "erpbridge-dotenv-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var previous = Environment.GetEnvironmentVariable(key);

        try
        {
            File.WriteAllText(Path.Combine(directory, ".env"), $"{key}=loaded-value{Environment.NewLine}");
            Environment.SetEnvironmentVariable(key, null);

            var applied = DotEnvLoader.Load(directory);

            _out.WriteLine($"Applied values: {applied}");
            Assert.Equal(1, applied);
            Assert.Equal("loaded-value", Environment.GetEnvironmentVariable(key));
        }
        finally
        {
            Environment.SetEnvironmentVariable(key, previous);
            Directory.Delete(directory, recursive: true);
        }
    }
}
