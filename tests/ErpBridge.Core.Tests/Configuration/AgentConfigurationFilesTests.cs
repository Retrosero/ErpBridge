using ErpBridge.Core.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ErpBridge.Core.Tests.Configuration;

/// <summary>
/// Kurulumla gelen <c>appsettings.example.json</c> yalnızca taban değer vermelidir.
/// En sona yüklenirse operatörün kendi ayarını sessizce ezer — ajan yanlış veritabanına
/// bağlanır ve bunu hiçbir yerde söylemez (PR #142 incelemesi).
/// </summary>
public sealed class AgentConfigurationFilesTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "erpb-cfg-" + Guid.NewGuid().ToString("N"));

    public AgentConfigurationFilesTests()
    {
        Directory.CreateDirectory(_dir);
        Write("appsettings.example.json", """{ "ErpBridge": { "LocalStore": { "DataSource": "ornek.db" }, "Ornekten": "evet" } }""");
    }

    public void Dispose()
    {
        try { Directory.Delete(_dir, recursive: true); } catch (IOException) { }
    }

    private void Write(string name, string json) => File.WriteAllText(Path.Combine(_dir, name), json);

    private IConfigurationRoot Build(string? environmentName = null) => new ConfigurationBuilder()
        .SetBasePath(_dir)
        .AddAgentJsonFiles(environmentName: environmentName)
        .Build();

    [Fact]
    public void The_operator_file_wins_over_the_shipped_example()
    {
        Write("appsettings.json", """{ "ErpBridge": { "LocalStore": { "DataSource": "operator.db" } } }""");

        var configuration = Build();

        configuration["ErpBridge:LocalStore:DataSource"].Should().Be("operator.db");
    }

    [Fact]
    public void The_example_still_fills_in_what_the_operator_did_not_set()
    {
        Write("appsettings.json", """{ "ErpBridge": { "LocalStore": { "DataSource": "operator.db" } } }""");

        Build()["ErpBridge:Ornekten"].Should().Be("evet", "örnek dosya taban değer olarak durur");
    }

    [Fact]
    public void Without_an_operator_file_the_example_is_what_runs()
    {
        Build()["ErpBridge:LocalStore:DataSource"].Should().Be("ornek.db");
    }

    [Fact]
    public void The_environment_file_wins_over_both()
    {
        Write("appsettings.json", """{ "ErpBridge": { "LocalStore": { "DataSource": "operator.db" } } }""");
        Write("appsettings.Development.json", """{ "ErpBridge": { "LocalStore": { "DataSource": "gelistirme.db" } } }""");

        Build(environmentName: "Development")["ErpBridge:LocalStore:DataSource"].Should().Be("gelistirme.db");
    }
}
