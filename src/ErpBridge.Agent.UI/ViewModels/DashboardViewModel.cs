using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using ErpBridge.Agent.UI.DependencyInjection;
using ErpBridge.Core.Authentication;
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
    private readonly IErpChangeLogSyncService _changeSet;
    private readonly IConfiguration _configuration;
    private readonly MutableMemoryConfigurationProvider _liveSettings;
    private readonly IAgentConfigStore _configStore;
    private readonly IErpAdapterFactory _adapterFactory;
    private readonly IAgentTokenService _tokens;
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

    public DashboardViewModel(
        IBootstrapSyncService bootstrap,
        IErpChangeLogSyncService changeSet,
        IConfiguration configuration,
        MutableMemoryConfigurationProvider liveSettings,
        IAgentConfigStore configStore,
        IErpAdapterFactory adapterFactory,
        IAgentTokenService tokens,
        ILogger<DashboardViewModel> logger)
    {
        _bootstrap = bootstrap ?? throw new ArgumentNullException(nameof(bootstrap));
        _changeSet = changeSet ?? throw new ArgumentNullException(nameof(changeSet));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _liveSettings = liveSettings ?? throw new ArgumentNullException(nameof(liveSettings));
        _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
        _adapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        RefreshCommand = new AsyncRelayCommand(_ => RefreshAsync());
        RunBootstrapCommand = new AsyncRelayCommand(
            execute: _ => RunBootstrapAsync(),
            canExecute: () => !IsBusy);
        // "Sıfırdan Kur" — sunucudaki snapshot'ı tamamen değiştirir.
        // Rutin döngü bunu yapamaz: tenant'ın aktif snapshot'ı olduğu sürece
        // ajan kalıcı olarak artımlı moddadır ve artımlı okuma ERP'den silinmiş
        // bir satırı bildiremez (satır delta'da yoktur, sunucu bunu
        // "değişmedi" diye okur). Silinen/iptal edilen kayıtlar bu yüzden
        // snapshot'ta kalır ve sıfırdan kurulan her cihaza servis edilir.
        RebuildSnapshotCommand = new AsyncRelayCommand(
            execute: _ => RebuildSnapshotAsync(),
            canExecute: () => !IsBusy);
        // "Senkronize Et" — Faz 12 trigger tabanlı change-set yolu.
        // Son sync'ten bu yana değişen INSERT/UPDATE/DELETE satırlarını
        // gönderir; tüm veriyi değil. Sık kullanım için tasarlandı
        // (bandwidth + latency düşük, idempotency garanti).
        RunSyncDeltaCommand = new AsyncRelayCommand(
            execute: _ => RunSyncDeltaAsync(),
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
    }

    /// <summary>Trigger a refresh — used on tab open and on the "Yenile" button.</summary>
    public System.Windows.Input.ICommand RefreshCommand { get; }

    /// <summary>
    /// Trigger-based delta sync. Sadece son sync'ten bu yana değişen
    /// (INSERT/UPDATE/DELETE) satırları gönderir. Tüm veriyi gönderen
    /// <see cref="RunBootstrapCommand"/>'ın aksine bu, hızlı ve
    /// bandwidth-dostu. "Senkronize Et" butonuna bağlıdır.
    /// </summary>
    public System.Windows.Input.ICommand RunSyncDeltaCommand { get; }

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
    /// <summary>
    /// Make sure the agent holds a usable bearer token before a push.
    ///
    /// <para>This used to register inline here with its own <c>HttpClient</c>,
    /// and it skipped the call whenever a token string was already present —
    /// expired or not. Since the central API issues 60-minute tokens and offers
    /// agents no refresh endpoint, an agent left open past that hour had every
    /// request rejected with 401, and pressing this screen's buttons could not
    /// recover it: the stale token was non-empty, so registration was skipped
    /// every time. <see cref="IAgentTokenService"/> now owns that lifecycle for
    /// the whole process.</para>
    /// </summary>
    private async Task<bool> EnsureRegisteredAsync()
    {
        try
        {
            return await _tokens.EnsureValidAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agent token check threw an exception.");
            _ = App.ReportExceptionAsync(ex, "Automatic agent registration");
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

            MikroCountSummaryDisplay = "MikroDB satır sayıları okunuyor…";
            HasMikroCountResult = true;

            var counts = await adapter.GetBootstrapRecordCountsAsync().ConfigureAwait(true);
            var customers = counts.Customers;
            var addresses = counts.CustomerAddresses;
            var contacts = counts.CustomerContacts;
            var stocks = counts.Stocks;
            var barcodes = counts.Barcodes;
            var openOrders = counts.OpenOrders;
            var cashBank = counts.CashAndBank;
            var lookups = counts.Lookups;
            var prices = counts.Prices;
            var salesConditions = counts.SalesConditions;
            var inventory = counts.Inventory;
            var customerTransactions = counts.CustomerTransactions;
            var stockTransactions = counts.StockTransactions;

            MikroCustomersCountDisplay = customers.ToString("N0", CultureInfo.CurrentCulture);
            MikroStocksCountDisplay = stocks.ToString("N0", CultureInfo.CurrentCulture);
            MikroOpenOrdersCountDisplay = openOrders.ToString("N0", CultureInfo.CurrentCulture);
            MikroCashAndBankCountDisplay = cashBank.ToString("N0", CultureInfo.CurrentCulture);
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
            _ = App.ReportExceptionAsync(ex, "MikroDB row count");
            MikroCountSummaryDisplay = "Sayım başarısız: " + ex.Message;
            HasMikroCountResult = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Trigger a refresh — used on tab open and on the "Yenile" button.</summary>
    public System.Windows.Input.ICommand RunBootstrapCommand { get; }

    /// <summary>Replaces the central snapshot with a freshly read full package.</summary>
    public System.Windows.Input.ICommand RebuildSnapshotCommand { get; }
    public System.Windows.Input.ICommand PushCustomersCommand { get; }
    public System.Windows.Input.ICommand PushStocksCommand { get; }
    public System.Windows.Input.ICommand PushOpenOrdersCommand { get; }
    public System.Windows.Input.ICommand PushCashAndBankCommand { get; }
    public System.Windows.Input.ICommand PushLookupsCommand { get; }
    public System.Windows.Input.ICommand PushPricesCommand { get; }
    public System.Windows.Input.ICommand PushInventoryCommand { get; }
    public System.Windows.Input.ICommand PushCustomerTransactionsCommand { get; }
    public System.Windows.Input.ICommand PushStockTransactionsCommand { get; }
    public System.Windows.Input.ICommand CheckMikroRowCountsCommand { get; }

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

    public async Task RunSyncDeltaAsync()
    {
        _logger.LogInformation("RunSyncDeltaAsync invoked from UI.");
        LastRunStatusDisplay = "▶ Senkronize ediliyor…";
        LastRunStatusBrush = WarningBadgeBrush;
        LastRunSummaryDisplay = "Son sync'ten bu yana değişen kayıtlar gönderiliyor…";
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
                _logger.LogWarning("RunSyncDeltaAsync aborted: auto-register failed.");
                return;
            }

            _logger.LogInformation("Step 1: running change-set sync.");
            var result = await _changeSet.RunOnceAsync().ConfigureAwait(true);
            _logger.LogInformation(
                "Step 2: RunOnceAsync returned. Success={Success}, Tables={Tables}, Upserts={Upserts}, Deletes={Deletes}, DurationMs={Duration}.",
                result.Success, result.TablesTouched, result.UpsertRowsPushed,
                result.DeleteRowsPushed, result.DurationMs);

            if (result.Success)
            {
                var totalRows = result.TotalRowsPushed;
                if (totalRows == 0)
                {
                    // Değişen kayıt yok: bu mutlaka başarı değil, sadece "boş iş".
                    // Operatör "boşuna tıkladım" demesin diye net bir mesaj.
                    LastRunSummaryDisplay = string.Format(
                        CultureInfo.CurrentCulture,
                        "Değişiklik yok · {0} ms",
                        result.DurationMs);
                    LastRunStatusDisplay = "✓ Değişiklik yok";
                    LastRunStatusBrush = SuccessBadgeBrush;
                    LastErrorDisplay = string.Empty;
                }
                else
                {
                    LastRunSummaryDisplay = string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} satır değişiklik gönderildi (güncelleme={1} silme={2}) · {3} ms",
                        totalRows, result.UpsertRowsPushed, result.DeleteRowsPushed, result.DurationMs);
                    LastRunStatusDisplay = "✓ Senkronize";
                    LastRunStatusBrush = SuccessBadgeBrush;
                    LastErrorDisplay = string.Empty;
                }
                _logger.LogInformation(
                    "Manual delta sync succeeded from _ERPB_SENKRONIZASYON. Tables={Tables}, Upserts={Upserts}, Deletes={Deletes}, DurationMs={Duration}.",
                    result.TablesTouched, result.UpsertRowsPushed, result.DeleteRowsPushed, result.DurationMs);
            }
            else
            {
                LastRunSummaryDisplay = "Senkronize hatası: " + (result.ErrorCode ?? "UNKNOWN");
                LastRunStatusDisplay = "✗ Başarısız";
                LastRunStatusBrush = DangerBadgeBrush;
                LastErrorDisplay = result.ErrorMessage ?? "Bilinmeyen hata";
                _logger.LogWarning(
                    "Manual delta sync FAILED. ErrorCode={ErrorCode}, Message={Message}.",
                    result.ErrorCode, result.ErrorMessage);
            }

            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            LastRunSummaryDisplay = "Senkronize crash: " + ex.GetType().Name;
            LastRunStatusDisplay = "✗ Crash";
            LastRunStatusBrush = DangerBadgeBrush;
            LastErrorDisplay = ex.Message;
            _logger.LogError(ex, "RunSyncDeltaAsync crashed.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Rebuild the central snapshot from scratch. Destructive on the server
    /// side — the active snapshot is replaced, not merged into — so the
    /// operator confirms first.
    /// </summary>
    public async Task RebuildSnapshotAsync()
    {
        _logger.LogInformation("RebuildSnapshotAsync invoked from UI.");

        var confirmed = MessageBox.Show(
            "Sunucudaki snapshot tamamen silinip Mikro'dan baştan kurulacak."
            + Environment.NewLine + Environment.NewLine
            + "Bunu, ERP'de silinmiş kayıtlar mobil uygulamada görünmeye devam ettiğinde kullanın. "
            + "Tüm veri yeniden gönderileceği için birkaç dakika sürebilir."
            + Environment.NewLine + Environment.NewLine
            + "Devam edilsin mi?",
            "Snapshot'ı sıfırdan kur",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);
        if (confirmed != MessageBoxResult.Yes)
        {
            _logger.LogInformation("Snapshot rebuild cancelled by the operator.");
            return;
        }

        LastRunStatusDisplay = "▶ Sıfırdan kuruluyor…";
        LastRunStatusBrush = WarningBadgeBrush;
        LastRunSummaryDisplay = "Snapshot baştan kuruluyor, bu birkaç dakika sürebilir…";
        LastErrorDisplay = string.Empty;
        IsBusy = true;
        try
        {
            var registered = await EnsureRegisteredAsync().ConfigureAwait(true);
            if (!registered)
            {
                LastRunSummaryDisplay = "Agent kayıt edilemedi — Ayarlar sekmesinden lisans anahtarını doğrulayın.";
                LastRunStatusDisplay = "✗ Kayıt gerekli";
                LastRunStatusBrush = DangerBadgeBrush;
                LastErrorDisplay = "Central API'ye kayıt yapılamadı.";
                _logger.LogWarning("RebuildSnapshotAsync aborted: auto-register failed.");
                return;
            }

            await _bootstrap.InvalidateAsync().ConfigureAwait(true);
            var result = await _bootstrap.RebuildSnapshotAsync().ConfigureAwait(true);
            _logger.LogInformation(
                "Snapshot rebuild returned. Success={Success}, Customers={Customers}, Stocks={Stocks}, DurationMs={Duration}.",
                result.Success, result.CustomersCount, result.StocksCount, result.DurationMs);

            if (result.Success)
            {
                LastRunSummaryDisplay = string.Format(
                    CultureInfo.CurrentCulture,
                    "Snapshot sıfırdan kuruldu · {0} cari · {1} stok · JSON {2:F2} MB · {3} ms",
                    result.CustomersCount, result.StocksCount, result.PayloadMegabytes, result.DurationMs);
                LastRunStatusDisplay = "✓ Sıfırdan kuruldu";
                LastRunStatusBrush = SuccessBadgeBrush;
                LastErrorDisplay = string.Empty;
            }
            else
            {
                LastRunSummaryDisplay = "Hata: " + (result.ErrorCode ?? "UNKNOWN");
                LastRunStatusDisplay = "✗ Başarısız";
                LastRunStatusBrush = DangerBadgeBrush;
                LastErrorDisplay = result.ErrorMessage ?? "Bilinmeyen hata";
                _logger.LogWarning(
                    "Snapshot rebuild FAILED. ErrorCode={ErrorCode}, Message={Message}.",
                    result.ErrorCode, result.ErrorMessage);
            }

            await RefreshAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Snapshot rebuild invocation crashed.");
            _ = App.ReportExceptionAsync(ex, "Snapshot rebuild");
            LastRunSummaryDisplay = "Hata: " + ex.GetType().Name + " — " + ex.Message;
            LastRunStatusDisplay = "✗ Başarısız";
            LastRunStatusBrush = DangerBadgeBrush;
            LastErrorDisplay = "Snapshot sıfırdan kurulamadı: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
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
                    "{0} satır aktarıldı · JSON {1:F2} MB · {2} ms",
                    totalRows, result.PayloadMegabytes, result.DurationMs);
                LastRunStatusDisplay = "✓ Başarılı";
                LastRunStatusBrush = SuccessBadgeBrush;
                LastErrorDisplay = string.Empty;
                _logger.LogInformation(
                    "Manual bootstrap succeeded. TotalRows={TotalRows}, PayloadBytes={PayloadBytes}, DurationMs={Duration}.",
                    totalRows, result.PayloadBytes, result.DurationMs);
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
            _ = App.ReportExceptionAsync(ex, "Manual bootstrap");
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
                    "{0}: {1} satır aktarıldı · JSON {2:F2} MB · {3} ms",
                    displayLabel, totalRows, result.PayloadMegabytes, result.DurationMs);
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
            _ = App.ReportExceptionAsync(ex, "Push section: " + sectionName);
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

    public Task RefreshAsync()
    {
        try
        {
            var lastUtc = _bootstrap.GetLastSyncAtUtc();
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

            if (lastUtc.HasValue)
            {
                // Phase 9: the worker now runs every 60 s, so the "next eligible
                // run" hint reflects the new cadence.
                var nextEligible = lastUtc.Value.AddSeconds(60);
                if (nextEligible > DateTimeOffset.Now)
                {
                    var until = nextEligible - DateTimeOffset.Now;
                    NextEligibleRunDisplay = $"Sonraki otomatik çalıştırma: {FormatAge(until)} sonra";
                }
                else
                {
                    NextEligibleRunDisplay = "Otomatik çalıştırma için hazır";
                }
            }
            else
            {
                NextEligibleRunDisplay = "Henüz bootstrap yapılmadı";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RefreshAsync failed.");
            _ = App.ReportExceptionAsync(ex, "Dashboard refresh");
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called by <c>BootstrapSignalService</c> when the central API notifies
    /// a new bootstrap package. Refreshes the timestamp + status badge, then
    /// kicks off a Mikro row-count check so the operator sees the new totals
    /// without having to hit "Yenile" manually. Skips the row-count refresh
    /// when <see cref="IsBusy"/> is already true (e.g. a manual bootstrap is
    /// running) so the signal does not interfere with the operator's flow.
    /// </summary>
    public async Task RefreshFromSignalAsync(DateTimeOffset? cursor)
    {
        _logger.LogInformation(
            "RefreshFromSignalAsync invoked (cursor={Cursor}, isBusy={IsBusy}).",
            cursor, IsBusy);
        await RefreshAsync().ConfigureAwait(true);
        if (IsBusy)
        {
            _logger.LogDebug(
                "RefreshFromSignalAsync: skipping Mikro row-count refresh because the VM is busy.");
            return;
        }
        await CheckMikroRowCountsAsync().ConfigureAwait(true);
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
