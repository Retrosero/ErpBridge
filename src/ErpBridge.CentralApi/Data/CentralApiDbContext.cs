using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Data;

/// <summary>
/// EF Core context for the central API PostgreSQL database. All tables live in
/// the default <c>public</c> schema. Schemas/indexes are configured via
/// <see cref="OnModelCreating"/> rather than data annotations to keep the
/// entity POCOs free of persistence concerns.
/// </summary>
public sealed class CentralApiDbContext : DbContext
{
    public CentralApiDbContext(DbContextOptions<CentralApiDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobAckRecord> JobAcks => Set<JobAckRecord>();
    public DbSet<BootstrapPackage> BootstrapPackages => Set<BootstrapPackage>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<WebhookEndpoint> WebhookEndpoints => Set<WebhookEndpoint>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();
    public DbSet<MobileDevice> MobileDevices => Set<MobileDevice>();
    public DbSet<DeviceActivationCode> DeviceActivationCodes => Set<DeviceActivationCode>();
    public DbSet<TelemetryIssue> TelemetryIssues => Set<TelemetryIssue>();
    public DbSet<TelemetryEvent> TelemetryEvents => Set<TelemetryEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(b =>
        {
            b.ToTable("tenants");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.StockDetailFieldsJson).HasColumnType("jsonb").HasDefaultValue("[]");
            b.Property(x => x.DeviceSeatLimit).HasDefaultValue(5);
            b.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<MobileDevice>(b =>
        {
            b.ToTable("mobile_devices"); b.HasKey(x => x.Id);
            b.Property(x => x.InstallationId).IsRequired().HasMaxLength(128);
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(128);
            b.Property(x => x.Platform).HasMaxLength(32); b.Property(x => x.AppVersion).HasMaxLength(64);
            b.HasIndex(x => new { x.TenantId, x.InstallationId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.IsActive });
            b.HasOne(x => x.Tenant).WithMany(x => x.MobileDevices).HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeviceActivationCode>(b =>
        {
            b.ToTable("device_activation_codes"); b.HasKey(x => x.Id);
            b.Property(x => x.CodeHash).IsRequired().HasMaxLength(64);
            b.HasIndex(x => x.CodeHash).IsUnique(); b.HasIndex(x => new { x.TenantId, x.ExpiresAtUtc });
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TelemetryIssue>(b =>
        {
            b.ToTable("telemetry_issues");
            b.HasKey(x => x.Id);
            b.Property(x => x.Fingerprint).IsRequired().HasMaxLength(64);
            b.Property(x => x.Kind).IsRequired().HasMaxLength(32);
            b.Property(x => x.Severity).IsRequired().HasMaxLength(16);
            b.Property(x => x.Title).IsRequired().HasMaxLength(240);
            b.Property(x => x.Status).HasConversion<int>();
            b.Property(x => x.LastAppVersion).HasMaxLength(64);
            b.HasIndex(x => new { x.TenantId, x.Fingerprint }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status, x.LastSeenAtUtc });
            b.HasIndex(x => new { x.TenantId, x.Severity, x.LastSeenAtUtc });
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TelemetryEvent>(b =>
        {
            b.ToTable("telemetry_events");
            b.HasKey(x => x.Id);
            b.Property(x => x.Kind).IsRequired().HasMaxLength(32);
            b.Property(x => x.Severity).IsRequired().HasMaxLength(16);
            b.Property(x => x.AppVersion).HasMaxLength(64);
            b.Property(x => x.AndroidVersion).HasMaxLength(64);
            b.Property(x => x.DeviceModel).HasMaxLength(128);
            b.Property(x => x.Screen).HasMaxLength(128);
            b.Property(x => x.Operation).HasMaxLength(128);
            b.Property(x => x.ExceptionType).HasMaxLength(256);
            b.Property(x => x.HttpMethod).HasMaxLength(16);
            b.Property(x => x.HttpRoute).HasMaxLength(512);
            b.Property(x => x.CorrelationId).HasMaxLength(128);
            b.Property(x => x.BreadcrumbsJson).HasColumnType("jsonb").HasDefaultValue("[]");
            b.HasIndex(x => x.EventId).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.OccurredAtUtc });
            b.HasIndex(x => new { x.MobileDeviceId, x.OccurredAtUtc });
            b.HasIndex(x => new { x.TelemetryIssueId, x.OccurredAtUtc });
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.MobileDevice).WithMany().HasForeignKey(x => x.MobileDeviceId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Issue).WithMany(x => x.Events).HasForeignKey(x => x.TelemetryIssueId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<License>(b =>
        {
            b.ToTable("licenses");
            b.HasKey(x => x.Id);
            b.Property(x => x.LicenseKey).IsRequired().HasMaxLength(255);
            b.HasIndex(x => x.LicenseKey).IsUnique();
            b.HasOne(x => x.Tenant)
                .WithMany(t => t.Licenses)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Agent>(b =>
        {
            b.ToTable("agents");
            b.HasKey(x => x.Id);
            b.Property(x => x.MachineId).IsRequired().HasMaxLength(255);
            b.HasIndex(x => new { x.TenantId, x.MachineId }).IsUnique();
            b.HasOne(x => x.Tenant)
                .WithMany(t => t.Agents)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Job>(b =>
        {
            b.ToTable("jobs");
            b.HasKey(x => x.Id);
            b.Property(x => x.ExternalId).IsRequired().HasMaxLength(128);
            b.Property(x => x.DocumentType).IsRequired().HasMaxLength(64);
            b.Property(x => x.PayloadJson).HasColumnType("jsonb");
            b.Property(x => x.Status).HasConversion<int>();
            b.HasIndex(x => new { x.TenantId, x.Status, x.EnqueuedAtUtc });
            b.HasIndex(x => new { x.TenantId, x.DocumentType, x.ExternalId }).IsUnique();
        });

        modelBuilder.Entity<JobAckRecord>(b =>
        {
            b.ToTable("job_acks");
            b.HasKey(x => x.Id);
            b.Property(x => x.Status).IsRequired().HasMaxLength(32);
            b.Property(x => x.ErrorCode).HasMaxLength(64);
            b.Property(x => x.ErpDocumentSeries).HasMaxLength(16);
            b.HasOne(x => x.Job)
                .WithMany()
                .HasForeignKey(x => x.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BootstrapPackage>(b =>
        {
            b.ToTable("bootstrap_packages");
            b.HasKey(x => x.Id);
            b.Property(x => x.PayloadJson).HasColumnType("jsonb");
            b.Property(x => x.SourceDatabase).IsRequired().HasMaxLength(128);
            b.HasIndex(x => new { x.TenantId, x.PulledAtUtc });
        });

        modelBuilder.Entity<AdminUser>(b =>
        {
            b.ToTable("admin_users");
            b.HasKey(x => x.Id);
            b.Property(x => x.Email).IsRequired().HasMaxLength(255);
            b.Property(x => x.PasswordHash).IsRequired();
            b.Property(x => x.DisplayName).IsRequired().HasMaxLength(255);
            b.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<ApiKey>(b =>
        {
            b.ToTable("api_keys");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.KeyPrefix).IsRequired().HasMaxLength(32);
            b.Property(x => x.KeyHash).IsRequired().HasMaxLength(64);
            b.Property(x => x.KeySalt).IsRequired().HasMaxLength(64);
            b.Property(x => x.Scopes).HasColumnType("text[]");
            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.KeyPrefix });
        });

        modelBuilder.Entity<WebhookEndpoint>(b =>
        {
            b.ToTable("webhook_endpoints");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.Url).IsRequired().HasMaxLength(2048);
            b.Property(x => x.SigningSecret).IsRequired().HasMaxLength(128);
            b.Property(x => x.SigningSecretPrefix).IsRequired().HasMaxLength(16);
            b.Property(x => x.SubscribedEvents).HasColumnType("text[]");
            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<WebhookDelivery>(b =>
        {
            b.ToTable("webhook_deliveries");
            b.HasKey(x => x.Id);
            b.Property(x => x.EventType).IsRequired().HasMaxLength(64);
            b.Property(x => x.PayloadJson).HasColumnType("jsonb");
            b.Property(x => x.Status).HasConversion<int>();
            b.HasOne(x => x.Endpoint)
                .WithMany(e => e.Deliveries)
                .HasForeignKey(x => x.EndpointId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.Status, x.NextRetryAtUtc });
            b.HasIndex(x => new { x.EndpointId, x.CreatedAtUtc });
        });
    }
}
