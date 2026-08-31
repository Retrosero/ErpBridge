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
    public DbSet<MobileTelemetryEvent> MobileTelemetryEvents => Set<MobileTelemetryEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(b =>
        {
            b.ToTable("tenants");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.MaxDeviceCount).HasDefaultValue(1);
            b.HasIndex(x => x.Name).IsUnique();
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

        modelBuilder.Entity<MobileTelemetryEvent>(b =>
        {
            b.ToTable("mobile_telemetry_events");
            b.HasKey(x => x.Id);
            b.Property(x => x.EventId).IsRequired().HasMaxLength(64);
            b.Property(x => x.Kind).IsRequired().HasMaxLength(32);
            b.Property(x => x.Severity).IsRequired().HasMaxLength(16);
            b.Property(x => x.AppVersion).IsRequired().HasMaxLength(64);
            b.Property(x => x.AndroidVersion).IsRequired().HasMaxLength(32);
            b.Property(x => x.DeviceModel).IsRequired().HasMaxLength(128);
            b.Property(x => x.Screen).IsRequired().HasMaxLength(120);
            b.Property(x => x.Operation).IsRequired().HasMaxLength(120);
            b.Property(x => x.ExceptionType).IsRequired().HasMaxLength(160);
            b.Property(x => x.Message).IsRequired().HasMaxLength(1000);
            b.Property(x => x.StackTrace).IsRequired().HasMaxLength(4000);
            b.Property(x => x.HttpMethod).HasMaxLength(16);
            b.Property(x => x.HttpRoute).HasMaxLength(300);
            b.Property(x => x.CorrelationId).HasMaxLength(128);
            b.Property(x => x.BreadcrumbsJson).HasColumnType("jsonb");
            b.HasIndex(x => new { x.TenantId, x.EventId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.OccurredAtUtc });
            b.HasIndex(x => new { x.Severity, x.ReceivedAtUtc });
        });
    }
}
