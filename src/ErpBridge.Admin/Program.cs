using ErpBridge.Admin.Api;
using ErpBridge.Admin.Auth;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// In-memory admin token holder — process-wide, never persisted to disk so
// the panel can be restarted without leaking an admin bearer.
builder.Services.AddSingleton<TokenStore>();

// Auth state provider wired to the token holder so <AuthorizeView> reflects
// the login state immediately after a successful Login.
builder.Services.AddScoped<AuthenticationStateProvider, AdminAuthStateProvider>();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// Typed HTTP client for the central API. The base address comes from
// "CentralApi:BaseUrl" — defaults to https://localhost:7001 in development.
var baseUrl = builder.Configuration["CentralApi:BaseUrl"] ?? "https://localhost:7001";
builder.Services.AddHttpClient<CentralApiClient>(client =>
{
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();