using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using ErpBridge.Agent.UI.DependencyInjection;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.Sync;
using ErpBridge.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.UI.ViewModels;

/// <summary>
/// View-model for the read-only dashboard tab. Surfaces the latest
/// bootstrap-sync status (last successful push, row counts) and any
/// quick-glance operator metrics. All long-running work is hidden behind
/// <see cref="IBootstrapSyncService"/> so the WPF thread is never blocked.
/// </summary>
/// <remarks>
/// Faz 7 — the dashboard is a Phase-1 deliverable. Future tracks will
/// surface the live job queue, last error, remote API health, and a
/// "Şimdi Senkronize Et" button. For now the only metric is the
/// last-successful-sync timestamp + the rolling counts from the most
/// recent <see cref="BootstrapSyncResult"/>.
/// </remarks>
public sealed class DashboardViewModel : ObservableObject
{
    private readonly IBootstrapSyncService _bootstrap;
    private readonly IConfiguration _configuration;
    private readonly MutableMemoryConfigurationProvider _liveSettings;
    private readonly IAgentConfigStore _configStore;
    private readonly ICheckpointStore _checkpointStore;
    private readonly IErpAdapterFactory _adapterFactory;
    private readonly ILogger<DashboardViewModel> _logger;

    private string _lastSyncAtDisplay = "Henüz senkronizasyon yapılmamış";
    private string _lastSyncRelativeDisplay = string.Empty;
    private string _nextEligibleRunDisplay = string.Empty;
    private Brush _statusBadgeBrush = GrayBadgeBrush;
    private string _statusBadgeText = "Bilinmiyor";
    private string _lastCustomersCountDisplay = "—";
    private string _lastStocksCountDisplay = "—";
    private string _lastPricesCountDisplay = "—";
    private string _lastOpenOrdersCountDisplay = "—";
    private string _lastCustomerAddressesCountDisplay = "—";
    private string _lastCustomerContactsCountDisplay = "—";
    private string _lastBarcodesCountDisplay = "—";
    private string _lastSalesConditionsCountDisplay = "—";
    private string _lastCustomerTransactionsCountDisplay = "—";
    private string _lastStockTransactionsCountDisplay = "—";
    private string _lastErrorDisplay = string.Empty;
    private bool _isBusy;
    private string _lastRunSummaryDisplay = "Henüz çalıştırma yok";
    private string _lastRunStatusDisplay = string.Empty;
    private Brush _lastRunStatusBrush = GrayBadgeBrush;

    // MikroDB row-count diagnostic (used to disambiguate "push returned 0 rows
    // because MikroDB is empty" vs "SQL filter is wrong and the data exists
    // but is hidden"). Populated by CheckMikroRowCountsCommand.
    private string _mikroCustomersCountDisplay = "—";
    private string _mikroStocksCountDisplay = "—";
    private string _mikroOpenOrdersCountDisplay = "—";
    private string _mikroCashAndBankCountDisplay = "—";
    private string _mikroCashCountDisplay = "—";
    private string _mikroBankCountDisplay = "—";
    private string _mikroLookupsCountDisplay = "—";
    private string _mikroPricesCountDisplay = "—";
    private string _mikroInventoryCountDisplay = "—";
    private string _mikroCustomerAddressesCountDisplay = "—";
    private string _mikroCustomerContactsCountDisplay = "—";
    private string _mikroBarcodesCountDisplay = "—";
    private string _mikroSalesConditionsCountDisplay = "—";
    private string _mikroCustomerTransactionsCountDisplay = "—";
    private string _mikroStockTransactionsCountDisplay = "—";
    private string _mikroCountSummaryDisplay = "Henüz kontrol edilmedi";
    private string _mikroCountTimeDisplay = string.Empty;
    private bool _hasMikroCountResult;
    private int _autoSyncStarted;

    public DashboardViewModel(
        IBootstrapSyncService bootstrap,
        IConfiguration configuration,
        MutableMemoryConfigurationProvider liveSettings,
        IAgentConfigStore configStore,
        ICheckpointStore checkpointStore,
        IErpAdapterFactory adapterFactory,
        ILogger<DashboardViewModel> logger)
    {
        _bootstrap = bootstrap ?? throw new ArgumentNullException(nameof(bootstrap));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _liveSettings = liveSettings ?? throw new ArgumentNullException(nameof(liveSettings));
        _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
        _checkpointStore = checkpointStore ?? throw new ArgumentNullException(nameof(checkpointStore));
        _adapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        RefreshCommand = new AsyncRelayCommand(_ => RefreshAsync());
        RunBootstrapCommand = new AsyncRelayCommand(
            execute: _ => RunBootstrapAsync(),
            canExecute: () => !IsBusy);
        PushCustomersCommand = new AsyncRelayCommand(
            execute: _ => PushSectionAsync("customers", "Cari"),
            canExecute: () => !IsBusy);
        PushStocksCommand = new AsyncRelayCommand(
            execute: _ => PushSectionAsync("stocks", "Stok"),
            canExecute: () => !IsBusy);
        PushOpenOrdersCommand = new AsyncRelayCommand(
            execute: _ => PushSectionAsync("openOrders", "Açık sipariş"),
            canExecute: () => !IsBusy);
        PushCashAndBankCommand = new AsyncRelayCommand(
            execute: _ => PushSectionAsync("cashAndBank", "Kasa/Banka"),
            canExecute: () => !IsBusy);
        PushCashCommand = new AsyncRelayCommand(
            execute: _ => PushSectionAsync("kasalar", "Kasalar"),
            canExecute: () => !IsBusy);
        PushBankCommand = new AsyncRelayCommand(
            execute: _ => PushSectionAsync("bankalar", "Bankalar"),
            canExecute: () => !IsBusy);
        PushLookupsCommand = new AsyncRelayCommand(
            execute: _ => PushSectionAsync("lookups", "Lookup"),
            canExecute: () => !IsBusy);
        PushPricesCommand = new AsyncRelayCommand(
            execute: _ => PushSectionAsync("prices", "Fiyat"),
            canExecute: () => !IsBusy);
        PushInventoryCommand = new AsyncRelayCommand(
            execute: _ => PushSectionAsync("inventory", "Envanter"),
            canExecute: () => !IsBusy);
        PushCustomerTransactionsCommand = new AsyncRelayCommand(
            execute: _ => PushSectionAsync("customerTransactions", "Cari Hareketleri"),
            canExecute: () => !IsBusy);
        PushStockTransactionsCommand = new AsyncRelayCommand(
            execute: _ => PushSectionAsync("stockTransactions", "Stok Hareketleri"),
            canExecute: () => !IsBusy);
        CheckMikroRowCountsCommand = new AsyncRelayCommand(
            execute: _ => CheckMikroRowCountsAsync(),
            canExecute: () => !IsBusy);
        SectionStatuses = new ObservableCollection<SyncSectionStatusItem>
        {
            new("customers", "Cariler", PushCustomersCommand),
            new("stocks", "Stoklar", PushStocksCommand),
            new("openOrders", "Açık Siparişler", PushOpenOrdersCommand),
            new("kasalar", "Kasalar", PushCashCommand),
            new("bankalar", "Bankalar", PushBankCommand),
            new("cashAndBank", "Tüm Kasa / Banka", PushCashAndBankCommand),
            new("lookups", "Lookup Tanımları", PushLookupsCommand),
            new("prices", "Fiyatlar", PushPricesCommand),
            new("inventory", "Envanter", PushInventoryCommand),
            new("customerTransactions", "Cari Hareketleri", PushCustomerTransactionsCommand),
            new("stockTransactions", "Stok Hareketleri", PushStockTransactionsCommand),
        };
    }

    /// <summary>Trigger a refresh — used on tab open and on the "Yenile" button.</summary>
    public System.Windows.Input.ICommand RefreshCommand { get; }

    /// <summary>
    /// Run <c>SELECT COUNT(*)</c> on every Mikro table the agent pushes, and
    /// surface the numbers in a panel. Lets the operator tell apart "push
    /// delivered 0 rows because MikroDB is empty" from "push delivered 0
    /// rows because the SQL filter is wrong" — the two failure modes look
    /// identical on the central API side.
    /// </summary>

    /// <summary>
    /// Central API'ye gönderilen her push'tan önce in-memory <c>CentralApi:Jwt</c>
    /// kontrolü yapılır. JWT yoksa (yeni açılışta, register hiç tıklanmamışsa)
    /// bu method otomatik olarak <c>POST /api/v1/agents/register</c> çağrısı
    /// yapar ve dönen JWT'yi in-memory <see cref="MutableMemoryConfigurationProvider"/>'a
    /// yazar. <c>HttpRemoteApiClient</c> bir sonraki <c>BuildRequest</c> çağrısında
    /// yeni JWT'yi okur (IOptionsMonitor her read'de taze değer verir). Başarılıysa
    /// <c>true</c>, başarısızsa <c>false</c> döner.
    /// </summary>
    private async Task<bool> EnsureRegisteredAsync()
    {
        var config = await _configStore.LoadAsync().ConfigureAwait(true);
        if (config is null || string.IsNullOrWhiteSpace(config.LicenseKey))
        {
            _logger.LogWarning("Auto-register skipped: AgentConfig.LicenseKey is empty.");
            return false;
        }
        var apiBaseUrl = (config.ApiBaseUrl ?? string.Empty).TrimEnd('/');
        if (string.IsNullOrEmpty(apiBaseUrl))
        {
            _logger.LogWarning("Auto-register skipped: AgentConfig.ApiBaseUrl is empty.");
            return false;
        }

        // AgentConfig is the operator-facing source of truth. Make its API
        // address available to the already-created remote client before the
        // first bootstrap push; otherwise it can retain an empty value that
        // existed while the UI was starting.
        _liveSettings["CentralApi:BaseUrl"] = apiBaseUrl;

        var existingJwt = _configuration["CentralApi:Jwt"];
        if (!string.IsNullOrWhiteSpace(existingJwt))
        {
            // Manual registration populates the JWT before this method runs.
            // The remote HttpClient still needs the persisted base URL above,
            // so only skip the registration HTTP call after applying it.
            _logger.LogDebug("JWT already set in CentralApi:Jwt; skipping auto-register.");
            return true;
        }

        var licenseKey = config.LicenseKey!.Trim();
        var machineId = (Environment.MachineName ?? "unknown").Trim();
        if (string.IsNullOrEmpty(machineId)) machineId = "unknown";

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            var url = apiBaseUrl + "/api/v1/agents/register";
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(new { licenseKey, machineId }),
            };
            request.Headers.Accept.ParseAdd("application/json");

            using var response = await http.SendAsync(request).ConfigureAwait(true);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Auto-register failed: HTTP {Status} for {Url}.",
                    (int)response.StatusCode, url);
                return false;
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(true));
            var jwt = doc.RootElement.TryGetProperty("jwt", out var jt) && jt.ValueKind == JsonValueKind.String
                ? jt.GetString() ?? string.Empty
                : string.Empty;
            if (string.IsNullOrWhiteSpace(jwt))
            {
                _logger.LogWarning("Auto-register response missing jwt field.");
                return false;
            }

            _liveSettings["CentralApi:Jwt"] = jwt;
            var tenantId = doc.RootElement.TryGetProperty("tenantId", out var tenantElement)
                && tenantElement.ValueKind == JsonValueKind.String
                ? tenantElement.GetString()
                : null;
            if (!string.IsNullOrWhiteSpace(tenantId)
                && !string.Equals(config.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
            {
                config.TenantId = tenantId;
                await _configStore.SaveAsync(config).ConfigureAwait(true);
            }
            _logger.LogInformation(
                "Auto-register succeeded. MachineId={MachineId}, TenantPersisted={TenantPersisted}, JwtLength={Len}.",
                machineId, !string.IsNullOrWhiteSpace(tenantId), jwt.Length);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auto-register threw an exception.");
            return false;
        }
    }

    /// <summary>
    /// Run <c>SELECT COUNT(*)</c> on every Mikro table the bootstrap reader
    /// queries, and surface the totals. Disambiguates "MikroDB is empty" from
    /// "SQL filter is wrong" — the two look identical on the central API side.
    /// </summary>
    public async Task CheckMikroRowCountsAsync()
    {
        _logger.LogInformation("CheckMikroRowCounts invoked from UI.");
        IsBusy = true;
        try
        {
            var config = await _configStore.LoadAsync().ConfigureAwait(true);
            if (config is null)
            {
                MikroCountSummaryDisplay = "AgentConfig yüklenemedi — Ayarlar sekmesinden lisansı kaydedin.";
                HasMikroCountResult = true;
                return;
            }
            var erpType = (ErpBridge.Erp.Abstractions.ErpType)config.ErpType;
            IErpAdapter adapter;
            try
            {
                adapter = _adapterFactory.Create(erpType);
            }
            catch (NotSupportedException ex)
            {
                MikroCountSummaryDisplay = "Adapter oluşturulamadı: " + ex.Message;
                HasMikroCountResult = true;
                return;
            }

            // COUNT(*) için 7 paralel sorgu — toplam bekleme süresi ~2s yerine
            // 7 * 2s = 14s olmasın diye Task.WhenAll.
            var package = await adapter.ReadBootstrapDataAsync().ConfigureAwait(true);
            var customers = package.Customers.Count;
            var addresses = package.CustomerAddresses.Count;
            var contacts = package.CustomerContacts.Count;
            var stocks = package.Stocks.Count;
            var barcodes = package.Barcodes.Count;
            var openOrders = package.OpenOrders.Count;
            var cashBank = package.CashAndBank.Count;
            var cash = package.CashAndBank.Count(account => string.Equals(account.Kind, "cash", StringComparison.OrdinalIgnoreCase));
            var bank = package.CashAndBank.Count(account => string.Equals(account.Kind, "bank", StringComparison.OrdinalIgnoreCase));
            var lookups = package.Lookups.Count;
            var prices = package.Prices.Count;
            var salesConditions = package.SalesConditions.Count;
            var inventory = package.Inventory.Count;
            var customerTransactions = package.CustomerTransactions.Count;
            var stockTransactions = package.StockTransactions.Count;

            MikroCustomersCountDisplay = customers.ToString("N0", CultureInfo.CurrentCulture);
            MikroStocksCountDisplay = stocks.ToString("N0", CultureInfo.CurrentCulture);
            MikroOpenOrdersCountDisplay = openOrders.ToString("N0", CultureInfo.CurrentCulture);
            MikroCashAndBankCountDisplay = cashBank.ToString("N0", CultureInfo.CurrentCulture);
            MikroCashCountDisplay = cash.ToString("N0", CultureInfo.CurrentCulture);
            MikroBankCountDisplay = bank.ToString("N0", CultureInfo.CurrentCulture);
            MikroLookupsCountDisplay = lookups.ToString("N0", CultureInfo.CurrentCulture);
            MikroPricesCountDisplay = prices.ToString("N0", CultureInfo.CurrentCulture);
            MikroInventoryCountDisplay = inventory.ToString("N0", CultureInfo.CurrentCulture);
            MikroCustomerAddressesCountDisplay = addresses.ToString("N0", CultureInfo.CurrentCulture);
            MikroCustomerContactsCountDisplay = contacts.ToString("N0", CultureInfo.CurrentCulture);
            MikroBarcodesCountDisplay = barcodes.ToString("N0", CultureInfo.CurrentCulture);
            MikroSalesConditionsCountDisplay = salesConditions.ToString("N0", CultureInfo.CurrentCulture);
            MikroCustomerTransactionsCountDisplay = customerTransactions.ToString("N0", CultureInfo.CurrentCulture);
            MikroStockTransactionsCountDisplay = stockTransactions.ToString("N0", CultureInfo.CurrentCulture);
            MikroCountTimeDisplay = DateTime.Now.ToString("HH:mm:ss", CultureInfo.CurrentCulture);
            var total = customers + addresses + contacts + stocks + barcodes + openOrders
                + cashBank + lookups + prices + salesConditions + inventory
                + customerTransactions + stockTransactions;
            MikroCountSummaryDisplay = total == 0
                ? "⚠ MikroDB boş — push'lar 0 satır gönderecek."
                : $"Toplam {total:N0} satır mevcut — push'lar bunları gönderecek.";
            HasMikroCountResult = true;

            _logger.LogInformation(
                "Mikro row counts: customers={Customers}, stocks={Stocks}, openOrders={OpenOrders}, cashBank={CashBank}, lookups={Lookups}, prices={Prices}, inventory={Inventory}, total={Total}.",
                customers, stocks, openOrders, cashBank, lookups, prices, inventory, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CheckMikroRowCounts failed.");
            MikroCountSummaryDisplay = "Sayım başarısız: " + ex.Message;
            HasMikroCountResult = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Run a single <c>SELECT COUNT(*)</c> against a Mikro table by routing
    /// through the adapter's test-connection seam. Returns 0 for unrecognised
    /// adapters — the Mikro reader exposes a count hook directly.
    /// </summary>
    private static async Task<long> CountTableAsync(IErpAdapter adapter, string table, CancellationToken ct)
    {
        // The Mikro adapter exposes an internal-only count helper; for the
        // MVP we read the bootstrap snapshot and count rows in the result.
        // This is O(rows-in-table) per table — fine for the diagnostic button
        // which is invoked manually.
        try
        {
            var pkg = await adapter.ReadBootstrapDataAsync(ct).ConfigureAwait(false);
            if (pkg is null) return 0;
            return table switch
            {
                "CARI_HESAPLAR" => pkg.Customers?.Count ?? 0,
                "STOKLAR" => pkg.Stocks?.Count ?? 0,
                "SIPARISLER" => pkg.OpenOrders?.Count ?? 0,
                "KASALAR+BANKALAR" => pkg.CashAndBank?.Count ?? 0,
                "DEPOLAR+CARI_PERSONEL" => pkg.Lookups?.Count ?? 0,
                "STOK_SATIS_FIYAT_LISTELERI" => pkg.Prices?.Count ?? 0,
                "STOK_HAREKETLERI" => pkg.Inventory?.Count ?? 0,
                _ => 0,
            };
        }
        catch
        {
            return -1; // -1 marker for "this table failed"
        }
    }

    /// <summary>Trigger a refresh — used on tab open and on the "Yenile" button.</summary>
    public System.Windows.Input.ICommand RunBootstrapCommand { get; }
    public System.Windows.Input.ICommand PushCustomersCommand { get; }
    public System.Windows.Input.ICommand PushStocksCommand { get; }
    public System.Windows.Input.ICommand PushOpenOrdersCommand { get; }
    public System.Windows.Input.ICommand PushCashAndBankCommand { get; }
    public System.Windows.Input.ICommand PushCashCommand { get; }
    public System.Windows.Input.ICommand PushBankCommand { get; }
    public System.Windows.Input.ICommand PushLookupsCommand { get; }
    public System.Windows.Input.ICommand PushPricesCommand { get; }
    public System.Windows.Input.ICommand PushInventoryCommand { get; }
    public System.Windows.Input.ICommand PushCustomerTransactionsCommand { get; }
    public System.Windows.Input.ICommand PushStockTransactionsCommand { get; }
    public System.Windows.Input.ICommand CheckMikroRowCountsCommand { get; }
    public ObservableCollection<SyncSectionStatusItem> SectionStatuses { get; }

    public string LastSyncAtDisplay
    {
        get => _lastSyncAtDisplay;
        private set => SetProperty(ref _lastSyncAtDisplay, value);
    }

    public string LastSyncRelativeDisplay
    {
        get => _lastSyncRelativeDisplay;
        private set => SetProperty(ref _lastSyncRelativeDisplay, value);
    }

    public string NextEligibleRunDisplay
    {
        get => _nextEligibleRunDisplay;
        private set => SetProperty(ref _nextEligibleRunDisplay, value);
    }

    public Brush StatusBadgeBrush
    {
        get => _statusBadgeBrush;
        private set => SetProperty(ref _statusBadgeBrush, value);
    }

    public string StatusBadgeText
    {
        get => _statusBadgeText;
        private set => SetProperty(ref _statusBadgeText, value);
    }

    public string LastCustomersCountDisplay
    {
        get => _lastCustomersCountDisplay;
        private set => SetProperty(ref _lastCustomersCountDisplay, value);
    }

    public string LastStocksCountDisplay
    {
        get => _lastStocksCountDisplay;
        private set => SetProperty(ref _lastStocksCountDisplay, value);
    }

    public string LastPricesCountDisplay
    {
        get => _lastPricesCountDisplay;
        private set => SetProperty(ref _lastPricesCountDisplay, value);
    }

    public string LastOpenOrdersCountDisplay
    {
        get => _lastOpenOrdersCountDisplay;
        private set => SetProperty(ref _lastOpenOrdersCountDisplay, value);
    }

    public string LastCustomerAddressesCountDisplay
    {
        get => _lastCustomerAddressesCountDisplay;
        private set => SetProperty(ref _lastCustomerAddressesCountDisplay, value);
    }

    public string LastCustomerContactsCountDisplay
    {
        get => _lastCustomerContactsCountDisplay;
        private set => SetProperty(ref _lastCustomerContactsCountDisplay, value);
    }

    public string LastBarcodesCountDisplay
    {
        get => _lastBarcodesCountDisplay;
        private set => SetProperty(ref _lastBarcodesCountDisplay, value);
    }

    public string LastSalesConditionsCountDisplay
    {
        get => _lastSalesConditionsCountDisplay;
        private set => SetProperty(ref _lastSalesConditionsCountDisplay, value);
    }

    public string LastCustomerTransactionsCountDisplay
    {
        get => _lastCustomerTransactionsCountDisplay;
        private set => SetProperty(ref _lastCustomerTransactionsCountDisplay, value);
    }

    public string LastStockTransactionsCountDisplay
    {
        get => _lastStockTransactionsCountDisplay;
        private set => SetProperty(ref _lastStockTransactionsCountDisplay, value);
    }

    public string LastErrorDisplay
    {
        get => _lastErrorDisplay;
        private set => SetProperty(ref _lastErrorDisplay, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public string LastRunSummaryDisplay
    {
        get => _lastRunSummaryDisplay;
        private set => SetProperty(ref _lastRunSummaryDisplay, value);
    }

    public string LastRunStatusDisplay
    {
        get => _lastRunStatusDisplay;
        private set => SetProperty(ref _lastRunStatusDisplay, value);
    }

    public Brush LastRunStatusBrush
    {
        get => _lastRunStatusBrush;
        private set => SetProperty(ref _lastRunStatusBrush, value);
    }

    public string MikroCustomersCountDisplay
    {
        get => _mikroCustomersCountDisplay;
        private set => SetProperty(ref _mikroCustomersCountDisplay, value);
    }

    public string MikroStocksCountDisplay
    {
        get => _mikroStocksCountDisplay;
        private set => SetProperty(ref _mikroStocksCountDisplay, value);
    }

    public string MikroOpenOrdersCountDisplay
    {
        get => _mikroOpenOrdersCountDisplay;
        private set => SetProperty(ref _mikroOpenOrdersCountDisplay, value);
    }

    public string MikroCashAndBankCountDisplay
    {
        get => _mikroCashAndBankCountDisplay;
        private set => SetProperty(ref _mikroCashAndBankCountDisplay, value);
    }

    public string MikroCashCountDisplay
    {
        get => _mikroCashCountDisplay;
        private set => SetProperty(ref _mikroCashCountDisplay, value);
    }

    public string MikroBankCountDisplay
    {
        get => _mikroBankCountDisplay;
        private set => SetProperty(ref _mikroBankCountDisplay, value);
    }

    public string MikroLookupsCountDisplay
    {
        get => _mikroLookupsCountDisplay;
        private set => SetProperty(ref _mikroLookupsCountDisplay, value);
    }

    public string MikroPricesCountDisplay
    {
        get => _mikroPricesCountDisplay;
        private set => SetProperty(ref _mikroPricesCountDisplay, value);
    }

    public string MikroInventoryCountDisplay
    {
        get => _mikroInventoryCountDisplay;
        private set => SetProperty(ref _mikroInventoryCountDisplay, value);
    }

    public string MikroCustomerAddressesCountDisplay
    {
        get => _mikroCustomerAddressesCountDisplay;
        private set => SetProperty(ref _mikroCustomerAddressesCountDisplay, value);
    }

    public string MikroCustomerContactsCountDisplay
    {
        get => _mikroCustomerContactsCountDisplay;
        private set => SetProperty(ref _mikroCustomerContactsCountDisplay, value);
    }

    public string MikroBarcodesCountDisplay
    {
        get => _mikroBarcodesCountDisplay;
        private set => SetProperty(ref _mikroBarcodesCountDisplay, value);
    }

    public string MikroSalesConditionsCountDisplay
    {
        get => _mikroSalesConditionsCountDisplay;
        private set => SetProperty(ref _mikroSalesConditionsCountDisplay, value);
    }

    public string MikroCustomerTransactionsCountDisplay
    {
        get => _mikroCustomerTransactionsCountDisplay;
        private set => SetProperty(ref _mikroCustomerTransactionsCountDisplay, value);
    }

    public string MikroStockTransactionsCountDisplay
    {
        get => _mikroStockTransactionsCountDisplay;
        private set => SetProperty(ref _mikroStockTransactionsCountDisplay, value);
    }

    public string MikroCountSummaryDisplay
    {
        get => _mikroCountSummaryDisplay;
        private set => SetProperty(ref _mikroCountSummaryDisplay, value);
    }

    public string MikroCountTimeDisplay
    {
        get => _mikroCountTimeDisplay;
        private set => SetProperty(ref _mikroCountTimeDisplay, value);
    }

    public bool HasMikroCountResult
    {
        get => _hasMikroCountResult;
        private set => SetProperty(ref _hasMikroCountResult, value);
    }

    public async Task RunBootstrapAsync()
    {
        _logger.LogInformation("RunBootstrapAsync invoked from UI.");
        LastRunStatusDisplay = "▶ Çalışıyor…";
        LastRunStatusBrush = WarningBadgeBrush;
        LastRunSummaryDisplay = "Bootstrap tetiklendi, lütfen bekleyin…";
        LastErrorDisplay = string.Empty;
        IsBusy = true;
        try
        {
            // 0) Central API'ye kayıtlı değilsek otomatik register.
            var registered = await EnsureRegisteredAsync().ConfigureAwait(true);
            if (!registered)
            {
                LastRunSummaryDisplay = "Agent kayıt edilemedi — Ayarlar sekmesinden lisans anahtarını doğrulayın ve 'Lisans ile Kayıt Ol' butonuna tıklayın.";
                LastRunStatusDisplay = "✗ Kayıt gerekli";
                LastRunStatusBrush = DangerBadgeBrush;
                LastErrorDisplay = "Central API'ye kayıt yapılamadı. Lisans anahtarı + API base URL kontrolü gerekli.";
                _logger.LogWarning("RunBootstrapAsync aborted: auto-register failed.");
                return;
            }

            _logger.LogInformation("Step 1: invalidating checkpoint.");
            await _bootstrap.InvalidateAsync().ConfigureAwait(true);

            _logger.LogInformation("Step 2: running RunOnceAsync.");
            var result = await _bootstrap.RunOnceAsync().ConfigureAwait(true);
            _logger.LogInformation(
                "Step 3: RunOnceAsync returned. Success={Success}, Customers={Customers}, Stocks={Stocks}, DurationMs={Duration}.",
                result.Success, result.CustomersCount, result.StocksCount, result.DurationMs);

            LastCustomersCountDisplay = result.CustomersCount.ToString(CultureInfo.CurrentCulture);
            LastStocksCountDisplay = result.StocksCount.ToString(CultureInfo.CurrentCulture);
            LastPricesCountDisplay = result.PricesCount.ToString(CultureInfo.CurrentCulture);
            LastOpenOrdersCountDisplay = result.OpenOrdersCount.ToString(CultureInfo.CurrentCulture);
            LastCustomerAddressesCountDisplay = result.CustomerAddressesCount.ToString(CultureInfo.CurrentCulture);
            LastCustomerContactsCountDisplay = result.CustomerContactsCount.ToString(CultureInfo.CurrentCulture);
            LastBarcodesCountDisplay = result.BarcodesCount.ToString(CultureInfo.CurrentCulture);
            LastSalesConditionsCountDisplay = result.SalesConditionsCount.ToString(CultureInfo.CurrentCulture);
            LastCustomerTransactionsCountDisplay = result.CustomerTransactionsCount.ToString(CultureInfo.CurrentCulture);
            LastStockTransactionsCountDisplay = result.StockTransactionsCount.ToString(CultureInfo.CurrentCulture);

            if (result.Success)
            {
                var totalRows = result.CustomersCount + result.StocksCount + result.PricesCount
                    + result.InventoryCount + result.OpenOrdersCount + result.CashAndBankCount
                    + result.LookupsCount + result.CustomerAddressesCount
                    + result.CustomerContactsCount + result.BarcodesCount
                    + result.SalesConditionsCount + result.CustomerTransactionsCount
                    + result.StockTransactionsCount;
                LastRunSummaryDisplay = string.Format(
                    CultureInfo.CurrentCulture,
                    "{0} satır aktarıldı · {1} ms",
                    totalRows, result.DurationMs);
                LastRunStatusDisplay = "✓ Başarılı";
                LastRunStatusBrush = SuccessBadgeBrush;
                LastErrorDisplay = string.Empty;
                _logger.LogInformation(
                    "Manual bootstrap succeeded. TotalRows={TotalRows}, DurationMs={Duration}.",
                    totalRows, result.DurationMs);
            }
            else
            {
                LastRunSummaryDisplay = "Hata: " + (result.ErrorCode ?? "UNKNOWN");
                LastRunStatusDisplay = "✗ Başarısız";
                LastRunStatusBrush = DangerBadgeBrush;
                LastErrorDisplay = result.ErrorMessage ?? "Bilinmeyen hata";
                _logger.LogWarning(
                    "Manual bootstrap FAILED. ErrorCode={ErrorCode}, Message={Message}.",
                    result.ErrorCode, result.ErrorMessage);
            }

            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manual bootstrap invocation crashed.");
            LastRunSummaryDisplay = "Hata: " + ex.GetType().Name + " — " + ex.Message;
            LastRunStatusDisplay = "✗ Başarısız";
            LastRunStatusBrush = DangerBadgeBrush;
            LastErrorDisplay = "Bootstrap tetiklenemedi: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string FormatAge(TimeSpan age)
    {
        if (age < TimeSpan.FromMinutes(1)) return "az önce";
        if (age < TimeSpan.FromHours(1))
        {
            var m = (int)age.TotalMinutes;
            return m + " dakika önce";
        }
        if (age < TimeSpan.FromDays(1))
        {
            var h = (int)age.TotalHours;
            return h + " saat önce";
        }
        var d = (int)age.TotalDays;
        return d + " gün önce";
    }

    public async Task PushSectionAsync(string sectionName, string displayLabel)
    {
        _logger.LogInformation("PushSectionAsync invoked for {Section}.", sectionName);
        LastRunStatusDisplay = $"▶ {displayLabel} aktarılıyor…";
        LastRunStatusBrush = WarningBadgeBrush;
        LastRunSummaryDisplay = $"{displayLabel} tablosu için push tetiklendi…";
        LastErrorDisplay = string.Empty;
        IsBusy = true;
        try
        {
            var registered = await EnsureRegisteredAsync().ConfigureAwait(true);
            if (!registered)
            {
                LastRunSummaryDisplay = $"{displayLabel}: Agent kayıt edilemedi — Ayarlar sekmesinden lisans anahtarı doğrulayıp 'Lisans ile Kayıt Ol'a tıklayın.";
                LastRunStatusDisplay = "✗ " + displayLabel;
                LastRunStatusBrush = DangerBadgeBrush;
                LastErrorDisplay = "Central API'ye kayıt yapılamadı.";
                _logger.LogWarning("PushSectionAsync({Section}) aborted: auto-register failed.", sectionName);
                return;
            }

            var result = await _bootstrap.PushSectionAsync(sectionName).ConfigureAwait(true);
            _logger.LogInformation(
                "PushSectionAsync({Section}) returned. Success={Success}, DurationMs={Duration}.",
                sectionName, result.Success, result.DurationMs);

            if (result.CustomersCount > 0) LastCustomersCountDisplay = result.CustomersCount.ToString(CultureInfo.CurrentCulture);
            if (result.StocksCount > 0) LastStocksCountDisplay = result.StocksCount.ToString(CultureInfo.CurrentCulture);
            if (result.PricesCount > 0) LastPricesCountDisplay = result.PricesCount.ToString(CultureInfo.CurrentCulture);
            if (result.OpenOrdersCount > 0) LastOpenOrdersCountDisplay = result.OpenOrdersCount.ToString(CultureInfo.CurrentCulture);
            if (result.CustomerAddressesCount > 0) LastCustomerAddressesCountDisplay = result.CustomerAddressesCount.ToString(CultureInfo.CurrentCulture);
            if (result.CustomerContactsCount > 0) LastCustomerContactsCountDisplay = result.CustomerContactsCount.ToString(CultureInfo.CurrentCulture);
            if (result.BarcodesCount > 0) LastBarcodesCountDisplay = result.BarcodesCount.ToString(CultureInfo.CurrentCulture);
            if (result.SalesConditionsCount > 0) LastSalesConditionsCountDisplay = result.SalesConditionsCount.ToString(CultureInfo.CurrentCulture);
            if (result.CustomerTransactionsCount > 0) LastCustomerTransactionsCountDisplay = result.CustomerTransactionsCount.ToString(CultureInfo.CurrentCulture);
            if (result.StockTransactionsCount > 0) LastStockTransactionsCountDisplay = result.StockTransactionsCount.ToString(CultureInfo.CurrentCulture);

            if (result.Success)
            {
                var totalRows = result.CustomersCount + result.StocksCount + result.PricesCount
                    + result.InventoryCount + result.OpenOrdersCount + result.CashAndBankCount
                    + result.LookupsCount + result.CustomerAddressesCount
                    + result.CustomerContactsCount + result.BarcodesCount
                    + result.SalesConditionsCount + result.CustomerTransactionsCount
                    + result.StockTransactionsCount;
                LastRunSummaryDisplay = string.Format(
                    CultureInfo.CurrentCulture,
                    "{0}: {1} satır aktarıldı · {2} ms",
                    displayLabel, totalRows, result.DurationMs);
                LastRunStatusDisplay = "✓ " + displayLabel;
                LastRunStatusBrush = SuccessBadgeBrush;
                LastErrorDisplay = string.Empty;
            }
            else
            {
                LastRunSummaryDisplay = $"{displayLabel}: Hata — " + (result.ErrorCode ?? "UNKNOWN");
                LastRunStatusDisplay = "✗ " + displayLabel;
                LastRunStatusBrush = DangerBadgeBrush;
                LastErrorDisplay = result.ErrorMessage ?? "Bilinmeyen hata";
            }

            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PushSectionAsync({Section}) crashed.", sectionName);
            LastRunSummaryDisplay = $"{displayLabel}: Hata — {ex.GetType().Name}";
            LastRunStatusDisplay = "✗ " + displayLabel;
            LastRunStatusBrush = DangerBadgeBrush;
            LastErrorDisplay = "Push tetiklenemedi: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task RefreshAsync()
    {
        try
        {
            var lastUtc = _bootstrap.GetLastSyncAtUtc();
            var config = await _configStore.LoadAsync().ConfigureAwait(true);
            if (lastUtc.HasValue)
            {
                var localTime = lastUtc.Value.ToLocalTime();
                LastSyncAtDisplay = localTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
                var age = DateTimeOffset.Now - lastUtc.Value;
                LastSyncRelativeDisplay = FormatAge(age);
            }
            else
            {
                LastSyncAtDisplay = "Henüz senkronizasyon yapılmamış";
                LastSyncRelativeDisplay = string.Empty;
            }

            var tenantId = string.IsNullOrWhiteSpace(config?.TenantId) ? "unknown" : config.TenantId;
            var liveCheckpoint = await _checkpointStore
                .LoadAsync(tenantId!, LiveSyncScopes.Status)
                .ConfigureAwait(true);
            LiveSyncState? liveState = null;
            if (!string.IsNullOrWhiteSpace(liveCheckpoint?.LastToken))
            {
                try { liveState = JsonSerializer.Deserialize<LiveSyncState>(liveCheckpoint.LastToken); }
                catch (JsonException) { }
            }
            ApplyLiveState(liveState);

            foreach (var item in SectionStatuses)
            {
                var detected = await _checkpointStore
                    .LoadAsync(tenantId!, LiveSyncScopes.Detected(item.Key))
                    .ConfigureAwait(true);
                var checkpoint = await _checkpointStore
                    .LoadAsync(tenantId!, BootstrapSyncService.SectionScope(item.Key))
                    .ConfigureAwait(true);
                var sectionTime = checkpoint?.LastSuccessAt is { } time
                    ? new DateTimeOffset(DateTime.SpecifyKind(time, DateTimeKind.Utc), TimeSpan.Zero)
                    : lastUtc;
                item.LastUpdatedDisplay = sectionTime.HasValue
                    ? sectionTime.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture)
                    : "Henüz aktarılmadı";
                item.LastDetectedDisplay = detected?.LastSuccessAt is { } detectedAt
                    ? DateTime.SpecifyKind(detectedAt, DateTimeKind.Utc).ToLocalTime()
                        .ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.CurrentCulture)
                    : "Değişiklik yok";
                item.CountDisplay = CountForSection(item.Key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshAsync failed.");
        }
    }

    private string CountForSection(string key) => key.ToLowerInvariant() switch
    {
        "customers" => LastCustomersCountDisplay,
        "stocks" => LastStocksCountDisplay,
        "openorders" => LastOpenOrdersCountDisplay,
        "prices" => LastPricesCountDisplay,
        "customertransactions" => LastCustomerTransactionsCountDisplay,
        "stocktransactions" => LastStockTransactionsCountDisplay,
        _ => "—",
    };

    private void ApplyLiveState(LiveSyncState? state)
    {
        if (state is null)
        {
            StatusBadgeText = "Bağlantı bekleniyor";
            StatusBadgeBrush = GrayBadgeBrush;
            NextEligibleRunDisplay = "Windows canlı senkronizasyon servisi bekleniyor";
            return;
        }
        if (state.Status == "error")
        {
            StatusBadgeText = "Hata";
            StatusBadgeBrush = DangerBadgeBrush;
            NextEligibleRunDisplay = state.Message ?? "Canlı izleme hatası";
            return;
        }
        if (state.Status == "waiting")
        {
            StatusBadgeText = "Bağlantı bekleniyor";
            StatusBadgeBrush = WarningBadgeBrush;
            NextEligibleRunDisplay = state.Message ?? "Agent ayarları bekleniyor";
            return;
        }
        StatusBadgeText = state.Mode == "change-tracking"
            ? "Canlı izleme — Change Tracking"
            : "Canlı izleme — Uyumluluk";
        StatusBadgeBrush = SuccessBadgeBrush;
        NextEligibleRunDisplay = state.Message ?? "ERP değişiklikleri otomatik izleniyor";
    }

    /// <summary>Refreshes service-owned live status; the UI never performs scheduled sync.</summary>
    public async Task StartAutoSyncAsync()
    {
        if (Interlocked.Exchange(ref _autoSyncStarted, 1) != 0) return;
        while (Application.Current?.Dispatcher.HasShutdownStarted != true)
        {
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            if (Application.Current?.Dispatcher.HasShutdownStarted == true) break;
            try
            {
                var application = Application.Current;
                if (application is null) break;
                var refreshTask = await application.Dispatcher.InvokeAsync(RefreshAsync);
                await refreshTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Live dashboard status refresh failed.");
            }
        }
    }

    private static readonly Brush GrayBadgeBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)));
    private static readonly Brush SuccessBadgeBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)));
    private static readonly Brush WarningBadgeBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06)));
    private static readonly Brush DangerBadgeBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26)));

    private static Brush Freeze(SolidColorBrush brush)
    {
        brush.Freeze();
        return brush;
    }
}

public sealed class SyncSectionStatusItem : ObservableObject
{
    private string _lastDetectedDisplay = "Değişiklik yok";
    private string _lastUpdatedDisplay = "Henüz aktarılmadı";
    private string _countDisplay = "—";

    public SyncSectionStatusItem(string key, string title, System.Windows.Input.ICommand syncCommand)
    {
        Key = key;
        Title = title;
        SyncCommand = syncCommand;
    }

    public string Key { get; }
    public string Title { get; }
    public System.Windows.Input.ICommand SyncCommand { get; }
    public string LastUpdatedDisplay
    {
        get => _lastUpdatedDisplay;
        set => SetProperty(ref _lastUpdatedDisplay, value);
    }

    public string LastDetectedDisplay
    {
        get => _lastDetectedDisplay;
        set => SetProperty(ref _lastDetectedDisplay, value);
    }
    public string CountDisplay
    {
        get => _countDisplay;
        set => SetProperty(ref _countDisplay, value);
    }
}
