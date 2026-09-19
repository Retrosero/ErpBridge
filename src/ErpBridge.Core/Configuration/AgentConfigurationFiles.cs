using Microsoft.Extensions.Configuration;

namespace ErpBridge.Core.Configuration;

/// <summary>
/// Ajanın JSON ayar dosyalarını doğru sırayla yükler.
///
/// <para>
/// <c>appsettings.example.json</c> kurulumla birlikte gelen <b>örnek</b> dosyadır: operatör
/// hiç ayar yazmadığında çalışan bir taban verir. Bu yüzden <b>ilk</b> sırada yüklenmelidir —
/// yapılandırmada son yüklenen kazanır, yani en sona konursa operatörün kendi
/// <c>appsettings.json</c>'ını sessizce ezer (PR #142 incelemesi).
/// </para>
/// </summary>
public static class AgentConfigurationFiles
{
    /// <summary>Örnek dosya (taban) → operatör dosyası → ortama özel dosya.</summary>
    public static IConfigurationBuilder AddAgentJsonFiles(
        this IConfigurationBuilder builder,
        bool operatorFileOptional = true,
        string? environmentName = null,
        bool reloadOnChange = true)
    {
        builder.AddJsonFile("appsettings.example.json", optional: true, reloadOnChange: false);
        builder.AddJsonFile("appsettings.json", optional: operatorFileOptional, reloadOnChange: reloadOnChange);
        if (!string.IsNullOrWhiteSpace(environmentName))
            builder.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: reloadOnChange);
        return builder;
    }
}
