using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ErpBridge.CentralApi.Options;

namespace ErpBridge.CentralApi.Tests.Support;

/// <summary>
/// In-process test host for the central API. Spins up the full ASP.NET Core
/// pipeline with EF Core replaced by a unique-named in-memory database per
/// instance (parallel-safe) and the rate limiter removed so endpoint tests
/// can hammer a tenant without bouncing off the limiter.
///
/// Test flow:
/// <list type="number">
///   <item><description>Construct via <c>using var factory = new CentralApiFactory();</c>.</description></item>
///   <item><description>Build an <see cref="HttpClient"/> via <see cref="CreateClient"/>.</description></item>
///   <item><description>Seed via <see cref="SeedTenantAsync"/> and mint a JWT via <see cref="IssueTestJwt"/>.</description></item>
/// </list>
/// </summary>
public class CentralApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "CentralApiTestDb_" + Guid.NewGuid().ToString("N");
    private readonly bool _keepDatabase;
    private readonly bool _disableRateLimiter;

    /// <summary>
    /// Default factory: fresh in-memory database, rate limiter stripped.
    /// Tests that need the limiter (RateLimitTests) construct a derived
    /// <c>RateLimitedFactory</c> that overrides the constructors.
    /// </summary>
    public CentralApiFactory() : this(keepDatabase: false, disableRateLimiter: true) { }

    /// <summary>
    /// Constructor used by tests that want to keep the database alive across
    /// factory disposals or that want the rate limiter active. xUnit only
    /// matches parameterless fixtures, so this overload is exercised only
    /// via direct instantiation (e.g. <c>new RateLimitedFactory()</c>) or
    /// via derived classes with their own parameterless constructor.
    /// </summary>
    /// <param name="keepDatabase">Skip the EnsureDeleted cleanup on disposal.</param>
    /// <param name="disableRateLimiter">Strip the rate limiter so unit tests can hammer endpoints without bouncing off the limiter.</param>
    protected CentralApiFactory(bool keepDatabase, bool disableRateLimiter)
    {
        _keepDatabase = keepDatabase;
        _disableRateLimiter = disableRateLimiter;
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        if (_disableRateLimiter)
            builder.UseSetting("RateLimiter:DisabledForTests", "true");
        builder.ConfigureServices(services =>
        {
            services.PostConfigure<ApiKeyVaultOptions>(options =>
                options.MasterKey = Convert.ToBase64String(new byte[32]
                {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
                    17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32,
                }));
            // Replace the production DbContext (Npgsql if a connection string
            // was supplied, in-memory fallback otherwise) with a fresh
            // in-memory store. The name is unique per factory so tests can
            // run in parallel without crossing.
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<CentralApiDbContext>));
            if (descriptor is not null) services.Remove(descriptor);
            services.RemoveAll<CentralApiDbContext>();
            services.AddDbContext<CentralApiDbContext>(opt =>
                opt.UseInMemoryDatabase(_databaseName));

            if (_disableRateLimiter)
            {
                // Endpoint tests verify semantic contract, not limiter metadata;
                // the limiter has its own dedicated test (RateLimitTests) that
                // constructs the factory with _disableRateLimiter=false.
                RemoveRateLimiterServices(services);
            }
        });
    }

    /// <summary>
    /// Strip every <c>Microsoft.AspNetCore.RateLimiting</c>-related service
    /// descriptor from the container. Without these registrations the
    /// <c>RequireRateLimiting</c> calls on individual endpoints become
    /// no-ops at runtime (the middleware has nothing to consult).
    /// </summary>
    private static void RemoveRateLimiterServices(IServiceCollection services)
    {
        var limiterDescriptors = services
            .Where(d => d.ServiceType.FullName?.StartsWith("Microsoft.AspNetCore.RateLimiting", StringComparison.Ordinal) == true
                        || d.ServiceType.FullName == "Microsoft.AspNetCore.RateLimiting.RateLimiterOptions")
            .ToList();
        foreach (var desc in limiterDescriptors) services.Remove(desc);
    }

    /// <summary>
    /// Seed a fresh <see cref="Tenant"/> + active <see cref="License"/> row
    /// pair. Returns both so callers can mint a JWT against them.
    /// </summary>
    public async Task<(Tenant Tenant, License License)> SeedTenantAsync(string licenseKey = "TEST-LICENSE-001", string tenantName = "Test Tenant")
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = tenantName };
        var license = new License
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            LicenseKey = licenseKey,
            IsActive = true,
            IssuedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddYears(1),
        };
        db.Tenants.Add(tenant);
        db.Licenses.Add(license);
        await db.SaveChangesAsync();
        return (tenant, license);
    }

    /// <summary>
    /// Create an <see cref="Agent"/> row linked to the supplied tenant. Used
    /// by tests that need an existing agentId (e.g. heartbeat / jobs).
    /// </summary>
    public async Task<Agent> SeedAgentAsync(Guid tenantId, string machineId = "MACHINE-001")
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            MachineId = machineId,
            LicenseKey = "TEST-LICENSE-001",
            RegisteredAtUtc = DateTimeOffset.UtcNow,
        };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();
        return agent;
    }

    /// <summary>
    /// Issue a JWT signed with the same key the host uses. The factory
    /// pulls the issuer out of DI to keep the key contract centralized.
    /// </summary>
    public string IssueTestJwt(Guid agentId, Guid tenantId)
    {
        using var scope = Services.CreateScope();
        var issuer = scope.ServiceProvider.GetRequiredService<Authentication.IJwtIssuer>();
        return issuer.Issue(agentId, tenantId).Token;
    }

    /// <summary>
    /// Resolve <see cref="CentralApiDbContext"/> from DI for tests that need
    /// to inspect or seed rows outside the production endpoints.
    /// </summary>
    public CentralApiDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
    }

    /// <summary>
    /// Seed an <see cref="AdminUser"/> row with a bcrypt-hashed password.
    /// Returns the row so tests can mint a JWT against it.
    /// </summary>
    public async Task<AdminUser> SeedAdminAsync(
        string email = "admin@test.local",
        string password = "TestAdminPassword!",
        string displayName = "Test Admin",
        bool isActive = true)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var admin = new AdminUser
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            DisplayName = displayName,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            IsActive = isActive,
        };
        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync();
        return admin;
    }

    /// <summary>
    /// Mint an admin JWT against an <see cref="AdminUser"/> row.
    /// </summary>
    public string IssueAdminJwt(Guid adminId)
    {
        using var scope = Services.CreateScope();
        var issuer = scope.ServiceProvider.GetRequiredService<Authentication.IJwtIssuer>();
        return issuer.IssueForAdmin(adminId).Token;
    }

    /// <summary>
    /// Seed a <see cref="Job"/> row for a tenant. Tests use this for
    /// admin-job list/detail/retry assertions.
    /// </summary>
    public async Task<Job> SeedJobAsync(
        Guid tenantId,
        string externalId,
        JobStatus status = JobStatus.Pending,
        string documentType = "sales_order",
        string payloadJson = "{\"lines\":[]}")
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var job = new Job
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExternalId = externalId,
            DocumentType = documentType,
            PayloadJson = payloadJson,
            Status = status,
            EnqueuedAtUtc = DateTimeOffset.UtcNow,
            RetryCount = 0,
        };
        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        return job;
    }

    /// <summary>
    /// Seed a <see cref="BootstrapPackage"/> row for a tenant. The payload is
    /// persisted verbatim; admin tests can read it back through the latest-summary
    /// endpoint.
    /// </summary>
    public async Task<BootstrapPackage> SeedBootstrapPackageAsync(
        Guid tenantId,
        string payloadJson,
        string sourceDatabase = "TEST-MIKRO",
        DateTimeOffset? pulledAtUtc = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var pkg = new BootstrapPackage
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PayloadJson = payloadJson,
            SourceDatabase = sourceDatabase,
            PulledAtUtc = pulledAtUtc ?? DateTimeOffset.UtcNow,
            ReceivedAtUtc = DateTimeOffset.UtcNow,
        };
        db.BootstrapPackages.Add(pkg);
        await db.SaveChangesAsync();
        return pkg;
    }

    /// <summary>
    /// Seed an <see cref="ApiKey"/> row with the supplied raw value. Returns
    /// the row plus the salt+hash pair so tests can verify the hash chain.
    /// </summary>
    public async Task<(ApiKey Key, string RawKey, byte[] Salt, byte[] Hash)> SeedApiKeyAsync(
        Guid tenantId,
        string rawKey,
        string name = "Test key",
        bool isActive = true,
        DateTimeOffset? expiresAtUtc = null,
        string[]? scopes = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var salt = new byte[16];
        System.Security.Cryptography.RandomNumberGenerator.Fill(salt);
        var hash = Authentication.ApiKeyAuthenticationHandler.ComputeHash(salt, rawKey);
        var prefix = rawKey.Substring(0, Math.Min(11, rawKey.Length));
        var key = new ApiKey
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            KeyPrefix = prefix,
            KeyHash = hash,
            KeySalt = salt,
            Scopes = scopes ?? new[] { "ingest:write" },
            IsActive = isActive,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = expiresAtUtc,
        };
        db.ApiKeys.Add(key);
        await db.SaveChangesAsync();
        return (key, rawKey, salt, hash);
    }

    /// <summary>
    /// Seed a <see cref="WebhookEndpoint"/> row with a known signing secret.
    /// </summary>
    public async Task<(WebhookEndpoint Endpoint, string Secret)> SeedWebhookAsync(
        Guid tenantId,
        string secret,
        string name = "Test webhook",
        string url = "https://example.test/hook",
        bool isActive = true,
        string[]? subscribedEvents = null)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var ep = new WebhookEndpoint
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Url = url,
            SigningSecret = secret,
            SigningSecretPrefix = secret.Substring(0, Math.Min(8, secret.Length)),
            SubscribedEvents = subscribedEvents ?? Array.Empty<string>(),
            IsActive = isActive,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };
        db.WebhookEndpoints.Add(ep);
        await db.SaveChangesAsync();
        return (ep, secret);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_keepDatabase)
        {
            // Try to delete the in-memory database to free memory; non-fatal if it fails.
            try
            {
                using var scope = Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
                db.Database.EnsureDeleted();
            }
            catch
            {
                // best-effort cleanup
            }
        }
        base.Dispose(disposing);
    }
}
