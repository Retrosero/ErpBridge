using ErpBridge.CentralApi.LogCenter;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.RateLimiting;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Health;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Options;
using ErpBridge.CentralApi.Parameters;
using ErpBridge.CentralApi.Security;
using ErpBridge.CentralApi.Telemetry;
using ErpBridge.CentralApi.Webhooks;
using ErpBridge.CentralApi.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

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

    /// <summary>
    /// Combines the JWT (Agent) and ApiKey authentication schemes. Used
    /// for ingest endpoints that must accept BOTH the legacy Windows
    /// Agent's JWT and the new SaaS API key. Either scope claim
    /// (<c>agent</c> or <c>apikey</c>) is sufficient.
    /// </summary>
    public const string AgentOrApiKeyPolicy = "AgentOrApiKey";

    /// <summary>Signed-in mobile app user (<c>scope=mobile-user</c> JWT).</summary>
    public const string MobileUserPolicy = "MobileUser";

    /// <summary>
    /// Mobile read/telemetry surface: a tenant API key (<c>scope=apikey</c>, the
    /// original ERP activation) or a signed-in mobile user whose user, device,
    /// tenant and subscription are still valid.
    /// </summary>
    public const string MobileClientPolicy = "MobileClient";

    /// <summary>A paired warehouse TV (<c>scope=display</c>, Faz 49); revocation is checked in the endpoints.</summary>
    public const string DisplayPolicy = "Display";

    /// <summary>Per paired TV, so a wall of boards never uses up the company's shared portal/phone limit.</summary>
    public const string PerDisplayRateLimitPolicy = "per-display";

    private const string ProductionCorsPolicy = "production-origins";

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

    /// <summary>Rate-limit policy for the chunked bootstrap upload routes.</summary>
    public const string BootstrapUploadRateLimitPolicy = "bootstrap-upload";

    /// <summary>
    /// Permits per minute for the chunked bootstrap upload. A full snapshot of a
    /// real Mikro database is roughly 210 chunk POSTs (~105k rows at 500 rows a
    /// chunk), which cannot fit in <see cref="DefaultPermitsPerMinute"/> — the
    /// operator's "rebuild snapshot" action failed with HTTP 429 every time. The
    /// periodic cycle adds to that: at a 20 s cadence it spends ~60 requests a
    /// minute on its own. Bulk transfer therefore gets its own, wider budget
    /// while every other agent route keeps the tighter default.
    /// </summary>
    public const int BootstrapUploadPermitsPerMinute = 600;

    /// <summary>
    /// Host entry point. Builds the WebApplication and runs the host. Database
    /// migrations are an explicit release operation invoked with <c>--migrate</c>.
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

        if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            if (!db.Database.IsRelational())
                throw new InvalidOperationException("--migrate requires the configured relational CentralApi database.");
            db.Database.Migrate();
            return;
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

        // Bring the parameter catalogue in line with the one embedded in this build. Idempotent,
        // and skipped by the test host: seeding ~4,700 rows into a throwaway in-memory database
        // for every test class costs far more than it proves.
        if (app.Configuration.GetValue("Parameters:SeedCatalogOnStartup", defaultValue: true))
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("ErpBridge.CentralApi.Parameters");

            try
            {
                var result = ParameterCatalogSeeder.Seed(db, ParameterCatalogFile.Load());
                if (result.Changed)
                {
                    logger.LogInformation(
                        "Parameter catalogue seeded: {Added} added, {Updated} updated, {Deprecated} deprecated, {Revived} revived.",
                        result.Added, result.Updated, result.Deprecated, result.Revived);
                }
            }
            catch (Exception ex)
            {
                // A stale catalogue is bad; a host that will not start is worse. The panel reports
                // the mismatch, and /health/schema already warns when the schema is behind.
                logger.LogError(ex, "Parameter catalogue could not be seeded; the panel may show stale metadata.");
            }
        }

        WarnIfSchemaIsBehind(app);
        ConfigureApp(app);
        app.Run();
    }

    /// <summary>
    /// A failed <c>--migrate</c> does not stop the container (see Dockerfile ENTRYPOINT); the app then
    /// runs on an old schema. Say so loudly at startup instead of only through failing requests.
    /// </summary>
    private static void WarnIfSchemaIsBehind(WebApplication app)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var schema = SchemaStatus.CheckAsync(db, CancellationToken.None).GetAwaiter().GetResult();
            if (schema.Status == SchemaStatus.Pending)
            {
                app.Logger.LogCritical(
                    "DATABASE SCHEMA IS BEHIND: {Pending} migration(s) pending, {Applied} applied. The --migrate step failed; see its output above. /health/schema returns 503 until fixed.",
                    schema.Pending, schema.Applied);
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Could not check the database schema version at startup.");
        }
    }

    /// <summary>
    /// Configure the <see cref="WebApplicationBuilder"/>. Exposed for tests
    /// that build the host via WebApplicationFactory.
    /// </summary>
    public static void ConfigureBuilder(WebApplicationBuilder builder, IConfiguration cfg)
    {
        // Log Merkezi L2a/L2b: one line per entry with scopes (correlation id, company) on the console Coolify
        // shows, and warning+ lines stored in log_events.
        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.IncludeScopes = true;
            options.UseUtcTimestamp = true;
            options.TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ ";
        });
        DatabaseLogProvider.Register(builder, cfg);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddMemoryCache();

        var allowTestDefaults = IsTestEnvironment(builder.Environment.EnvironmentName);
        ValidateRuntimeConfiguration(cfg, allowTestDefaults);
        ConfigureCors(builder.Services, cfg, allowTestDefaults);
        ConfigureData(builder.Services, cfg, allowTestDefaults);
        ConfigureAuthentication(builder.Services, cfg, allowTestDefaults);
        ConfigureRateLimiter(builder.Services);
        builder.Services.Configure<AdminSeedOptions>(cfg.GetSection("Admin"));

        // Parametre Yönetimi (P1c): effective-value resolution and Fora's write semantics.
        builder.Services.AddScoped<ParameterResolver>();
        builder.Services.Configure<ApiKeyVaultOptions>(cfg.GetSection("ApiKeyVault"));
        builder.Services.Configure<AuditRetentionOptions>(cfg.GetSection(AuditRetentionOptions.SectionName));
        builder.Services.AddSingleton<IApiKeyVault, ApiKeyVault>();
        builder.Services.AddSingleton<ApiKeyUsageTracker>();
        builder.Services.AddHostedService<ApiKeyUsageFlushWorker>();

        // Webhook fan-out: JobsEndpoints.AckAsync resolves
        // IWebhookDispatcher and enqueues per-endpoint delivery rows.
        // The hosted service drains them asynchronously.
        builder.Services.AddScoped<IWebhookDispatcher, WebhookDispatcher>();
        builder.Services.AddHttpClient("WebhookDispatcher")
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                UseProxy = false,
                ConnectCallback = WebhookTargetValidator.ConnectPublicAsync,
            });
        builder.Services.AddHostedService<WebhookDispatcherWorker>();
        builder.Services.AddHostedService<MobileTelemetryRetentionWorker>();
        // Faz 15.6 — audit log retention worker. Disabled in tests via
        // "AuditRetention:Enabled": false in appsettings.Test.json.
        builder.Services.AddHostedService<AuditRetentionWorker>();

        // Phase 9: in-memory pub/sub for "new bootstrap package available"
        // signals consumed by the WPF desktop UI's long-poll loop. Single
        // replica only — multi-instance scale would need a Redis backplane.
        builder.Services.AddSingleton<IBootstrapNotificationHub, BootstrapNotificationHub>();

        // Faz 26: the single writer of mobile_records. Stateless apart from its
        // logger, but scoped so it reads naturally alongside the DbContext it is
        // always handed.
        builder.Services.AddScoped<ErpBridge.CentralApi.Sync.MobileRecordProjector>();
        builder.Services.AddScoped<ErpBridge.CentralApi.Sync.MobileRecordBackfill>();
        builder.Services.AddScoped<ErpBridge.CentralApi.Sync.MobileRecordRetention>();
        builder.Services.AddScoped<ErpBridge.CentralApi.Mobile.MobileSeatService>();
        // Log Merkezi: the single writer for log_events / log_error_groups.
        builder.Services.AddScoped<ErpBridge.CentralApi.LogCenter.ILogEventWriter, ErpBridge.CentralApi.LogCenter.LogEventWriter>();
        builder.Services.AddHostedService<ErpBridge.CentralApi.LogCenter.LogGroupBackfillWorker>();
        builder.Services.Configure<ErpBridge.CentralApi.LogCenter.LogRetentionOptions>(cfg.GetSection(ErpBridge.CentralApi.LogCenter.LogRetentionOptions.SectionName));
        builder.Services.AddScoped<ErpBridge.CentralApi.LogCenter.LogRetention>();
        builder.Services.AddHostedService<ErpBridge.CentralApi.LogCenter.LogRetentionWorker>();
        builder.Services.AddScoped<ErpBridge.CentralApi.Native.NativeDocumentProcessor>();
        builder.Services.AddScoped<ErpBridge.CentralApi.Team.TeamDocumentProcessor>();
        builder.Services.AddScoped<ErpBridge.CentralApi.Approvals.ApprovalService>();
        // Faz 47: warehouse queue. The event hub is in memory: one CentralApi container (plan step 4).
        builder.Services.AddSingleton<ErpBridge.CentralApi.Notifications.ITenantEventHub, ErpBridge.CentralApi.Notifications.TenantEventHub>();
        builder.Services.AddScoped<ErpBridge.CentralApi.Warehouse.FulfillmentService>();
    }

    /// <summary>
    /// Wire the EF Core <see cref="CentralApiDbContext"/>. In-memory storage is
    /// available only to the explicit in-process test host.
    /// </summary>
    public static void ConfigureData(IServiceCollection services, IConfiguration cfg, bool allowTestDefaults)
    {
        var connectionString = cfg.GetConnectionString("CentralApi");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<CentralApiDbContext>(opt =>
                opt.UseNpgsql(ApplyPoolDefaults(connectionString)));
        }
        else if (allowTestDefaults)
        {
            services.AddDbContext<CentralApiDbContext>(opt => opt.UseInMemoryDatabase("CentralApiFallback"));
        }
        else
        {
            throw new InvalidOperationException("ConnectionStrings:CentralApi is required outside the test environment.");
        }
    }

    /// <summary>
    /// Caps the Npgsql pool below Postgres's own default <c>max_connections</c> (100)
    /// when the operator hasn't already set one explicitly. CentralApi is currently
    /// a single instance, so Npgsql's own default (100) could alone consume the
    /// entire server-side connection budget, leaving no headroom for migrations,
    /// health checks, or manual admin connections on the same small VPS.
    /// </summary>
    private static string ApplyPoolDefaults(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        if (!builder.ContainsKey("Maximum Pool Size"))
        {
            builder.MaxPoolSize = 60;
        }

        return builder.ConnectionString;
    }

    /// <summary>
    /// Configure JWT bearer authentication. The signing key is read from the
    /// <c>Jwt</c> config section. The stable test-only key is available only to
    /// the in-process test host.
    /// </summary>
    public static void ConfigureAuthentication(IServiceCollection services, IConfiguration cfg, bool allowTestDefaults)
    {
        // Disable the legacy claim mapping so the wire-format names (`sub`,
        // `tenant`, `scope`) reach the controllers verbatim instead of being
        // rewritten to SOAP-style claim URIs. This is set globally on the
        // static handler and applies to both issuer and validator.
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

        var jwt = cfg.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var signingKey = !string.IsNullOrWhiteSpace(jwt.SigningKey)
            ? jwt.SigningKey
            : allowTestDefaults
                ? TestJwtConstants.TestSigningKey
                : throw new InvalidOperationException("Jwt:SigningKey is required outside the test environment.");
        if (string.IsNullOrWhiteSpace(jwt.SigningKey))
            jwt.SigningKey = signingKey;

        services.Configure<JwtOptions>(cfg.GetSection("Jwt"));
        services.PostConfigure<JwtOptions>(options =>
        {
            if (string.IsNullOrWhiteSpace(options.SigningKey))
                options.SigningKey = signingKey;
        });
        services.AddSingleton<IJwtIssuer, JwtIssuer>();
        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, MobileUserStateHandler>();
        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationMiddlewareResultHandler, MobileUserAuthorizationResultHandler>();

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

            // The "AgentOrApiKey" policy accepts EITHER the legacy Agent JWT
            // (scope=agent) or the new SaaS API key (scope=apikey). Used by
            // every agent-facing ingest endpoint so the existing Windows
            // Agent can still push while new tenants register an API key
            // instead. This is the OR-semantic equivalent of the previous
            // single-policy requirement — the previous setup only allowed
            // ApiKey, which broke the bootstrap change-set push on existing
            // installations (regression introduced in Wave 8).
            // The "MobileUser" policy accepts only a token minted by the mobile
            // sign-in endpoint. It proves the signature and scope; whether the
            // user, device, tenant and subscription are still valid is checked
            // against the database on every call (MobileUserAccess).
            options.AddPolicy(DisplayPolicy, policy => policy
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireClaim("scope", CentralApiClaims.DisplayScope));

            options.AddPolicy(MobileUserPolicy, policy => policy
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireClaim("scope", CentralApiClaims.MobileUserScope)
                .AddRequirements(new MobileUserStateRequirement()));

            options.AddPolicy(MobileClientPolicy, policy => policy
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme,
                    ApiKeyAuthenticationHandler.SchemeName)
                .RequireAssertion(ctx =>
                    ctx.User.HasClaim("scope", "apikey") ||
                    ctx.User.HasClaim("scope", CentralApiClaims.MobileUserScope))
                .AddRequirements(new MobileUserStateRequirement { PhoneClientOnly = true }));

            options.AddPolicy(AgentOrApiKeyPolicy, policy => policy
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(
                    JwtBearerDefaults.AuthenticationScheme,
                    ApiKeyAuthenticationHandler.SchemeName)
                .RequireAssertion(ctx =>
                    ctx.User.HasClaim("scope", "agent") ||
                    ctx.User.HasClaim("scope", "apikey") ||
                    ctx.User.HasClaim("scope", CentralApiClaims.MobileUserScope))
                // A signed-in mobile user sends sales and collections through the
                // same ingest endpoints the API-key app used; they must still be valid.
                .AddRequirements(new MobileUserStateRequirement()));
        });
    }

    /// <summary>
    /// Reject configuration that would otherwise make a non-test host boot with
    /// test credentials or transient storage. Kept public for startup tests.
    /// </summary>
    public static void ValidateRuntimeConfiguration(IConfiguration cfg, bool allowTestDefaults)
    {
        if (allowTestDefaults) return;

        if (string.IsNullOrWhiteSpace(cfg.GetConnectionString("CentralApi")))
            throw new InvalidOperationException("ConnectionStrings:CentralApi is required outside the test environment.");

        var signingKey = cfg.GetSection("Jwt").Get<JwtOptions>()?.SigningKey;
        if (string.IsNullOrWhiteSpace(signingKey) || string.Equals(signingKey, TestJwtConstants.TestSigningKey, StringComparison.Ordinal))
            throw new InvalidOperationException("A non-test host requires a non-test Jwt:SigningKey.");

        var vaultKey = cfg.GetSection("ApiKeyVault").Get<ApiKeyVaultOptions>()?.MasterKey;
        if (string.IsNullOrWhiteSpace(vaultKey))
            throw new InvalidOperationException("ApiKeyVault:MasterKey is required outside the test environment.");

        try
        {
            if (Convert.FromBase64String(vaultKey).Length != 32)
                throw new InvalidOperationException("ApiKeyVault:MasterKey must decode to exactly 32 bytes.");
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("ApiKeyVault:MasterKey must be valid base64.", ex);
        }

        var allowedOrigins = cfg.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        if (allowedOrigins.Length == 0 || allowedOrigins.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("Cors:AllowedOrigins requires at least one explicit origin outside the test environment.");
    }

    private static void ConfigureCors(IServiceCollection services, IConfiguration cfg, bool allowTestDefaults)
    {
        var allowedOrigins = cfg.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        services.AddCors(options => options.AddPolicy(ProductionCorsPolicy, policy =>
        {
            if (allowTestDefaults)
            {
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                return;
            }

            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }));
    }

    private static bool IsTestEnvironment(string environmentName) =>
        string.Equals(environmentName, "Test", StringComparison.OrdinalIgnoreCase)
        || string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase);

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

            opt.AddPolicy(BootstrapUploadRateLimitPolicy, httpContext =>
            {
                var agentId = httpContext.User?.FindFirst("sub")?.Value
                    ?? httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? "anonymous";
                return RateLimitPartition.GetFixedWindowLimiter("bootstrap:" + agentId, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = BootstrapUploadPermitsPerMinute,
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

            opt.AddPolicy(PerDisplayRateLimitPolicy, httpContext =>
            {
                var displayId = httpContext.User.FindFirst("sub")?.Value ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter("display:" + displayId, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 60,
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
        // Log Merkezi L0e: every request gets a correlation id first, so the exception handler, the logs and
        // the response all carry the same one. The handler answers an unhandled exception with a bare 500
        // ApiError and records the details in the log centre.
        app.UseCorrelationId();
        app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = UnhandledExceptionHandler.HandleAsync });

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();
        app.UseCors(ProductionCorsPolicy);

        // Endpoint rate-limit policies partition agent, admin and tenant
        // traffic by authenticated claims. Authentication must therefore run
        // before the limiter; otherwise every protected request is charged to
        // the anonymous bucket and a single caller can throttle all tenants.
        app.UseAuthentication();

        // Log Merkezi L2c: caller context for every log line of the request, 5xx and slow requests recorded.
        app.UseRequestOutcomeLogging();

        // The in-process test factory can explicitly disable this middleware
        // for endpoint tests that are unrelated to throttling. Production
        // never probes-and-swallows a missing limiter registration: a broken
        // configuration must fail host startup rather than serve unprotected.
        if (!app.Configuration.GetValue<bool>("RateLimiter:DisabledForTests"))
        {
            app.UseRateLimiter();
        }

        app.UseAuthorization();

        // Liveness deliberately has no dependency on PostgreSQL: an orchestrator
        // can restart only a wedged process without hiding a database outage.
        app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }))
            .WithName("HealthLive").WithTags("System").AllowAnonymous();
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
            .WithName("Health").WithTags("System").AllowAnonymous();
        app.MapGet("/health/ready", async (CentralApiDbContext db, CancellationToken ct) =>
        {
            try
            {
                return await db.Database.CanConnectAsync(ct)
                    ? Results.Ok(new { status = "ready" })
                    : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
            catch (Exception)
            {
                // Do not disclose connection strings, hostnames, or provider diagnostics.
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }).WithName("HealthReady").WithTags("System").AllowAnonymous();

        // Schema drift: 503 while migrations are pending. Not the container healthcheck on purpose —
        // an orchestrator restarting the app would not apply a migration that already failed.
        app.MapGet("/health/schema", async (CentralApiDbContext db, CancellationToken ct) =>
        {
            try
            {
                var schema = await SchemaStatus.CheckAsync(db, ct);
                var body = new { status = schema.Status, applied = schema.Applied, pending = schema.Pending };
                return schema.Status == SchemaStatus.Pending
                    ? Results.Json(body, statusCode: StatusCodes.Status503ServiceUnavailable)
                    : Results.Ok(body);
            }
            catch (Exception)
            {
                // Same rule as readiness: no provider diagnostics to anonymous callers.
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }).WithName("HealthSchema").WithTags("System").AllowAnonymous();

        app.MapAgentsEndpoints();
        app.MapLicensesEndpoints();
        app.MapJobsEndpoints();
        app.MapBootstrapEndpoints();
        app.MapBootstrapUploadEndpoints();
        app.MapBootstrapNotifyEndpoints();
        app.MapIngestEndpoints();
        app.MapDocumentStatusEndpoints();
        app.MapChangeSetEndpoints();
        app.MapChangeSetAndroidEndpoints();
        app.MapMobileSyncPullEndpoints();
        app.MapAndroidNotifyEndpoints();
        app.MapAndroidEndpoints();
        app.MapMobileTelemetryEndpoints();
        app.MapMobileAccountEndpoints();
        app.MapMobileApprovalEndpoints();
        app.MapPortalEndpoints();
        app.MapPortalErpWriteEndpoints();
        app.MapPortalErpDocumentsEndpoints();
        app.MapPortalNativeCardsEndpoints();
        app.MapPortalNativeCustomerCardsEndpoints();
        app.MapPortalNativePaymentsEndpoints();
        app.MapPortalNativeAuditEndpoints();
        app.MapPortalNativeLedgerEndpoints();
        app.MapPortalNativeSalesEndpoints();
        app.MapPortalNativeDocumentsEndpoints();
        app.MapPortalNativeStockCountsEndpoints();
        app.MapWarehouseEndpoints();
        app.MapDisplayEndpoints();
        app.MapAdminParameterEndpoints();
        app.MapAgentParameterEndpoints();
        app.MapParameterEndpoints();
        app.MapParameterReadEndpoints();
        app.MapAdminAuditEndpoints();
        app.MapAdminSyncQueueEndpoints();
        app.MapAdminAuthEndpoints();
        app.MapAdminTenantsEndpoints();
        app.MapAdminErpCompaniesEndpoints();
        app.MapAdminLicensesEndpoints();
        app.MapAdminAgentsEndpoints();
        app.MapAdminJobsEndpoints();
        app.MapAdminBootstrapEndpoints();
        app.MapAdminMobileRecordsEndpoints();
        app.MapAdminApiKeysEndpoints();
        app.MapAdminWebhooksEndpoints();
        app.MapAdminTelemetryEndpoints();
        app.MapAdminLogEndpoints();
        app.MapInternalLogEndpoints();
        app.MapAdminMobileSeatsEndpoints();
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
