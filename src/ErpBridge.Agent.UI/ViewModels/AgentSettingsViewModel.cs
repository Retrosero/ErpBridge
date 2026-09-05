using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Media;
using ErpBridge.Agent.UI.DependencyInjection;
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
    private readonly MutableMemoryConfigurationProvider _liveSettings;
    private readonly ILogger<AgentSettingsViewModel> _logger;

    private string _licenseKey = string.Empty;
    private string _sqlServer = string.Empty;
    private string _sqlUserName = string.Empty;
    private string _sqlPassword = string.Empty;
    private string _mikroDatabaseName = string.Empty;
    private string _apiBaseUrl = "https://api.erpbridge.local";
    private bool _useWindowsAuth;
    // Faz 10 — multi-firm Mikro: company / branch / warehouse numbers used by
    // every bootstrap reader query. Stored as strings so the WPF TextBox can
    // two-way bind to them and surface validation errors locally; the
    // view-model parses the strings on read/write.
    private string _companyNo = "1";
    private string _branchNo = "0";
    private string _warehouseNo = "1";
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

    // Faz 7 — tab visibility + dashboard glue.
    private bool _hasSavedConfig;
    private string _lastSavedAtDisplay = string.Empty;
    private string _lastSyncAtDisplay = string.Empty;

    // Faz 8 — Central API lisans + sunucu DB kontrol (diagnostic test butonları).
    private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(10) };
    private bool? _licenseCheckValid;
    private string _licenseCheckTenantIdDisplay = string.Empty;
    private string _licenseCheckExpiresAtDisplay = string.Empty;
    private string _licenseCheckErrorCodeDisplay = string.Empty;
    private string _licenseCheckErrorMessageDisplay = string.Empty;
    private long? _licenseCheckLatencyMs;
    private string _licenseCheckTimeDisplay = string.Empty;
    private bool _hasLicenseCheckResult;
    private string _serverDbCheckStatusDisplay = string.Empty;
    private string _serverDbCheckHttpDisplay = string.Empty;
    private string _serverDbCheckBodyDisplay = string.Empty;
    private long? _serverDbCheckLatencyMs;
    private string _serverDbCheckTimeDisplay = string.Empty;
    private bool _hasServerDbCheckResult;

    // Faz 8 — Central API'ye agent kayıt (POST /api/v1/agents/register). Bu çağrı
    // olmadan bootstrap push 401 alır (Central, JWT olmadan /api/v1/bootstrap'u
    // reddeder). Register başarılıysa dönen JWT in-memory olarak
    // CentralApiOptions.Jwt'ye yazılır — sonraki çağrılar geçer.
    private bool _hasRegisterResult;
    private bool? _registerSuccess;
    private string _registerAgentIdDisplay = string.Empty;
    private string _registerTenantIdDisplay = string.Empty;
    private string _registerExpiresAtDisplay = string.Empty;
    private string _registerErrorCodeDisplay = string.Empty;
    private string _registerErrorMessageDisplay = string.Empty;
    private long? _registerLatencyMs;
    private string _registerTimeDisplay = string.Empty;

    /// <summary>Build the view-model — every dependency is required.</summary>
    /// <param name="liveSettings">
    /// In-memory <see cref="MutableMemoryConfigurationProvider"/> seeded by the
    /// host at startup. The view-model writes the latest Mikro credentials here
    /// so the orchestrator's <c>MikroConnectionSettings.FromConfiguration</c>
    /// call sees the freshly-typed values without a process restart. Writing
    /// into a dedicated provider (rather than calling
    /// <c>IConfigurationRoot.Reload()</c> on the JSON-backed root) prevents
    /// the JSON provider from re-reading the file and erasing the in-memory
    /// edits.
    /// </param>
    public AgentSettingsViewModel(
        IAgentConfigStore store,
        IAgentConfigToErpSettingsMapper configToErpSettings,
        IMikroConnectionTestOrchestrator orchestrator,
        IConfiguration configuration,
        MutableMemoryConfigurationProvider liveSettings,
        ILogger<AgentSettingsViewModel> logger)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _configToErpSettings = configToErpSettings ?? throw new ArgumentNullException(nameof(configToErpSettings));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _liveSettings = liveSettings ?? throw new ArgumentNullException(nameof(liveSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        SaveCommand = new RelayCommand(_ => _ = SaveAsync(), _ => !IsBusy);
        TestConnectionCommand = new AsyncRelayCommand(
            execute: _ => TestConnectionAsync(),
            canExecute: () => !IsBusy);
        RedetectVersionCommand = new AsyncRelayCommand(
            execute: _ => RedetectVersionAsync(),
            canExecute: () => !IsBusy && HasConnectionTestResult);
        TestLicenseCommand = new AsyncRelayCommand(
            execute: _ => TestLicenseAsync(),
            canExecute: () => !IsBusy
                && !string.IsNullOrWhiteSpace(LicenseKey)
                && !string.IsNullOrWhiteSpace(ApiBaseUrl));
        TestServerDbCommand = new AsyncRelayCommand(
            execute: _ => TestServerDbAsync(),
            canExecute: () => !IsBusy && !string.IsNullOrWhiteSpace(ApiBaseUrl));
        RegisterAgentCommand = new AsyncRelayCommand(
            execute: _ => RegisterAgentAsync(),
            canExecute: () => !IsBusy
                && !string.IsNullOrWhiteSpace(LicenseKey)
                && !string.IsNullOrWhiteSpace(ApiBaseUrl));
    }

    /// <summary>Lisans anahtarı.</summary>
    public string LicenseKey
    {
        get => _licenseKey;
        set
        {
            if (SetProperty(ref _licenseKey, value))
            {
                ((AsyncRelayCommand)TestLicenseCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)RegisterAgentCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>SQL Server adresi (host veya host\instance).</summary>
    public string SqlServer { get => _sqlServer; set => SetProperty(ref _sqlServer, value); }

    /// <summary>SQL kullanıcı adı.</summary>
    public string SqlUserName { get => _sqlUserName; set => SetProperty(ref _sqlUserName, value); }

    /// <summary>SQL şifresi (PasswordBox'tan alınır, property'ye atanırken DTO'ya kopyalanır).</summary>
    public string SqlPassword { get => _sqlPassword; set => SetProperty(ref _sqlPassword, value); }

    /// <summary>Mikro database adı.</summary>
    public string MikroDatabaseName { get => _mikroDatabaseName; set => SetProperty(ref _mikroDatabaseName, value); }

    /// <summary>
    /// Mikro firma numarası. Tüm bootstrap sorguları bu değerle filtrelenir
    /// (<c>*_firmano = @firmNo</c>). Faz 10: artık kullanıcı tarafından
    /// ayarlanabilir; tek-firmalı kurulumlarda varsayılan 1'dir.
    /// </summary>
    public string CompanyNo
    {
        get => _companyNo;
        set
        {
            if (SetProperty(ref _companyNo, value ?? string.Empty))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Mikro şube numarası. Satış siparişi yazımı için kullanılır. Tek
    /// şubeli kurulumlarda 0 bırakılabilir.
    /// </summary>
    public string BranchNo
    {
        get => _branchNo;
        set
        {
            if (SetProperty(ref _branchNo, value ?? string.Empty))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Stok envanter sorguları için varsayılan depo numarası
    /// (<c>STOK_HAREKETLERI.sth_depo_no</c>). Per-row depo numarası olmayan
    /// toplamalar bu değerle yapılır.
    /// </summary>
    public string WarehouseNo
    {
        get => _warehouseNo;
        set
        {
            if (SetProperty(ref _warehouseNo, value ?? string.Empty))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Central API base URL.</summary>
    public string ApiBaseUrl
    {
        get => _apiBaseUrl;
        set
        {
            if (SetProperty(ref _apiBaseUrl, value))
            {
                ((AsyncRelayCommand)TestLicenseCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)TestServerDbCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)RegisterAgentCommand).RaiseCanExecuteChanged();
            }
        }
    }

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
                ((AsyncRelayCommand)TestLicenseCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)TestServerDbCommand).RaiseCanExecuteChanged();
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

    /// <summary>
    /// True when the user has saved a complete Mikro configuration at least
    /// once during this process lifetime. Drives the Pano tab visibility —
    /// the dashboard only appears once the operator has real settings to
    /// monitor. Refreshed on every successful <see cref="SaveAsync"/>.
    /// </summary>
    public bool HasSavedConfig
    {
        get => _hasSavedConfig;
        private set => SetProperty(ref _hasSavedConfig, value);
    }

    /// <summary>Localised "saved at" timestamp for the footer of the Settings tab.</summary>
    public string LastSavedAtDisplay
    {
        get => _lastSavedAtDisplay;
        private set => SetProperty(ref _lastSavedAtDisplay, value);
    }

    /// <summary>
    /// Localised "last successful sync at" timestamp — wired to the
    /// Pano tab's primary metric. Populated by the dashboard service when
    /// the tab is opened.
    /// </summary>
    public string LastSyncAtDisplay
    {
        get => _lastSyncAtDisplay;
        private set => SetProperty(ref _lastSyncAtDisplay, value);
    }

    /// <summary>"true" / "false" / "—" — bound to the lisans kontrol paneli.</summary>
    public string LicenseCheckValidDisplay
    {
        get
        {
            if (!_hasLicenseCheckResult) return "—";
            if (_licenseCheckValid is null) return "(no response)";
            return _licenseCheckValid.Value ? "✓ true" : "✗ false";
        }
    }

    /// <summary>Resolved tenant GUID (empty if invalid).</summary>
    public string LicenseCheckTenantIdDisplay
    {
        get
        {
            if (!_hasLicenseCheckResult) return string.Empty;
            return _licenseCheckTenantIdDisplay;
        }
    }

    /// <summary>License expiry in local time (empty if no expiry / invalid).</summary>
    public string LicenseCheckExpiresAtDisplay
    {
        get
        {
            if (!_hasLicenseCheckResult || string.IsNullOrEmpty(_licenseCheckExpiresAtDisplay)) return "—";
            return _licenseCheckExpiresAtDisplay;
        }
    }

    /// <summary>Server-returned error code (LICENSE_NOT_FOUND / LICENSE_EXPIRED / LICENSE_INVALID / MISSING_LICENSE_KEY).</summary>
    public string LicenseCheckErrorCodeDisplay
    {
        get
        {
            if (!_hasLicenseCheckResult) return string.Empty;
            return string.IsNullOrEmpty(_licenseCheckErrorCodeDisplay) ? "—" : _licenseCheckErrorCodeDisplay;
        }
    }

    public string LicenseCheckErrorMessageDisplay
    {
        get
        {
            if (!_hasLicenseCheckResult) return string.Empty;
            return _licenseCheckErrorMessageDisplay;
        }
    }

    public long? LicenseCheckLatencyMs
    {
        get => _licenseCheckLatencyMs;
        private set => SetProperty(ref _licenseCheckLatencyMs, value);
    }

    public string LicenseCheckTimeDisplay
    {
        get => _licenseCheckTimeDisplay;
        private set => SetProperty(ref _licenseCheckTimeDisplay, value);
    }

    /// <summary>True once a lisans kontrol call produced a result. Gates the panel visibility.</summary>
    public bool HasLicenseCheckResult
    {
        get => _hasLicenseCheckResult;
        private set => SetProperty(ref _hasLicenseCheckResult, value);
    }

    public string ServerDbCheckStatusDisplay
    {
        get
        {
            if (!_hasServerDbCheckResult) return "—";
            return string.IsNullOrEmpty(_serverDbCheckStatusDisplay) ? "(no response)" : _serverDbCheckStatusDisplay;
        }
        private set => SetProperty(ref _serverDbCheckStatusDisplay, value);
    }

    public string ServerDbCheckHttpDisplay
    {
        get
        {
            if (!_hasServerDbCheckResult) return "—";
            return string.IsNullOrEmpty(_serverDbCheckHttpDisplay) ? "(no response)" : _serverDbCheckHttpDisplay;
        }
        private set => SetProperty(ref _serverDbCheckHttpDisplay, value);
    }

    public string ServerDbCheckBodyDisplay
    {
        get
        {
            if (!_hasServerDbCheckResult) return "—";
            return string.IsNullOrEmpty(_serverDbCheckBodyDisplay) ? "(empty)" : _serverDbCheckBodyDisplay;
        }
        private set => SetProperty(ref _serverDbCheckBodyDisplay, value);
    }

    public long? ServerDbCheckLatencyMs
    {
        get => _serverDbCheckLatencyMs;
        private set => SetProperty(ref _serverDbCheckLatencyMs, value);
    }

    public string ServerDbCheckTimeDisplay
    {
        get => _serverDbCheckTimeDisplay;
        private set => SetProperty(ref _serverDbCheckTimeDisplay, value);
    }

    public bool HasServerDbCheckResult
    {
        get => _hasServerDbCheckResult;
        private set => SetProperty(ref _hasServerDbCheckResult, value);
    }

    // Register-agent (POST /api/v1/agents/register) için görüntü property'leri.
    // XAML bunları bir Border panelinde bağlar; başarılıysa agentId/tenantId/
    // expiresAtUtc, başarısızsa errorCode/message gösterir.
    public bool HasRegisterResult
    {
        get => _hasRegisterResult;
        private set => SetProperty(ref _hasRegisterResult, value);
    }

    public string RegisterSuccessDisplay =>
        !_hasRegisterResult ? string.Empty
        : _registerSuccess == true ? "✓ kayıtlı"
        : "✗ başarısız";

    public string RegisterAgentIdDisplay
    {
        get => _registerAgentIdDisplay;
        private set => SetProperty(ref _registerAgentIdDisplay, value);
    }

    public string RegisterTenantIdDisplay
    {
        get => _registerTenantIdDisplay;
        private set => SetProperty(ref _registerTenantIdDisplay, value);
    }

    public string RegisterExpiresAtDisplay
    {
        get => _registerExpiresAtDisplay;
        private set => SetProperty(ref _registerExpiresAtDisplay, value);
    }

    public string RegisterErrorCodeDisplay
    {
        get => _registerErrorCodeDisplay;
        private set => SetProperty(ref _registerErrorCodeDisplay, value);
    }

    public string RegisterErrorMessageDisplay
    {
        get => _registerErrorMessageDisplay;
        private set => SetProperty(ref _registerErrorMessageDisplay, value);
    }

    public long? RegisterLatencyMs
    {
        get => _registerLatencyMs;
        private set => SetProperty(ref _registerLatencyMs, value);
    }

    public string RegisterTimeDisplay
    {
        get => _registerTimeDisplay;
        private set => SetProperty(ref _registerTimeDisplay, value);
    }

    public System.Windows.Input.ICommand SaveCommand { get; }

    public System.Windows.Input.ICommand TestConnectionCommand { get; }

    public System.Windows.Input.ICommand RedetectVersionCommand { get; }

    /// <summary>
    /// Central API'ye kayıtlı lisans anahtarını doğrulatır
    /// (POST <c>{apiBaseUrl}/api/v1/licenses/validate</c>). Lisan yoksa
    /// panel gizli kalır; 200 gelirse tenantId + expiresAtUtc gösterilir,
    /// 404/410 gelirse errorCode/message gösterilir.
    /// </summary>
    public System.Windows.Input.ICommand TestLicenseCommand { get; }

    /// <summary>
    /// Central API'nin <c>GET /health</c> endpoint'ine ping atar. Yanıt
    /// metni + HTTP status + latency gösterilir. /health, merkezi DB
    /// bağlantısını kullandığı için bu çağrı dolaylı bir DB kontrolüdür.
    /// </summary>
    public System.Windows.Input.ICommand TestServerDbCommand { get; }

    /// <summary>
    /// Agent'ı lisansla Central API'ye kaydeder
    /// (POST <c>{apiBaseUrl}/api/v1/agents/register</c>). Başarılıysa dönen
    /// JWT in-memory olarak <c>CentralApiOptions.Jwt</c>'ye yazılır — sonraki
    /// bootstrap push ve heartbeat çağrıları bu token'la imzalanır.
    /// </summary>
    public System.Windows.Input.ICommand RegisterAgentCommand { get; }

    /// <summary>Load persisted config into the view-model. Called on window open.</summary>
    public async Task LoadAsync(CancellationToken ct = default)
    {
        try
        {
            // WPF rule: do NOT use ConfigureAwait(false) here — the await must
            // resume on the captured UI SynchronizationContext, otherwise the
            // following property mutations fire from a worker thread and WPF's
            // binding subsystem throws InvalidOperationException, which kills
            // the process when propagated from the App.xaml.cs Loaded event
            // handler. Same rule applies to Save/TestConnection/Redetect.
            var config = await _store.LoadAsync(ct);
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
            ApiBaseUrl = config.ApiBaseUrl ?? string.Empty;
            UseWindowsAuth = config.UseWindowsAuth;
            // Faz 10: multi-firm Mikro — CompanyNo / BranchNo / WarehouseNo
            // round-trip through the persisted AgentConfig and the Mikro
            // connection settings so the bootstrap reader filters by the
            // operator's chosen company/warehouse.
            CompanyNo = config.CompanyNo.ToString(CultureInfo.InvariantCulture);
            BranchNo = config.BranchNo.ToString(CultureInfo.InvariantCulture);
            WarehouseNo = config.WarehouseNo.ToString(CultureInfo.InvariantCulture);

            // Push the saved Mikro credentials into the live
            // MutableMemoryConfigurationProvider so the bootstrap path
            // (which reads IConfiguration on every cycle) sees the same
            // values the test-connection button sees. Without this the
            // MikroAdapterFactory's re-read at Create() time would still
            // see the empty appsettings.json Mikro section on the first
            // launch after process restart, before the user clicks Kaydet.
            WriteMikroSectionToConfiguration(config);

            // A previously-saved config is on disk — show the Pano tab so the
            // operator can see the last sync status. The exact timestamp is
            // fetched by the dashboard service when the tab is opened.
            HasSavedConfig = true;
            Status = "Konfigürasyon yüklendi.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AgentConfig load failed.");
            _ = App.ReportExceptionAsync(ex, "Load agent configuration");
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
            // No ConfigureAwait(false) — see LoadAsync for the WPF rationale.
            await _store.SaveAsync(config);

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

            // Reveal the Pano tab the first time the operator lands a real
            // configuration — and stamp the footer with the moment of the
            // save so the operator can tell at a glance when the last write
            // happened.
            HasSavedConfig = true;
            LastSavedAtDisplay = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);

            Status = "Konfigürasyon kaydedildi.";
        }
        catch (Exception ex)
        {
            // SQL password is excluded from this log statement — the catch only
            // carries the exception message + class name, never the DTO.
            _logger.LogError(ex,
                "AgentConfig save failed for Server={Server}, Database={Database}.",
                SqlServer, MikroDatabaseName);
            _ = App.ReportExceptionAsync(ex, "Save agent configuration");
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

            // No ConfigureAwait(false) — see LoadAsync for the WPF rationale.
            var result = await _orchestrator.RunFullTestAsync();
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
            _ = App.ReportExceptionAsync(ex, "Mikro connection test");
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
            // No ConfigureAwait(false) — see LoadAsync for the WPF rationale.
            var result = await _orchestrator.RunFullTestAsync();
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
            _ = App.ReportExceptionAsync(ex, "Mikro version detection");
            Status = "Versiyon tespiti başarısız: " + ConnectionStringMasker.MaskForLog(ex.Message);
            TroubleshootingHint = BuildTroubleshootingHint(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Faz 8 — Central API üzerinden lisans anahtarını doğrular. Endpoint
    /// <c>POST {apiBaseUrl}/api/v1/licenses/validate</c>. Mutating değildir;
    /// yalnızca mevcut lisansın aktif + expire olmamış + tenant aktif olduğunu
    /// kontrol eder. 200 → <see cref="LicenseCheckValidDisplay"/> "✓ true";
    /// 404/410 → "✗ false" + errorCode (LICENSE_NOT_FOUND / LICENSE_EXPIRED).
    /// </summary>
    /// <remarks>
    /// No ConfigureAwait(false) — WPF UI bağlamında devam etmek zorunda
    /// (bkz. <see cref="LoadAsync"/>'in tepesindeki uzun açıklama).
    /// </remarks>
    private async Task TestLicenseAsync()
    {
        var baseUrl = (ApiBaseUrl ?? string.Empty).TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
        {
            Status = "Lisans kontrolü başarısız: API base URL boş.";
            HasLicenseCheckResult = false;
            return;
        }

        var licenseKey = (LicenseKey ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(licenseKey))
        {
            Status = "Lisans kontrolü başarısız: lisans anahtarı boş.";
            HasLicenseCheckResult = false;
            return;
        }

        IsBusy = true;
        try
        {
            var url = baseUrl + "/api/v1/licenses/validate";
            var sw = System.Diagnostics.Stopwatch.StartNew();
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(new { licenseKey }),
            };
            request.Headers.Accept.ParseAdd("application/json");

            using var response = await _http.SendAsync(request).ConfigureAwait(true);
            sw.Stop();
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

            LicenseCheckLatencyMs = sw.ElapsedMilliseconds;
            LicenseCheckTimeDisplay = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            HasLicenseCheckResult = true;

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                _licenseCheckValid = root.TryGetProperty("valid", out var v) && v.ValueKind == JsonValueKind.True;
                if (root.TryGetProperty("tenantId", out var t) && t.ValueKind == JsonValueKind.String
                    && Guid.TryParse(t.GetString(), out var tenantGuid))
                {
                    _licenseCheckTenantIdDisplay = tenantGuid.ToString();
                }
                else
                {
                    _licenseCheckTenantIdDisplay = string.Empty;
                }
                if (root.TryGetProperty("expiresAtUtc", out var e) && e.ValueKind == JsonValueKind.String
                    && DateTimeOffset.TryParse(e.GetString(), CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var exp))
                {
                    _licenseCheckExpiresAtDisplay = exp.ToLocalTime()
                        .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                }
                else
                {
                    _licenseCheckExpiresAtDisplay = string.Empty;
                }
                _licenseCheckErrorCodeDisplay = root.TryGetProperty("errorCode", out var ec) && ec.ValueKind == JsonValueKind.String
                    ? ec.GetString() ?? string.Empty
                    : string.Empty;
                _licenseCheckErrorMessageDisplay = root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String
                    ? m.GetString() ?? string.Empty
                    : string.Empty;

                OnPropertyChanged(nameof(LicenseCheckValidDisplay));
                OnPropertyChanged(nameof(LicenseCheckTenantIdDisplay));
                OnPropertyChanged(nameof(LicenseCheckExpiresAtDisplay));
                OnPropertyChanged(nameof(LicenseCheckErrorCodeDisplay));
                OnPropertyChanged(nameof(LicenseCheckErrorMessageDisplay));

                _logger.LogInformation(
                    "License validate OK. Valid={Valid}, TenantId={TenantId}, ExpiresAtUtc={ExpiresAtUtc}, LatencyMs={Latency}.",
                    _licenseCheckValid, _licenseCheckTenantIdDisplay, _licenseCheckExpiresAtDisplay, sw.ElapsedMilliseconds);

                Status = _licenseCheckValid == true
                    ? $"Lisans geçerli.\nTenant: {_licenseCheckTenantIdDisplay}\nExpires: {_licenseCheckExpiresAtDisplay}"
                    : $"Lisans geçersiz: {_licenseCheckErrorCodeDisplay}";
            }
            else
            {
                _licenseCheckValid = false;
                _licenseCheckTenantIdDisplay = string.Empty;
                _licenseCheckExpiresAtDisplay = string.Empty;

                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;
                    _licenseCheckErrorCodeDisplay = root.TryGetProperty("errorCode", out var ec) && ec.ValueKind == JsonValueKind.String
                        ? ec.GetString() ?? string.Empty
                        : $"HTTP {(int)response.StatusCode}";
                    _licenseCheckErrorMessageDisplay = root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String
                        ? m.GetString() ?? string.Empty
                        : body;
                }
                catch
                {
                    _licenseCheckErrorCodeDisplay = $"HTTP {(int)response.StatusCode}";
                    _licenseCheckErrorMessageDisplay = body;
                }

                OnPropertyChanged(nameof(LicenseCheckValidDisplay));
                OnPropertyChanged(nameof(LicenseCheckTenantIdDisplay));
                OnPropertyChanged(nameof(LicenseCheckExpiresAtDisplay));
                OnPropertyChanged(nameof(LicenseCheckErrorCodeDisplay));
                OnPropertyChanged(nameof(LicenseCheckErrorMessageDisplay));

                _logger.LogWarning(
                    "License validate FAILED. StatusCode={StatusCode}, ErrorCode={ErrorCode}, LatencyMs={Latency}.",
                    (int)response.StatusCode, _licenseCheckErrorCodeDisplay, sw.ElapsedMilliseconds);

                Status = $"Lisans doğrulama başarısız: HTTP {(int)response.StatusCode} {_licenseCheckErrorCodeDisplay}";
            }
        }
        catch (Exception ex)
        {
            HasLicenseCheckResult = true;
            _ = App.ReportExceptionAsync(ex, "License validation");
            _licenseCheckValid = null;
            _licenseCheckErrorCodeDisplay = "CLIENT_ERROR";
            _licenseCheckErrorMessageDisplay = ex.Message;
            OnPropertyChanged(nameof(LicenseCheckValidDisplay));
            OnPropertyChanged(nameof(LicenseCheckErrorCodeDisplay));
            OnPropertyChanged(nameof(LicenseCheckErrorMessageDisplay));
            _logger.LogError(ex, "License validate client-side failure.");
            Status = "Lisans kontrolü başarısız: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Faz 8 — Central API'nin <c>GET /health</c> endpoint'ine ping atar.
    /// Merkezi sunucu + DB bağlantısının canlı olduğunu doğrular (health
    /// check DB bağlantısı kuran EF context'i açar). Yanıt metni + HTTP
    /// status + latency <see cref="ServerDbCheckStatusDisplay"/> alanlarına
    /// yazılır.
    /// </summary>
    private async Task TestServerDbAsync()
    {
        var baseUrl = (ApiBaseUrl ?? string.Empty).TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
        {
            Status = "Sunucu DB kontrolü başarısız: API base URL boş.";
            HasServerDbCheckResult = false;
            return;
        }

        IsBusy = true;
        try
        {
            var url = baseUrl + "/health";
            var sw = System.Diagnostics.Stopwatch.StartNew();
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.ParseAdd("application/json");

            using var response = await _http.SendAsync(request).ConfigureAwait(true);
            sw.Stop();
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

            ServerDbCheckLatencyMs = sw.ElapsedMilliseconds;
            ServerDbCheckTimeDisplay = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            ServerDbCheckHttpDisplay = ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture)
                + " " + (response.ReasonPhrase ?? string.Empty);
            ServerDbCheckBodyDisplay = string.IsNullOrWhiteSpace(body) ? "(empty body)" : body;
            ServerDbCheckStatusDisplay = response.IsSuccessStatusCode ? "OK" : "FAIL";
            HasServerDbCheckResult = true;

            OnPropertyChanged(nameof(ServerDbCheckStatusDisplay));
            OnPropertyChanged(nameof(ServerDbCheckHttpDisplay));
            OnPropertyChanged(nameof(ServerDbCheckBodyDisplay));

            _logger.LogInformation(
                "Server health check OK. Url={Url}, StatusCode={StatusCode}, LatencyMs={Latency}.",
                url, (int)response.StatusCode, sw.ElapsedMilliseconds);

            Status = response.IsSuccessStatusCode
                ? $"Sunucu erişilebilir.\nHTTP: {ServerDbCheckHttpDisplay}\nLatency: {sw.ElapsedMilliseconds} ms"
                : $"Sunucu hata döndü.\nHTTP: {ServerDbCheckHttpDisplay}\nBody: {ServerDbCheckBodyDisplay}";
        }
        catch (Exception ex)
        {
            HasServerDbCheckResult = true;
            _ = App.ReportExceptionAsync(ex, "License server database check");
            ServerDbCheckStatusDisplay = "FAIL";
            ServerDbCheckHttpDisplay = "—";
            ServerDbCheckBodyDisplay = ex.Message;
            OnPropertyChanged(nameof(ServerDbCheckStatusDisplay));
            OnPropertyChanged(nameof(ServerDbCheckHttpDisplay));
            OnPropertyChanged(nameof(ServerDbCheckBodyDisplay));
            _logger.LogError(ex, "Server health check client-side failure. BaseUrl={BaseUrl}.", baseUrl);
            Status = "Sunucu DB kontrolü başarısız: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Agent'ı Central API'ye kaydeder (POST <c>{baseUrl}/api/v1/agents/register</c>).
    /// Bu çağrı, lisans anahtarı + makine kimliğinden yeni bir JWT üretir ve
    /// agent'ı veritabanına yazar. Dönen JWT in-memory olarak
    /// <c>CentralApiOptions.Jwt</c>'ye yazılır — sonraki bootstrap push ve
    /// heartbeat çağrıları bu token'la imzalanır. Aynı (lisans, makine) için
    /// idempotent — tekrar register, JWT'yi yenilemez, sadece var olan
    /// agent'ın LicenseKey alanını günceller (yeni JWT vermez).
    /// </summary>
    private async Task RegisterAgentAsync()
    {
        var baseUrl = (ApiBaseUrl ?? string.Empty).TrimEnd('/');
        if (string.IsNullOrEmpty(baseUrl))
        {
            Status = "Agent kayıt başarısız: API base URL boş.";
            HasRegisterResult = false;
            return;
        }
        var licenseKey = (LicenseKey ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(licenseKey))
        {
            Status = "Agent kayıt başarısız: lisans anahtarı boş.";
            HasRegisterResult = false;
            return;
        }

        // Makine kimliği — birden fazla agent aynı lisansla farklı makinelerde
        // çalışabilsin diye bilgisayar adını kullanıyoruz. Gerçek üretimde
        // bir machine-id kaynağı (SMBIOS UUID vs.) tercih edilir.
        var machineId = (Environment.MachineName ?? "unknown").Trim();
        if (string.IsNullOrEmpty(machineId)) machineId = "unknown";

        IsBusy = true;
        try
        {
            var url = baseUrl + "/api/v1/agents/register";
            var sw = System.Diagnostics.Stopwatch.StartNew();
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(new { licenseKey, machineId }),
            };
            request.Headers.Accept.ParseAdd("application/json");
            // Register anonim — mevcut JWT'yi temizle ki yanlışlıkla eski
            // token gönderilip 401 alınmasın.
            request.Headers.Authorization = null;

            using var response = await _http.SendAsync(request).ConfigureAwait(true);
            sw.Stop();
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

            RegisterLatencyMs = sw.ElapsedMilliseconds;
            RegisterTimeDisplay = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            HasRegisterResult = true;

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(body);
                var root = doc.RootElement;
                var jwt = root.TryGetProperty("jwt", out var jt) && jt.ValueKind == JsonValueKind.String
                    ? jt.GetString() ?? string.Empty
                    : string.Empty;
                var agentId = root.TryGetProperty("agentId", out var aid) && aid.ValueKind == JsonValueKind.String
                    && Guid.TryParse(aid.GetString(), out var aidg) ? aidg : Guid.Empty;
                var tenantId = root.TryGetProperty("tenantId", out var tn) && tn.ValueKind == JsonValueKind.String
                    && Guid.TryParse(tn.GetString(), out var tng) ? tng : Guid.Empty;
                var expires = root.TryGetProperty("expiresAtUtc", out var ex) && ex.ValueKind == JsonValueKind.String
                    && DateTimeOffset.TryParse(ex.GetString(), CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var exp)
                    ? exp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture)
                    : string.Empty;

                if (string.IsNullOrWhiteSpace(jwt))
                {
                    _registerSuccess = false;
                    _registerErrorCodeDisplay = "EMPTY_RESPONSE";
                    _registerErrorMessageDisplay = "Central API boş cevap döndü (jwt alanı eksik).";
                    Status = "Agent kayıt başarısız: JWT alınamadı.";
                }
                else
                {
                    _registerSuccess = true;
                    _registerAgentIdDisplay = agentId == Guid.Empty ? string.Empty : agentId.ToString();
                    _registerTenantIdDisplay = tenantId == Guid.Empty ? string.Empty : tenantId.ToString();
                    _registerExpiresAtDisplay = expires;
                    _registerErrorCodeDisplay = string.Empty;
                    _registerErrorMessageDisplay = string.Empty;

                    // JWT'yi in-memory config'e yaz — sonraki bootstrap push
                    // ve heartbeat bu token'la gidecek. Disk'e yazmıyoruz;
                    // her açılışta yeniden register atılması kabul edilebilir
                    // ve hatta tavsiye edilen davranış (token rotation).
                    _liveSettings["CentralApi:Jwt"] = jwt;

                    _logger.LogInformation(
                        "Agent registered. MachineId={MachineId}, AgentId={AgentId}, TenantId={TenantId}, LatencyMs={Latency}.",
                        machineId, _registerAgentIdDisplay, _registerTenantIdDisplay, sw.ElapsedMilliseconds);

                    Status = $"Agent kayıt başarılı.\nAgentId: {_registerAgentIdDisplay}\nTenant: {_registerTenantIdDisplay}\nJWT in-memory olarak kaydedildi.\nExpires: {expires}";
                }
            }
            else
            {
                _registerSuccess = false;
                _registerAgentIdDisplay = string.Empty;
                _registerTenantIdDisplay = string.Empty;
                _registerExpiresAtDisplay = string.Empty;
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;
                    _registerErrorCodeDisplay = root.TryGetProperty("errorCode", out var ec) && ec.ValueKind == JsonValueKind.String
                        ? ec.GetString() ?? string.Empty
                        : $"HTTP {(int)response.StatusCode}";
                    _registerErrorMessageDisplay = root.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String
                        ? m.GetString() ?? string.Empty
                        : body;
                }
                catch
                {
                    _registerErrorCodeDisplay = $"HTTP {(int)response.StatusCode}";
                    _registerErrorMessageDisplay = body;
                }

                _logger.LogWarning(
                    "Agent register FAILED. MachineId={MachineId}, StatusCode={StatusCode}, ErrorCode={ErrorCode}, LatencyMs={Latency}.",
                    machineId, (int)response.StatusCode, _registerErrorCodeDisplay, sw.ElapsedMilliseconds);

                Status = $"Agent kayıt başarısız: HTTP {(int)response.StatusCode} {_registerErrorCodeDisplay}";
            }

            OnPropertyChanged(nameof(RegisterSuccessDisplay));
            OnPropertyChanged(nameof(RegisterAgentIdDisplay));
            OnPropertyChanged(nameof(RegisterTenantIdDisplay));
            OnPropertyChanged(nameof(RegisterExpiresAtDisplay));
            OnPropertyChanged(nameof(RegisterErrorCodeDisplay));
            OnPropertyChanged(nameof(RegisterErrorMessageDisplay));
        }
        catch (Exception ex)
        {
            HasRegisterResult = true;
            _ = App.ReportExceptionAsync(ex, "Agent registration");
            _registerSuccess = false;
            _registerErrorCodeDisplay = "EXCEPTION";
            _registerErrorMessageDisplay = ex.Message;
            OnPropertyChanged(nameof(RegisterSuccessDisplay));
            OnPropertyChanged(nameof(RegisterErrorCodeDisplay));
            OnPropertyChanged(nameof(RegisterErrorMessageDisplay));
            _logger.LogError(ex, "Agent register client-side failure. MachineId={MachineId}.", machineId);
            Status = "Agent kayıt başarısız: " + ex.Message;
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
            ApiBaseUrl = ApiBaseUrl?.Trim() ?? string.Empty,
            UseWindowsAuth = UseWindowsAuth,
            // Faz 10: parse the three new int fields defensively. Bad input
            // is coerced to the AgentConfig default so the persistence layer
            // never sees a non-integer (the underlying column is text but
            // every reader treats it as a number).
            CompanyNo = TryParseInt(CompanyNo, fallback: 1),
            BranchNo = TryParseInt(BranchNo, fallback: 0),
            WarehouseNo = TryParseInt(WarehouseNo, fallback: 1),
            ErpType = Core.Domain.ErpType.Mikro,
        };
    }

    /// <summary>
    /// Parse a WPF TextBox string into an int; return <paramref name="fallback"/>
    /// on blank / non-numeric input. Invariant culture so a Turkish-locale
    /// operator typing "1,5" doesn't accidentally treat the comma as a decimal
    /// separator and silently lose data.
    /// </summary>
    private static int TryParseInt(string? raw, int fallback)
        => int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : fallback;

    /// <summary>
    /// Project Mikro settings and the Central API URL from <paramref name="config"/>
    /// into the live <see cref="IConfiguration"/>. The remote API client reads
    /// these through <c>IOptionsMonitor</c>, so the operator's saved URL is used
    /// without requiring a process restart.
    /// </summary>
    /// <remarks>
    /// Writes go to the dedicated <see cref="MutableMemoryConfigurationProvider"/>
    /// rather than the JSON-backed root. The JSON provider would otherwise
    /// re-read the file on the next <c>Reload()</c> and erase the in-memory
    /// edits, so we explicitly target the in-memory layer.
    /// <para>
    /// <see cref="MutableMemoryConfigurationProvider"/> exposes an indexer
    /// that mutates the backing dictionary in place; the orchestrator's next
    /// read sees the freshly-typed values without us having to fire a Reload
    /// token.
    /// </para>
    /// </remarks>
    private void WriteMikroSectionToConfiguration(AgentConfig config)
    {
        const string prefix = "Mikro:";
        _liveSettings[prefix + "Server"] = config.SqlServer ?? string.Empty;
        _liveSettings[prefix + "UserId"] = config.SqlUserName ?? string.Empty;
        _liveSettings[prefix + "Password"] = config.SqlPassword ?? string.Empty;
        _liveSettings[prefix + "DatabaseName"] = config.MikroDatabaseName ?? string.Empty;
        _liveSettings[prefix + "IntegratedSecurity"] = config.UseWindowsAuth ? "true" : "false";
        // Faz 10: propagate the multi-firm numbers into the live Mikro
        // section so MikroConnectionSettings.FromConfiguration sees them on
        // the next adapter construction (and the test-connection button
        // uses the same values as the bootstrap reader).
        _liveSettings[prefix + "CompanyNo"] = config.CompanyNo.ToString(CultureInfo.InvariantCulture);
        _liveSettings[prefix + "WarehouseNo"] = config.WarehouseNo.ToString(CultureInfo.InvariantCulture);
        _liveSettings["CentralApi:BaseUrl"] = config.ApiBaseUrl ?? string.Empty;
    }

    private bool TryValidateInputs(out string error)
        => AgentSettingsValidation.TryValidate(
            SqlServer, SqlUserName, MikroDatabaseName, UseWindowsAuth, out error);

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
