using System.Globalization;
using System.Windows.Media;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Mikro.Adapters;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AbstractionsMikroVersion = ErpBridge.Erp.Abstractions.MikroVersion;

namespace ErpBridge.Agent.UI.ViewModels;

/// <summary>
/// Backing view-model for the agent settings window. Mirrors the
/// <see cref="AgentConfig"/> shape so a two-way binding is straightforward.
/// "Bağlantıyı test et" wires through <see cref="IMikroConnectionTestOrchestrator"/>
/// (the single seam owned by <c>Erp.Mikro</c>); "Kaydet" persists through
/// <see cref="IAgentConfigStore"/> and refreshes the live
/// <see cref="IConfiguration"/> so subsequent adapter calls see fresh values.
/// </summary>
/// <remarks>
/// <para>
/// SQL passwords are scrubbed through <see cref="ConnectionStringMasker"/> at
/// every emit-to-log / status-panel boundary. A leaked
/// <c>SqlException.Message</c> that contains the connection string fragment
/// cannot escape this chokepoint.
/// </para>
/// <para>
/// Faz 3 Track 2 — the view-model now exposes a V15 / V16 / Unknown badge,
/// a "Sunucu bilgisi" panel with the resolved identity strategy, latency
/// and the test timestamp, and a separate "Versiyonu yeniden tespit et"
/// command that re-probes Mikro after invalidating the orchestrator cache.
/// The badge is rendered as a <see cref="Brush"/> property so XAML can
/// bind it directly to a <c>Border.Background</c> without a converter.
/// </para>
/// <para>
/// All long-running commands go through <see cref="AsyncRelayCommand"/>,
/// which disables itself while the underlying <see cref="Task"/> is in
/// flight — preventing the user from double-firing the connection test.
/// </para>
/// </remarks>
public sealed class AgentSettingsViewModel : ObservableObject
{
    private readonly IAgentConfigStore _store;
    private readonly IAgentConfigToErpSettingsMapper _configToErpSettings;
    private readonly IMikroConnectionTestOrchestrator _orchestrator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AgentSettingsViewModel> _logger;

    private string _licenseKey = string.Empty;
    private string _sqlServer = string.Empty;
    private string _sqlUserName = string.Empty;
    private string _sqlPassword = string.Empty;
    private string _mikroDatabaseName = string.Empty;
    private string _companyNo = "1";
    private string _branchNo = "1";
    private string _apiBaseUrl = "https://api.erpbridge.local";
    private bool _useWindowsAuth;
    private string _status = "Hazır.";
    private bool _isBusy;

    // Faz 3 Track 2 — version + connection-test state surface.
    private string _mikroVersionBadge = "—";
    private Brush _mikroVersionBrush = GrayBadgeBrush;
    private string _serverVersionDisplay = string.Empty;
    private string _identityStrategyDisplay = string.Empty;
    private long? _lastTestLatencyMs;
    private string _lastTestTimeDisplay = string.Empty;
    private bool _hasConnectionTestResult;
    private string _mikroVersionTooltip = string.Empty;
    private string _troubleshootingHint = string.Empty;

    /// <summary>Build the view-model — every dependency is required.</summary>
    public AgentSettingsViewModel(
        IAgentConfigStore store,
        IAgentConfigToErpSettingsMapper configToErpSettings,
        IMikroConnectionTestOrchestrator orchestrator,
        IConfiguration configuration,
        ILogger<AgentSettingsViewModel> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _configToErpSettings = configToErpSettings ?? throw new ArgumentNullException(nameof(configToErpSettings));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        SaveCommand = new RelayCommand(_ => _ = SaveAsync(), _ => !IsBusy);
        TestConnectionCommand = new AsyncRelayCommand(
            execute: _ => TestConnectionAsync(),
            canExecute: () => !IsBusy);
        RedetectVersionCommand = new AsyncRelayCommand(
            execute: _ => RedetectVersionAsync(),
            canExecute: () => !IsBusy && HasConnectionTestResult);
    }

    /// <summary>Lisans anahtarı.</summary>
    public string LicenseKey { get => _licenseKey; set => SetProperty(ref _licenseKey, value); }

    /// <summary>SQL Server adresi (host veya host\instance).</summary>
    public string SqlServer { get => _sqlServer; set => SetProperty(ref _sqlServer, value); }

    /// <summary>SQL kullanıcı adı.</summary>
    public string SqlUserName { get => _sqlUserName; set => SetProperty(ref _sqlUserName, value); }

    /// <summary>SQL şifresi (PasswordBox'tan alınır, property'ye atanırken DTO'ya kopyalanır).</summary>
    public string SqlPassword { get => _sqlPassword; set => SetProperty(ref _sqlPassword, value); }

    /// <summary>Mikro database adı.</summary>
    public string MikroDatabaseName { get => _mikroDatabaseName; set => SetProperty(ref _mikroDatabaseName, value); }

    /// <summary>Firma no (integer parse).</summary>
    public string CompanyNo { get => _companyNo; set => SetProperty(ref _companyNo, value); }

    /// <summary>Şube no (integer parse).</summary>
    public string BranchNo { get => _branchNo; set => SetProperty(ref _branchNo, value); }

    /// <summary>Central API base URL.</summary>
    public string ApiBaseUrl { get => _apiBaseUrl; set => SetProperty(ref _apiBaseUrl, value); }

    /// <summary>
    /// True when Mikro is reached via Windows Authentication (Trusted_Connection /
    /// Integrated Security). When set, <see cref="SqlUserName"/> and
    /// <see cref="SqlPassword"/> are ignored at the connection-string layer.
    /// </summary>
    public bool UseWindowsAuth
    {
        get => _useWindowsAuth;
        set
        {
            if (SetProperty(ref _useWindowsAuth, value))
            {
                OnPropertyChanged(nameof(SqlUserName));
                OnPropertyChanged(nameof(SqlPassword));
            }
        }
    }

    /// <summary>Son durum mesajı — UI'da multiline TextBlock'a bağlanır.</summary>
    public string Status { get => _status; set => SetProperty(ref _status, value); }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)TestConnectionCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)RedetectVersionCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>"V15" / "V16" / "Unknown" / "—". Bound to the badge TextBlock.</summary>
    public string MikroVersionBadge
    {
        get => _mikroVersionBadge;
        private set => SetProperty(ref _mikroVersionBadge, value);
    }

    /// <summary>Brush used to paint the badge background — direct binding, no converter.</summary>
    public Brush MikroVersionBrush
    {
        get => _mikroVersionBrush;
        private set => SetProperty(ref _mikroVersionBrush, value);
    }

    /// <summary>Server-product-version string (e.g. "16.0.1.7") for the info panel.</summary>
    public string ServerVersionDisplay
    {
        get => _serverVersionDisplay;
        private set => SetProperty(ref _serverVersionDisplay, value);
    }

    /// <summary>Display name of the picked identity strategy (e.g. "V15/RECno", "V16/Guid").</summary>
    public string IdentityStrategyDisplay
    {
        get => _identityStrategyDisplay;
        private set => SetProperty(ref _identityStrategyDisplay, value);
    }

    /// <summary>Latency of the most recent test in milliseconds, or <c>null</c> before the first test.</summary>
    public long? LastTestLatencyMs
    {
        get => _lastTestLatencyMs;
        private set => SetProperty(ref _lastTestLatencyMs, value);
    }

    /// <summary>Localised timestamp of the most recent test (empty until the first test runs).</summary>
    public string LastTestTimeDisplay
    {
        get => _lastTestTimeDisplay;
        private set => SetProperty(ref _lastTestTimeDisplay, value);
    }

    /// <summary>True once a test has produced a result. Gates <see cref="RedetectVersionCommand"/>.</summary>
    public bool HasConnectionTestResult
    {
        get => _hasConnectionTestResult;
        private set
        {
            if (SetProperty(ref _hasConnectionTestResult, value))
            {
                ((AsyncRelayCommand)RedetectVersionCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Tooltip text under the badge — a single-line summary for accessibility.</summary>
    public string MikroVersionTooltip
    {
        get => _mikroVersionTooltip;
        private set => SetProperty(ref _mikroVersionTooltip, value);
    }

    /// <summary>Troubleshooting hint rendered when the connection test fails.</summary>
    public string TroubleshootingHint
    {
        get => _troubleshootingHint;
        private set => SetProperty(ref _troubleshootingHint, value);
    }

    public System.Windows.Input.ICommand SaveCommand { get; }

    public System.Windows.Input.ICommand TestConnectionCommand { get; }

    public System.Windows.Input.ICommand RedetectVersionCommand { get; }

    /// <summary>Load persisted config into the view-model. Called on window open.</summary>
    public async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            var config = await _store.LoadAsync(ct).ConfigureAwait(false);
            if (config is null)
            {
                Status = "Henüz kayıtlı konfigürasyon yok. Alanları doldurun ve Kaydet'e basın.";
                return;
            }

            LicenseKey = config.LicenseKey ?? string.Empty;
            SqlServer = config.SqlServer ?? string.Empty;
            SqlUserName = config.SqlUserName ?? string.Empty;
            SqlPassword = config.SqlPassword ?? string.Empty;
            MikroDatabaseName = config.MikroDatabaseName ?? string.Empty;
            CompanyNo = config.CompanyNo.ToString(CultureInfo.InvariantCulture);
            BranchNo = config.BranchNo.ToString(CultureInfo.InvariantCulture);
            ApiBaseUrl = config.ApiBaseUrl ?? string.Empty;
            UseWindowsAuth = config.UseWindowsAuth;
            Status = "Konfigürasyon yüklendi.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AgentConfig load failed.");
            Status = "Konfigürasyon yüklenemedi: " + ex.Message;
        }
    }

    private async Task SaveAsync()
    {
        if (!TryValidateInputs(out var error))
        {
            Status = error;
            return;
        }

        IsBusy = true;
        try
        {
            var config = BuildAgentConfig();
            await _store.SaveAsync(config).ConfigureAwait(false);

            // Push the freshly-typed Mikro settings into the live configuration so
            // MikroAdapter — which re-reads IConfiguration on every TestConnection
            // call — sees the new values without a process restart. Non-Mikro keys
            // are kept untouched.
            WriteMikroSectionToConfiguration(config);

            // Password NEVER appears in the log message — only the host, user,
            // database, and result code. SqlUserName is safe to log (not secret).
            _logger.LogInformation(
                "AgentConfig saved. Server={Server}, Database={Database}, UserName={UserName}, Company={Company}, Branch={Branch}.",
                config.SqlServer, config.MikroDatabaseName, config.SqlUserName,
                config.CompanyNo, config.BranchNo);

            Status = "Konfigürasyon kaydedildi.";
        }
        catch (Exception ex)
        {
            // SQL password is excluded from this log statement — the catch only
            // carries the exception message + class name, never the DTO.
            _logger.LogError(ex,
                "AgentConfig save failed for Server={Server}, Database={Database}.",
                SqlServer, MikroDatabaseName);
            Status = "Kaydetme başarısız: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TestConnectionAsync()
    {
        if (!TryValidateInputs(out var error))
        {
            Status = error;
            return;
        }

        IsBusy = true;
        try
        {
            var config = BuildAgentConfig();

            // Mapper contract: the view-model is allowed to surface a clear
            // "all fields required" message without ever touching Mikro types
            // (the mapper does the type check below).
            var settings = _configToErpSettings.ToErpSettings(config);
            if (settings is null)
            {
                Status = "Konfigürasyon geçersiz: tüm alanlar zorunlu.";
                HasConnectionTestResult = false;
                ResetBadge();
                return;
            }

            if (settings is not MikroConnectionSettings)
            {
                Status = "Adapter ayarları beklenen formatta değil (Mikro değil?).";
                HasConnectionTestResult = false;
                ResetBadge();
                return;
            }

            // Push the freshly-typed Mikro settings into the live configuration
            // so the orchestrator sees them on its next probe.
            WriteMikroSectionToConfiguration(config);

            var result = await _orchestrator.RunFullTestAsync().ConfigureAwait(false);
            ApplyTestResult(result, prefix: "Bağlantı");

            if (result.Ok)
            {
                _logger.LogInformation(
                    "Mikro connection test OK. Server={Server}, Database={Database}, ServerVersion={ServerVersion}, MikroVersion={MikroVersion}, LatencyMs={Latency}.",
                    SqlServer, MikroDatabaseName, result.ServerVersion, result.DetectedMikroVersion, result.LatencyMs);
            }
            else
            {
                _logger.LogWarning(
                    "Mikro connection test FAILED. Server={Server}, Database={Database}, MaskedMessage={MaskedMessage}.",
                    SqlServer, MikroDatabaseName, ConnectionStringMasker.MaskForLog(result.Message));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "TestConnection failed for Server={Server}, Database={Database}.",
                SqlServer, MikroDatabaseName);
            Status = "Bağlantı testi başarısız: " + ConnectionStringMasker.MaskForLog(ex.Message);
            TroubleshootingHint = BuildTroubleshootingHint(ex);
            HasConnectionTestResult = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RedetectVersionAsync()
    {
        if (!TryValidateInputs(out var error))
        {
            Status = error;
            return;
        }

        IsBusy = true;
        try
        {
            // Force a re-probe: drop the cached entry first so the orchestrator
            // hits the database. If the connection itself is down this surfaces
            // a clean error on the badge instead of a stale "V15" string.
            _orchestrator.InvalidateCache();
            var config = BuildAgentConfig();
            WriteMikroSectionToConfiguration(config);

            // Re-use the same orchestration path so the cache state stays
            // consistent. MikroAdapter's TestConnectionAsync would also re-probe
            // but the orchestrator is the single seam and owns the cache.
            var result = await _orchestrator.RunFullTestAsync().ConfigureAwait(false);
            ApplyTestResult(result, prefix: "Versiyon tespiti");

            if (result.Ok)
            {
                _logger.LogInformation(
                    "Mikro redetect succeeded. Database={Database}, ServerVersion={ServerVersion}, MikroVersion={MikroVersion}, LatencyMs={Latency}.",
                    MikroDatabaseName, result.ServerVersion, result.DetectedMikroVersion, result.LatencyMs);
            }
            else
            {
                _logger.LogWarning(
                    "Mikro redetect FAILED. Database={Database}, MaskedMessage={MaskedMessage}.",
                    MikroDatabaseName, ConnectionStringMasker.MaskForLog(result.Message));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Redetect version failed for Database={Database}.",
                MikroDatabaseName);
            Status = "Versiyon tespiti başarısız: " + ConnectionStringMasker.MaskForLog(ex.Message);
            TroubleshootingHint = BuildTroubleshootingHint(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Project a <see cref="ErpConnectionTestResult"/> into the badge / info-panel
    /// state and the <see cref="Status"/> string. Centralised so the test and
    /// redetect commands stay in lockstep on what the user sees.
    /// </summary>
    private void ApplyTestResult(ErpConnectionTestResult result, string prefix)
    {
        HasConnectionTestResult = true;
        LastTestLatencyMs = result.LatencyMs;
        LastTestTimeDisplay = result.TestedAtUtc.HasValue
            ? result.TestedAtUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture)
            : DateTimeOffset.UtcNow.ToLocalTime()
                .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);

        if (result.Ok)
        {
            ApplyBadgeFromVersion(result.DetectedMikroVersion);
            ServerVersionDisplay = result.ServerVersion ?? "—";
            IdentityStrategyDisplay = string.IsNullOrWhiteSpace(result.IdentityStrategyName)
                ? "—"
                : result.IdentityStrategyName!;
            TroubleshootingHint = string.Empty;

            Status = prefix + " başarılı.\n" +
                     "Server: " + SqlServer + "\n" +
                     "DB: " + MikroDatabaseName + "\n" +
                     "Mesaj: " + (result.Message ?? "ok") + "\n" +
                     "ServerVersion: " + (result.ServerVersion ?? "?");
        }
        else
        {
            // Soft failure: clear the badge to "Unknown" + red so the operator
            // sees the test did not yield a usable version.
            ApplyBadgeFromVersion(AbstractionsMikroVersion.Unknown);
            ServerVersionDisplay = "—";
            IdentityStrategyDisplay = "—";
            var masked = ConnectionStringMasker.MaskForLog(result.Message) ?? "bilinmeyen hata";
            Status = prefix + " başarısız: " + masked;
            TroubleshootingHint = BuildTroubleshootingHintFromMessage(result.Message);
        }
    }

    private void ApplyBadgeFromVersion(AbstractionsMikroVersion? version)
    {
        if (!version.HasValue || version.Value == AbstractionsMikroVersion.Unknown)
        {
            MikroVersionBadge = "Unknown";
            MikroVersionBrush = RedBadgeBrush;
            MikroVersionTooltip = "Mikro versiyonu tespit edilemedi. Versiyonu yeniden tespit et butonu ile yeniden deneyin.";
            return;
        }

        if (version.Value == AbstractionsMikroVersion.V15)
        {
            MikroVersionBadge = "V15";
            MikroVersionBrush = BlueBadgeBrush;
            MikroVersionTooltip = "Mikro V15 — RECno identity strategy kullanılır.";
            return;
        }

        if (version.Value == AbstractionsMikroVersion.V16)
        {
            MikroVersionBadge = "V16";
            MikroVersionBrush = GreenBadgeBrush;
            MikroVersionTooltip = "Mikro V16 — Guid identity strategy kullanılır.";
        }
    }

    private void ResetBadge()
    {
        MikroVersionBadge = "—";
        MikroVersionBrush = GrayBadgeBrush;
        MikroVersionTooltip = string.Empty;
        ServerVersionDisplay = string.Empty;
        IdentityStrategyDisplay = string.Empty;
        LastTestLatencyMs = null;
        LastTestTimeDisplay = string.Empty;
    }

    /// <summary>
    /// Build an <see cref="AgentConfig"/> from the current view-model state.
    /// Used by both Save (persists the freshly typed values) and Test
    /// Connection (so the user can probe without saving first).
    /// </summary>
    private AgentConfig BuildAgentConfig()
    {
        return new AgentConfig
        {
            LicenseKey = LicenseKey?.Trim() ?? string.Empty,
            SqlServer = SqlServer?.Trim() ?? string.Empty,
            SqlUserName = SqlUserName?.Trim() ?? string.Empty,
            SqlPassword = SqlPassword ?? string.Empty,
            MikroDatabaseName = MikroDatabaseName?.Trim() ?? string.Empty,
            CompanyNo = int.Parse(CompanyNo, CultureInfo.InvariantCulture),
            BranchNo = int.Parse(BranchNo, CultureInfo.InvariantCulture),
            ApiBaseUrl = ApiBaseUrl?.Trim() ?? string.Empty,
            UseWindowsAuth = UseWindowsAuth,
            ErpType = Core.Domain.ErpType.Mikro,
        };
    }

    /// <summary>
    /// Project the Mikro-relevant fields of <paramref name="config"/> into the
    /// "Mikro" section of the live <see cref="IConfiguration"/>. The adapter
    /// reads this section on every TestConnection call, so updating it here
    /// makes the user-visible Mikro credentials the source of truth without
    /// requiring a process restart.
    /// </summary>
    private void WriteMikroSectionToConfiguration(AgentConfig config)
    {
        if (_configuration is not IConfigurationRoot root)
        {
            // Read-only configuration (e.g. test fixture without reload). The
            // adapter will fall back to the values present at construction
            // time — not ideal, but the Save+Test loop is still observable.
            return;
        }

        var section = _configuration.GetSection(MikroConnectionSettings.ConfigurationSection);
        section["Server"] = config.SqlServer ?? string.Empty;
        section["UserId"] = config.SqlUserName ?? string.Empty;
        section["Password"] = config.SqlPassword ?? string.Empty;
        section["DatabaseName"] = config.MikroDatabaseName ?? string.Empty;
        section["IntegratedSecurity"] = config.UseWindowsAuth ? "true" : "false";

        // Touch the root to make sure reload-aware providers notice the change
        // even when an in-memory provider above us doesn't forward by itself.
        root.Reload();
    }

    private bool TryValidateInputs(out string error)
        => AgentSettingsValidation.TryValidate(
            SqlServer, SqlUserName, MikroDatabaseName, CompanyNo, BranchNo, UseWindowsAuth, out error);

    /// <summary>
    /// Build a user-visible troubleshooting hint based on the exception's
    /// message text. The hint set is small — server unreachable, login
    /// failure, database missing, generic — and lives here (not in
    /// resources) because the WPF form renders them verbatim.
    /// </summary>
    /// <remarks>
    /// <para>
    /// SqlException (the most common error from the Mikro adapter) is NOT
    /// caught with a typed <c>catch</c> clause here because the WPF UI
    /// project does not reference <c>Microsoft.Data.SqlClient</c>. The
    /// message is the carrier either way; the heuristic that follows
    /// matches on Turkish / English keywords that both SqlException and
    /// our own configuration-missing messages use.
    /// </para>
    /// </remarks>
    private static string BuildTroubleshootingHint(Exception ex)
        => BuildTroubleshootingHintFromMessage(ex.Message);

    private static string BuildTroubleshootingHintFromMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Kontrol edin: Sunucu adı, SQL Server servisinin dışarıdan bağlantıya izin verdiği, kullanıcı adı/şifre ve database adı.";
        }

        var lowered = message.ToLowerInvariant();
        var hints = new List<string>();

        // "server", "network", "timeout" — connection-level failure
        if (lowered.Contains("server") || lowered.Contains("network") || lowered.Contains("timeout") || lowered.Contains("connect"))
        {
            hints.Add("• Sunucu adı doğru mu? (host veya host\\instance)");
            hints.Add("• SQL Server servisi dışarıdan bağlantıya izin veriyor mu? (TCP 1433, firewall)");
        }

        // "login", "password", "authentication" — credential failure
        if (lowered.Contains("login") || lowered.Contains("password") || lowered.Contains("authentication") || lowered.Contains("credential"))
        {
            hints.Add("• Kullanıcı adı ve şifre doğru mu?");
            hints.Add("• SQL login'e sysadmin / db_owner yetkisi verildi mi?");
        }

        // "cannot open database", "database", "catalog" — database-level failure
        if (lowered.Contains("database") || lowered.Contains("catalog") || lowered.Contains("cannot open"))
        {
            hints.Add("• Database adı doğru mu? SQL Server'da listeleniyor mu?");
            hints.Add("• Login bu database'e erişim yetkisine sahip mi?");
        }

        if (hints.Count == 0)
        {
            return "Kontrol edin: Sunucu adı, SQL Server servisinin dışarıdan bağlantıya izin verdiği, kullanıcı adı/şifre ve database adı.";
        }

        return string.Join(Environment.NewLine, hints);
    }

    /// <summary>Pre-built brushes for the badge. Allocated once — the brush is a freezable singleton.</summary>
    private static readonly Brush BlueBadgeBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x19, 0x76, 0xD2)));
    private static readonly Brush GreenBadgeBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x38, 0x8E, 0x3C)));
    private static readonly Brush RedBadgeBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)));
    private static readonly Brush GrayBadgeBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)));

    private static Brush Freeze(SolidColorBrush brush)
    {
        brush.Freeze();
        return brush;
    }
}
