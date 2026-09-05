using System.Collections.Generic;
using System.Windows;
using ErpBridge.Agent.UI.DependencyInjection;
using ErpBridge.Agent.UI.Services;
using ErpBridge.Agent.UI.ViewModels;
using ErpBridge.Agent.UI.Views;
using ErpBridge.LocalStore.Sqlite.Migrations;
using H.NotifyIcon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace ErpBridge.Agent.UI;

/// <summary>
/// WPF application bootstrap. Wires the DI container, configures Serilog,
/// ensures the local SQLite schema is up-to-date, builds the system-tray
/// icon, and resolves the main window + view-model from the container.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// In-memory configuration provider that holds the live Mikro settings
    /// written by the WPF "Kaydet" / "Bağlantıyı test et" buttons.
    /// Registered as a singleton on the DI container so the view-model and
    /// the orchestrator resolve the same instance — edits made by the
    /// view-model are visible to <c>MikroConnectionSettings.FromConfiguration</c>
    /// on the next read.
    /// </summary>
    public static MutableMemoryConfigurationProvider? LiveSettings { get; private set; }

    /// <summary>True when the operator used the tray menu's "Çıkış" item to terminate the agent.</summary>
    public static bool ExitRequested { get; private set; }

    /// <summary>
    /// Service provider for code-behind that needs DI access (e.g. UserControls
    /// instantiated by XAML that can't use constructor injection). Set in
    /// <see cref="OnStartup"/> before any view is loaded; <c>null</c> outside
    /// the application lifetime.
    /// </summary>
    public static IServiceProvider? Services { get; private set; }

    /// <summary>Best-effort remote error reporting after the DI container is ready.</summary>
    public static DesktopAgentTelemetryReporter? TelemetryReporter { get; private set; }

    private ServiceProvider? _services;
    private TaskbarIcon? _tray;
    private IDesktopSignalService? _signalService;
    private DesktopHeartbeatService? _heartbeatService;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1) Wire the global unhandled-exception hooks FIRST so even startup
        // failures leave a trace.
        var bootLogger = BuildBootstrapLogger();
        HookGlobalExceptionHandlers(bootLogger);

        // 2) Configuration + Mikro live-settings provider.
        var liveSource = new MutableMemoryConfigurationSource(new MemoryConfigurationSource
        {
            InitialData = new Dictionary<string, string?>
            {
                ["Mikro:Server"] = string.Empty,
                ["Mikro:UserId"] = string.Empty,
                ["Mikro:Password"] = string.Empty,
                ["Mikro:DatabaseName"] = string.Empty,
                ["Mikro:IntegratedSecurity"] = "false",
            },
        });

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.example.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables(prefix: "ERPBridge_")
            .Add(liveSource)
            .Build();

        var root = (IConfigurationRoot)configuration;
        LiveSettings = (MutableMemoryConfigurationProvider)root
            .Providers
            .OfType<MutableMemoryConfigurationProvider>()
            .Single();

        // 3) DI container.
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(LiveSettings);
        services.AddErpBridgeAgentUi(configuration);
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<MainWindow>();

        _services = services.BuildServiceProvider(validateScopes: true);
        TelemetryReporter = _services.GetRequiredService<DesktopAgentTelemetryReporter>();

        // 4) Ensure SQLite schema is up-to-date.
        var migrationRunner = _services.GetRequiredService<MigrationRunner>();
        var startupLogger = _services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger<App>();

        try
        {
            // Stay on the UI thread — Window.Show() and the LiveSettings wiring
            // below must run on the dispatcher.
            await migrationRunner.EnsureSchemaAsync();
            startupLogger.LogInformation("ErpBridge Agent UI starting up — schema ready.");
        }
        catch (Exception ex)
        {
            startupLogger.LogError(ex, "Local SQLite migration failed at startup.");
            _ = ReportExceptionAsync(ex, "SQLite migration", "FATAL");
        }

        // 5) System tray icon. Built in code-behind so we can wire menu
        // items, double-click restore, and exit-on-demand without XAML
        // boilerplate. The icon survives window minimize/close so the agent
        // keeps "running" in the system tray.
        _tray = BuildTrayIcon(_services.GetRequiredService<ILoggerFactory>().CreateLogger("App.Tray"));
        _tray.Visibility = Visibility.Visible;

        // 6) Resolve and show the main window.
        // IMPORTANT: Services MUST be set before MainWindow is resolved, because
        // MainWindow's XAML instantiates UserControls (DashboardView) whose
        // constructors need App.Services. Otherwise the UserControls see a null
        // App.Services and silently fail their DataContext override.
        Services = _services;

        var window = _services.GetRequiredService<MainWindow>();
        var viewModel = _services.GetRequiredService<AgentSettingsViewModel>();
        window.DataContext = viewModel;
        window.Loaded += async (_, _) => await viewModel.LoadAsync();
        MainWindow = window;
        window.Show();

        // Phase 9: long-poll the central API for "new bootstrap package
        // available" notifications. The callback re-uses the singleton
        // DashboardViewModel's RefreshFromSignalAsync so the operator sees
        // the fresh snapshot without touching the UI. A missing
        // IDesktopSignalService would only happen if DI registration is
        // broken; the try/catch keeps the rest of the app usable so the
        // operator can see and fix the misconfiguration.
        try
        {
            var dashboardVm = _services.GetRequiredService<DashboardViewModel>();
            _signalService = _services.GetRequiredService<IDesktopSignalService>();
            _signalService.Start(cursor => dashboardVm.RefreshFromSignalAsync(cursor));
            startupLogger.LogInformation(
                "Desktop signal service started — waiting for central-API bootstrap notifications.");
        }
        catch (Exception ex)
        {
            startupLogger.LogError(ex,
                "Failed to start the desktop signal service. The UI will still work but live updates are disabled.");
            _ = ReportExceptionAsync(ex, "Desktop signal startup");
        }

        _heartbeatService = _services.GetRequiredService<DesktopHeartbeatService>();
        _heartbeatService.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        // Stop the long-poll loop before disposing the DI container so the
        // background task doesn't try to resolve services that are already torn
        // down.
        if (_signalService is not null)
        {
            try
            {
                _signalService.StopAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Signal service stop failed: {ex.Message}");
            }
            _signalService = null;
        }
        if (_heartbeatService is not null)
        {
            try { _heartbeatService.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Heartbeat service stop failed: {ex.Message}"); }
            _heartbeatService = null;
        }
        TelemetryReporter = null;
        _services?.Dispose();
        base.OnExit(e);
    }

    /// <summary>
    /// Build the system-tray icon. Double-click restores the main window;
    /// the right-click context menu offers "Pano" (switches to the dashboard
    /// tab) and "Çıkış" (terminates the agent). The icon is loaded from the
    /// embedded <c>pack://application:,,,/assets/tray-32.png</c> resource.
    /// </summary>
    private TaskbarIcon BuildTrayIcon(Microsoft.Extensions.Logging.ILogger logger)
    {
        var menu = new System.Windows.Controls.ContextMenu();

        var showItem = new System.Windows.Controls.MenuItem { Header = "Pencereyi Göster" };
        showItem.Click += (_, _) => ShowMainWindow();
        menu.Items.Add(showItem);

        var dashboardItem = new System.Windows.Controls.MenuItem { Header = "Pano" };
        dashboardItem.Click += (_, _) => ShowMainWindow();
        menu.Items.Add(dashboardItem);

        menu.Items.Add(new System.Windows.Controls.Separator());

        var exitItem = new System.Windows.Controls.MenuItem { Header = "Çıkış" };
        exitItem.Click += (_, _) =>
        {
            logger.LogInformation("Operator requested exit from tray menu.");
            ExitRequested = true;
            _tray?.Dispose();
            _tray = null;
            Shutdown();
        };
        menu.Items.Add(exitItem);

        var tray = new TaskbarIcon
        {
            ToolTipText = "ErpBridge Agent — arka planda çalışıyor",
            // Use the Icon property (System.Drawing.Icon) rather than
            // IconSource (ImageSource). IconSource would require H.NotifyIcon
            // to convert a PNG to a System.Drawing.Icon at runtime, which
            // raises "Argument 'picture' must be a picture that can be used
            // as a Icon." for embedded PNG resources. The .ico resource is
            // already a valid Windows icon so we can hand it over as-is.
            Icon = LoadTrayIcon(),
            ContextMenu = menu,
        };
        tray.TrayMouseDoubleClick += (_, _) => ShowMainWindow();

        return tray;
    }

    /// <summary>
    /// Load the embedded <c>assets/icon.ico</c> as a
    /// <see cref="System.Drawing.Icon"/>. The file is compiled into the
    /// assembly as a <c>Resource</c> by the csproj, so we resolve it via
    /// <see cref="Application.GetResourceStream"/>. We fall back to the
    /// PNG (and to <see cref="SystemIcons.Application"/>) when the ICO
    /// resource is missing — keeps the agent runnable on developer
    /// machines where the generator script has not been run yet.
    /// </summary>
    private static System.Drawing.Icon LoadTrayIcon()
    {
        try
        {
            var streamInfo = GetResourceStream(new System.Uri("pack://application:,,,/assets/icon.ico"));
            if (streamInfo?.Stream is not null)
            {
                using (streamInfo.Stream)
                {
                    return new System.Drawing.Icon(streamInfo.Stream);
                }
            }
        }
        catch (Exception ex)
        {
            // Fall through to the SystemIcons fallback — the operator
            // gets a working tray icon even when the .ico resource is
            // broken. The exception is intentionally swallowed; the boot
            // log already shows a critical entry if this happens twice.
            System.Diagnostics.Debug.WriteLine($"Tray ICO load failed: {ex.Message}");
        }

        return System.Drawing.SystemIcons.Application;
    }

    /// <summary>
    /// Bring the main window back to the foreground. Idempotent — safe to
    /// call from the tray's double-click and from the context menu's
    /// "Pencereyi Göster" item.
    /// </summary>
    private void ShowMainWindow()
    {
        if (MainWindow is null) return;
        MainWindow.Show();
        MainWindow.WindowState = WindowState.Normal;
        MainWindow.Activate();
        MainWindow.Topmost = true;
        MainWindow.Topmost = false;
        MainWindow.Focus();
    }

    /// <summary>
    /// Build a Serilog logger from the on-disk appsettings.json so we have a
    /// sink to write to BEFORE the DI container is constructed. Used only by
    /// the global exception hooks below.
    /// </summary>
    private static Microsoft.Extensions.Logging.ILogger BuildBootstrapLogger()
    {
        var bootstrapConfig = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.example.json", optional: true)
            .AddEnvironmentVariables(prefix: "ERPBridge_")
            .Build();

        var serilog = new LoggerConfiguration()
            .ReadFrom.Configuration(bootstrapConfig)
            .Enrich.FromLogContext()
            .WriteTo.File(
                "logs/ui-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        return new LoggerFactory().AddSerilog(serilog).CreateLogger("App.Bootstrap");
    }

    /// <summary>
    /// Catch every otherwise-uncaught exception and route it through Serilog
    /// before WPF / .NET kills the process. Each handler is best-effort —
    /// once one of them fires the process is in an unrecoverable state, but
    /// at least the operator gets a stack trace instead of a silent death.
    /// </summary>
    private static void HookGlobalExceptionHandlers(Microsoft.Extensions.Logging.ILogger logger)
    {
        Current.DispatcherUnhandledException += (_, args) =>
        {
            logger.LogCritical(args.Exception,
                "UNHANDLED DISPATCHER EXCEPTION (UI thread) — handled to keep the process alive.");
            _ = ReportExceptionAsync(args.Exception, "Unhandled UI exception", "FATAL");
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            logger.LogCritical(args.ExceptionObject as Exception,
                "UNHANDLED APPDOMAIN EXCEPTION (non-UI thread) — process likely terminating.");
            if (args.ExceptionObject is Exception exception)
                _ = ReportExceptionAsync(exception, "Unhandled background exception", "FATAL");
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            logger.LogCritical(args.Exception,
                "UNOBSERVED TASK EXCEPTION (async void / fire-and-forget) — marking observed.");
            _ = ReportExceptionAsync(args.Exception, "Unobserved task exception", "FATAL");
            args.SetObserved();
        };
    }

    /// <summary>Report an exception without letting remote diagnostics alter UI flow.</summary>
    public static Task ReportExceptionAsync(Exception exception, string operation, string severity = "ERROR")
        => TelemetryReporter?.ReportExceptionAsync(exception, operation, severity) ?? Task.CompletedTask;
}
