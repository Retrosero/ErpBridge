using ErpBridge.Diagnostics;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Session;
using MudBlazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Log Merkezi L2e: warning+ lines go to the CentralApi's log centre when Logs:InternalIngestKey is set.
builder.Logging.AddRemoteLogShipping(builder.Configuration, "portal");
builder.Services.AddTransient<CorrelationIdHandler>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddMudServices(options =>
{
    options.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    options.SnackbarConfiguration.VisibleStateDuration = 4000;
});

// One session per circuit. Never a singleton: that would share one company's token with
// every visitor (see PortalSession).
builder.Services.AddScoped<PortalSession>();
builder.Services.AddScoped<ISessionPersistence, ProtectedBrowserPersistence>();
var keysPath = builder.Services.AddPortalDataProtection(builder.Configuration);

var baseUrl = builder.Configuration["CentralApi:BaseUrl"] ?? "https://localhost:7001";
builder.Services.AddHttpClient<PortalApiClient>(client =>
{
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<CorrelationIdHandler>();

// Warehouse TV boards (plan step 7): anonymous pairing and the screen's own token, never a person's session.
builder.Services.AddHttpClient<DisplayApiClient>(client =>
{
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(40);
}).AddHttpMessageHandler<CorrelationIdHandler>();
builder.Services.AddScoped<IDisplaySessionStore, ProtectedDisplaySessionStore>();
builder.Services.AddSingleton(new KioskTiming());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    if (keysPath is null)
    {
        app.Logger.LogWarning(
            "{Setting} is not set: session encryption keys live only in this container, so every redeploy signs every user out.",
            PortalDataProtection.KeysPathSetting);
    }
    else if (PortalDataProtection.IsMountPoint(keysPath) == false)
    {
        app.Logger.LogWarning(
            "Session encryption keys are written to {Path}, but no volume is mounted there: they are lost on redeploy and every user is signed out. Mount a persistent volume at {Path}.",
            keysPath, keysPath);
    }
}

// Log Merkezi L2e/L2f: the /Error page's code is the CorrelationId of the exception the handler logs.
app.UseRequestCorrelationScope();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    // The portal shows a company's figures; it must not be framed by another site.
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
