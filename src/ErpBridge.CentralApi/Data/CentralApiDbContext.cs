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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(b =>
        {
            b.ToTable("tenants");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
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
    }
}