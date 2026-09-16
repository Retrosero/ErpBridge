using ErpBridge.Portal.Api;
using ErpBridge.Portal.Session;
using MudBlazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<ISessionPersistence, ProtectedSessionPersistence>();

var baseUrl = builder.Configuration["CentralApi:BaseUrl"] ?? "https://localhost:7001";
builder.Services.AddHttpClient<PortalApiClient>(client =>
{
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

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
