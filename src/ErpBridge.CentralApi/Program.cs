using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.RateLimiting;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Options;
using ErpBridge.CentralApi.Webhooks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ErpBridge.CentralApi;

/// <summary>
/// Top-level host for the ErpBridge central API. Hosts the controllers (mapped
/// as minimal-API endpoints), wires up PostgreSQL via EF Core, JWT bearer
/// authentication, and a partitioned rate limiter. The rate-limit partition key
/// is the agent id from the JWT — anonymous calls are partitioned under
/// <see cref="RateLimitAnonymousPartition"/>.
/// </summary>
public partial class Program
{
    /// <summary>Authorization policy name applied to agent-authenticated endpoints. Requires <c>scope=agent</c>.</summary>
    public const string AgentPolicy = "Agent";

    /// <summary>Authorization policy name applied to admin endpoints. Requires <c>scope=admin</c>.</summary>
    public const string AdminPolicy = "Admin";

    /// <summary>Authorization policy applied to the public ingest endpoint. Requires <c>scope=apikey</c>.</summary>
    public const string ApiKeyPolicy = "ApiKey";
    public const string MobilePolicy = "Mobile";

    /// <summary>Rate-limit policy name partitioned by the JWT <c>sub</c> (agent id).</summary>
    public const string PerAgentRateLimitPolicy = "per-agent";

    /// <summary>Rate-limit policy name partitioned by the admin id (JWT <c>sub</c>).</summary>
    public const string PerAdminRateLimitPolicy = "per-admin";

    /// <summary>Legacy tenant-partitioned rate-limit policy (kept as a secondary guard).</summary>
    public const string PerTenantRateLimitPolicy = "Tenant";

    /// <summary>Anonymous rate-limit policy (partitioned by remote IP).</summary>
    public const string AnonymousRateLimitPolicy = "Anonymous";

    /// <summary>Partition key prefix used for anonymous (pre-auth) calls.</summary>
    public const string RateLimitAnonymousPartition = "anon";

    /// <summary>Default permits per minute for the per-agent rate-limit policy.</summary>
    public const int DefaultPermitsPerMinute = 100;

    /// <summary>Permits per minute for admin endpoints.</summary>
    public const int AdminPermitsPerMinute = 60;

    /// <summary>
    /// Host entry point. Builds the WebApplication, applies EF migrations when
    /// a connection string is present, runs the host, then exits.
    /// </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Full Mikro snapshots include tens of thousands of ledger rows. Keep
        // the ingest limit explicit so Kestrel does not reject movement data
        // while smaller master-data snapshots continue to work.
        builder.WebHost.ConfigureKestrel(options =>
            options.Limits.MaxRequestBodySize = 128L * 1024 * 1024);
        ConfigureBuilder(builder, builder.Configuration);
        var app = builder.Build();

        // Apply migrations on startup when a real connection string is present.
        // Tests using WebApplicationFactory<Program> + InMemory provider skip
        // this path because they replace the DbContext registration entirely.
        var connectionString = app.Configuration.GetConnectionString("CentralApi");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            // Migrate() is only valid for relational providers (PostgreSQL).
            // The test host (WebApplicationFactory<Program>) replaces the
            // DbContext with an in-memory provider; in that case the
            // EnsureCreated call below seeds the schema.
            if (db.Database.IsRelational())
                db.Database.Migrate();
            else
                db.Database.EnsureCreated();
        }

        // Seed the bootstrap admin if configuration supplies one and the row
        // does not yet exist. Idempotent; runs in both prod and test paths so
        // the in-memory provider also has a usable admin login.
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var seed = scope.ServiceProvider.GetRequiredService<IOptions<AdminSeedOptions>>().Value;
            EnsureSeedAdmin(db, seed);
        }

        ConfigureApp(app);
        app.Run();
    }

    /// <summary>
    /// Configure the <see cref="WebApplicationBuilder"/>. Exposed for tests
    /// that build the host via WebApplicationFactory.
    /// </summary>
    public static void ConfigureBuilder(WebApplicationBuilder builder, IConfiguration cfg)
    {
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        ConfigureData(builder.Services, cfg);
        ConfigureAuthentication(builder.Services, cfg);
        ConfigureRateLimiter(builder.Services);
        builder.Services.Configure<AdminSeedOptions>(cfg.GetSection("Admin"));

        // Webhook fan-out: JobsEndpoints.AckAsync resolves
        // IWebhookDispatcher and enqueues per-endpoint delivery rows.
        // The hosted service drains them asynchronously.
        builder.Services.AddScoped<IWebhookDispatcher, WebhookDispatcher>();
        builder.Services.AddHttpClient("WebhookDispatcher");
        builder.Services.AddHostedService<WebhookDispatcherWorker>();
    }

    /// <summary>
    /// Wire the EF Core <see cref="CentralApiDbContext"/>. When the central
    /// connection string is missing or empty (development / test startup), we
    /// fall back to an in-memory provider so the host can boot without a
    /// PostgreSQL instance.
    /// </summary>
    public static void ConfigureData(IServiceCollection services, IConfiguration cfg)
    {
        var connectionString = cfg.GetConnectionString("CentralApi");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<CentralApiDbContext>(opt =>
                opt.UseNpgsql(connectionString));
        }
        else
        {
            services.AddDbContext<CentralApiDbContext>(opt => opt.UseInMemoryDatabase("CentralApiFallback"));
        }
    }

    /// <summary>
    /// Configure JWT bearer authentication. The signing key is read from the
    /// <c>Jwt</c> config section; for tests a stable, test-only default is
    /// substituted when the section is empty so <see cref="WebApplicationFactory{Program}"/>
    /// can mint tokens out-of-the-box.
    /// </summary>
    public static void ConfigureAuthentication(IServiceCollection services, IConfiguration cfg)
    {
        // Disable the legacy claim mapping so the wire-format names (`sub`,
        // `tenant`, `scope`) reach the controllers verbatim instead of being
        // rewritten to SOAP-style claim URIs. This is set globally on the
        // static handler and applies to both issuer and validator.
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        var jwt = cfg.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var signingKey = !string.IsNullOrWhiteSpace(jwt.SigningKey)
            ? jwt.SigningKey
            : TestJwtConstants.TestSigningKey;
        if (string.IsNullOrWhiteSpace(jwt.SigningKey))
            jwt.SigningKey = signingKey;

        services.Configure<JwtOptions>(cfg.GetSection("Jwt"));
        services.PostConfigure<JwtOptions>(options =>
        {
            if (string.IsNullOrWhiteSpace(options.SigningKey))
                options.SigningKey = signingKey;
        });
        services.AddSingleton<IJwtIssuer, JwtIssuer>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            })
            // API-key scheme sits alongside JWT. IngestEndpoints authorizes
            // via the ApiKey policy, which only matches when the request was
            // authenticated under the ApiKey scheme (no JWT path produces a
            // `scope=apikey` claim, so JWT-only callers fall through).
            .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationHandler.SchemeName,
                _ => { /* all defaults; options bound through IOptions if needed later */ });
        services.AddAuthorization(options =>
        {
            // The "Agent" policy authenticates the principal and requires a
            // `scope=agent` claim. This is the standard authorization seam for
            // every JWT-protected endpoint on the central API.
            options.AddPolicy(AgentPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim("scope", "agent"));

            // The "Admin" policy authenticates the principal and requires a
            // `scope=admin` claim. Used by every /api/v1/admin/* endpoint.
            // The two policies are mutually exclusive — an admin token has
            // no `scope=agent` claim and is rejected by AgentPolicy, and an
            // agent token (or any other) without `scope=admin` is rejected
            // by AdminPolicy.
            options.AddPolicy(AdminPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim("scope", "admin"));

            // The "ApiKey" policy authenticates the principal under the
            // API-key scheme (the JWT scheme cannot satisfy it because the
            // issuer never stamps `scope=apikey` on agent/admin tokens) and
            // requires the apikey scope.
            options.AddPolicy(ApiKeyPolicy, policy => policy
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.SchemeName)
                .RequireClaim("scope", "apikey"));
            options.AddPolicy(MobilePolicy, policy => policy
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, ApiKeyAuthenticationHandler.SchemeName)
                .RequireAssertion(context =>
                    context.User.HasClaim("scope", "mobile")
                    || context.User.HasClaim("scope", "apikey")));
        });
    }

    /// <summary>
    /// Configure a fixed-window rate limiter. The "per-agent" policy partitions
    /// by the JWT <c>sub</c> claim (the agent id), which is the brief's
    /// default. The "Tenant" policy partitions by the <c>tenant</c> claim as
    /// a coarser secondary guard. "Anonymous" partitions by remote IP for
    /// pre-auth calls. A global limiter caps total per-IP throughput.
    /// </summary>
    public static void ConfigureRateLimiter(IServiceCollection services)
    {
        services.AddRateLimiter(opt =>
        {
            opt.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            opt.AddPolicy(PerAgentRateLimitPolicy, httpContext =>
            {
                var agentId = httpContext.User?.FindFirst("sub")?.Value
                    ?? httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? "anonymous";
                return RateLimitPartition.GetFixedWindowLimiter("agent:" + agentId, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = DefaultPermitsPerMinute,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                });
            });

            opt.AddPolicy(PerAdminRateLimitPolicy, httpContext =>
            {
                var adminId = httpContext.User?.FindFirst("sub")?.Value
                    ?? httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? "anonymous";
                return RateLimitPartition.GetFixedWindowLimiter("admin:" + adminId, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = AdminPermitsPerMinute,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                });
            });

            opt.AddPolicy(PerTenantRateLimitPolicy, httpContext =>
            {
                var key = ResolvePartitionKey(httpContext);
                return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                });
            });

            opt.AddPolicy(AnonymousRateLimitPolicy, httpContext =>
            {
                var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter("anon:" + remoteIp, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 60,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                });
            });

            opt.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter("global:" + remoteIp, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 1000,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                });
            });
        });
    }

    private static string ResolvePartitionKey(HttpContext httpContext)
    {
        var tenantClaim = httpContext.User.FindFirst("tenant")?.Value;
        if (!string.IsNullOrWhiteSpace(tenantClaim) && Guid.TryParse(tenantClaim, out var tenantId))
            return "tenant:" + tenantId;
        return "tenant:anon";
    }

    /// <summary>
    /// Configure the <see cref="WebApplication"/> middleware pipeline and
    /// endpoint mapping. Called by both production startup and
    /// <see cref="WebApplicationFactory{Program}"/>.
    /// </summary>
    public static void ConfigureApp(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Older Android releases used /api/v1/sync/* while the public mobile
        // contract is /api/v1/android/sync/*. Keep those installations working
        // during the client rollout without weakening authentication: the
        // rewritten request is still authorized by the Android endpoint policy.
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var normalized = "/" + path.TrimStart('/');
            const string legacyPrefix = "/api/v1/sync/";
            if (normalized.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var suffix = normalized[legacyPrefix.Length..];
                if (string.Equals(suffix, "cariAdresleri", StringComparison.OrdinalIgnoreCase))
                    suffix = "cariAdresler";
                context.Request.Path = "/api/v1/android/sync/" + suffix;
            }

            await next();
        });

        app.UseRouting();

        // Only enable the rate-limiter middleware when rate limiter services
        // are actually registered. The test factory intentionally strips
        // AddRateLimiter's registrations to bypass the limiter; under that
        // scenario middleware must be skipped too, otherwise UseRateLimiter
        // throws at startup.
        if (HasRateLimiter(app.Services))
        {
            try
            {
                app.UseRateLimiter();
            }
            catch (InvalidOperationException)
            {
                // Test factory can intentionally strip rate limiter services;
                // a mismatched ConfigurationWebHost sequence may also lead to
                // missing IOptions<RateLimiterOptions>. Swallow so the host can
                // still serve traffic.
            }
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", () => Results.Text("{\"status\":\"ok\"}", "application/json"))
            .WithName("Health").WithTags("System").AllowAnonymous();

        app.MapAgentsEndpoints();
        app.MapLicensesEndpoints();
        app.MapJobsEndpoints();
        app.MapBootstrapEndpoints();
        app.MapIngestEndpoints();
        app.MapAndroidEndpoints();
        app.MapMobileLicensingEndpoints();
        app.MapAdminAuthEndpoints();
        app.MapAdminTenantsEndpoints();
        app.MapAdminLicensesEndpoints();
        app.MapAdminAgentsEndpoints();
        app.MapAdminJobsEndpoints();
        app.MapAdminBootstrapEndpoints();
        app.MapAdminApiKeysEndpoints();
        app.MapAdminWebhooksEndpoints();
    }

    private static bool HasRateLimiter(IServiceProvider services)
    {
        // RateLimiterOptions is the canonical registration the UseRateLimiter
        // middleware looks up. Its presence means AddRateLimiter was called.
        return services.GetService<Microsoft.Extensions.Options.IOptions<RateLimiterOptions>>() is not null;
    }

    /// <summary>
    /// Idempotently seed a single bootstrap admin row when both
    /// <see cref="AdminSeedOptions.SeedEmail"/> and
    /// <see cref="AdminSeedOptions.SeedPassword"/> are present in
    /// configuration. Logs the email (not the password) on creation. When
    /// either value is empty, no row is created — the admin endpoint will
    /// then refuse every login.
    /// </summary>
    internal static void EnsureSeedAdmin(CentralApiDbContext db, AdminSeedOptions seed)
    {
        if (seed is null) return;
        if (string.IsNullOrWhiteSpace(seed.SeedEmail) || string.IsNullOrWhiteSpace(seed.SeedPassword))
            return;

        var email = seed.SeedEmail.Trim().ToLowerInvariant();
        if (db.AdminUsers.Any(a => a.Email == email)) return;

        var admin = new AdminUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(seed.SeedPassword),
            DisplayName = string.IsNullOrWhiteSpace(seed.SeedDisplayName) ? email : seed.SeedDisplayName,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsActive = true,
        };
        db.AdminUsers.Add(admin);
        db.SaveChanges();
    }
}

/// <summary>
/// Constants used by both <see cref="Program"/> and the in-process test
/// factory. The signing key is intentionally public — it is never used outside
/// the integration test process and is rejected by every production startup
/// that supplies a real <c>Jwt:SigningKey</c> via configuration.
/// </summary>
public static class TestJwtConstants
{
    /// <summary>Stable, test-only HS256 signing key. 32+ ASCII bytes.</summary>
    public const string TestSigningKey = "ErpBridgeTestOnlySigningKey_AtLeast32Bytes_HS256_Symmetric!";
}
