using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.LogCenter;
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

    /// <summary>Log Merkezi L3f — a thinned history of agent heartbeats (change or 15 minutes, whichever first).</summary>
    public DbSet<AgentHeartbeatLogEntry> AgentHeartbeatLog => Set<AgentHeartbeatLogEntry>();
    public DbSet<ErpCompany> ErpCompanies => Set<ErpCompany>();
    public DbSet<AgentCompanyAssignment> AgentCompanyAssignments => Set<AgentCompanyAssignment>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobAckRecord> JobAcks => Set<JobAckRecord>();
    public DbSet<BootstrapPackage> BootstrapPackages => Set<BootstrapPackage>();
    public DbSet<BootstrapSnapshot> BootstrapSnapshots => Set<BootstrapSnapshot>();
    public DbSet<BootstrapSnapshotChunk> BootstrapSnapshotChunks => Set<BootstrapSnapshotChunk>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<ApiKeySecretAccessAudit> ApiKeySecretAccessAudits => Set<ApiKeySecretAccessAudit>();
    public DbSet<WebhookEndpoint> WebhookEndpoints => Set<WebhookEndpoint>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();
    public DbSet<MobileTelemetryEvent> MobileTelemetryEvents => Set<MobileTelemetryEvent>();
    public DbSet<LogEvent> LogEvents => Set<LogEvent>();
    public DbSet<LogErrorGroup> LogErrorGroups => Set<LogErrorGroup>();
    public DbSet<LogSettings> LogSettings => Set<LogSettings>();
    public DbSet<ErpWriteSettings> ErpWriteSettings => Set<ErpWriteSettings>();
    public DbSet<MobileUserErpMapping> MobileUserErpMappings => Set<MobileUserErpMapping>();
    public DbSet<ChangeSetRecord> ChangeSets => Set<ChangeSetRecord>();
    public DbSet<MobileSyncQueueItem> MobileSyncQueue => Set<MobileSyncQueueItem>();

    /// <summary>
    /// Faz 26 — current state of everything a mobile device may hold, one row per
    /// record, ordered by when it last changed. Devices page it with a single
    /// cursor, so a fresh install and a routine delta run the same query.
    /// </summary>
    public DbSet<MobileRecord> MobileRecords => Set<MobileRecord>();

    /// <summary>Faz 26 — per-tenant allocator for <see cref="MobileRecord.UpdatedSeq"/>.</summary>
    public DbSet<TenantSyncCounter> TenantSyncCounters => Set<TenantSyncCounter>();

    /// <summary>Faz 15.6 — append-only audit log of every change-set bundle the central API accepts.</summary>
    public DbSet<ChangeSetAuditEntry> ChangeSetAuditEntries => Set<ChangeSetAuditEntry>();

    /// <summary>
    /// Parametre Yönetimi (P1a) — every parameter Fora declares, with its default and the metadata
    /// the panel renders it from. Seeded from the catalogue embedded in this build.
    /// </summary>
    public DbSet<ParameterCatalogEntry> ParameterCatalog => Set<ParameterCatalogEntry>();

    /// <summary>
    /// Parametre Yönetimi (P1b) — parameters a customer has moved away from their catalogue
    /// default. Only deviations are stored, exactly as Fora does it.
    /// </summary>
    public DbSet<ParameterValue> ParameterValues => Set<ParameterValue>();

    /// <summary>Parametre Yönetimi (P1d) — per-scope change counter, so clients can poll cheaply.</summary>
    public DbSet<ParameterRevision> ParameterRevisions => Set<ParameterRevision>();

    /// <summary>Parametre Yönetimi (P1d) — append-only record of who changed which parameter.</summary>
    public DbSet<ParameterAuditEntry> ParameterAudit => Set<ParameterAuditEntry>();

    /// <summary>What each agent run did to one company's Mikro parameter table (D8).</summary>
    public DbSet<ParameterMirrorReport> ParameterMirrorReports => Set<ParameterMirrorReport>();

    /// <summary>Rows the agent found in Mikro holding something the centre did not set.</summary>
    public DbSet<ParameterMirrorDrift> ParameterMirrorDrifts => Set<ParameterMirrorDrift>();

    /// <summary>Read-only scans of a customer's existing Fora settings, uploaded as proposals.</summary>
    public DbSet<ForaImportBatch> ForaImportBatches => Set<ForaImportBatch>();

    /// <summary>What one scan found, matched or not.</summary>
    public DbSet<ForaImportRow> ForaImportRows => Set<ForaImportRow>();

    /// <summary>Faz 15.5 — Mikro <c>_ERPB_PARAMETRELER</c> snapshot mirror, one row per parameter.</summary>
    public DbSet<ParameterRecord> Parameters => Set<ParameterRecord>();

    /// <summary>Mobile app users; one active user is one paid seat.</summary>
    public DbSet<MobileUser> MobileUsers => Set<MobileUser>();
    public DbSet<MobileUserRole> UserRoles => Set<MobileUserRole>();

    /// <summary>Phones that signed in to a tenant, for support and revocation.</summary>
    public DbSet<MobileDevice> MobileDevices => Set<MobileDevice>();

    /// <summary>Seat purchases; append-only history with one current row per tenant.</summary>
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();

    /// <summary>Stock on hand for tenants without an ERP (the central API is the book of record).</summary>
    public DbSet<NativeStockLevel> NativeStockLevels => Set<NativeStockLevel>();

    /// <summary>Customer balances for tenants without an ERP.</summary>
    public DbSet<NativeCustomerBalance> NativeCustomerBalances => Set<NativeCustomerBalance>();

    /// <summary>Company approval queue shared by all approvers.</summary>
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();

    /// <summary>History of every approval request.</summary>
    public DbSet<ApprovalRequestEvent> ApprovalRequestEvents => Set<ApprovalRequestEvent>();

    /// <summary>Which operations of a company need approval.</summary>
    public DbSet<TenantApprovalRules> TenantApprovalRules => Set<TenantApprovalRules>();

    /// <summary>Faz 47 — sales orders the warehouse prepares (plan step 4).</summary>
    public DbSet<OrderFulfillment> OrderFulfillments => Set<OrderFulfillment>();

    /// <summary>Faz 47 — the unchangeable history of every fulfillment.</summary>
    public DbSet<OrderFulfillmentEvent> OrderFulfillmentEvents => Set<OrderFulfillmentEvent>();

    /// <summary>Faz 47 — per-company warehouse module switch and delay thresholds.</summary>
    public DbSet<TenantWarehouseSettings> TenantWarehouseSettings => Set<TenantWarehouseSettings>();

    /// <summary>Faz 49 — warehouse TVs paired to a company (plan step 7).</summary>
    public DbSet<DisplayDevice> DisplayDevices => Set<DisplayDevice>();

    /// <summary>Faz 49 — codes TVs show until a manager pairs them.</summary>
    public DbSet<DisplayPairingCode> DisplayPairingCodes => Set<DisplayPairingCode>();

    /// <summary>GOAL_PANEL_ERPSIZ E7b — who changed what from the portal in a native tenant's own books (D5).</summary>
    public DbSet<NativeAuditLogEntry> NativeAuditLogEntries => Set<NativeAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApprovalRequest>(b =>
        {
            b.ToTable("approval_requests");
            b.HasKey(x => x.Id);
            b.Property(x => x.ExternalId).IsRequired().HasMaxLength(128);
            b.Property(x => x.Kind).IsRequired().HasMaxLength(16);
            b.Property(x => x.CounterpartyName).IsRequired().HasMaxLength(200);
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.SummaryJson).HasColumnType("jsonb");
            b.Property(x => x.DocumentsJson).HasColumnType("jsonb");
            b.Property(x => x.Status).IsRequired().HasMaxLength(16);
            b.Property(x => x.RequestedByName).HasMaxLength(120);
            b.Property(x => x.DecidedByName).HasMaxLength(120);
            b.Property(x => x.DecisionNote).HasMaxLength(500);
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.ExternalId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status, x.RequestedSeq });
            b.HasIndex(x => new { x.TenantId, x.UpdatedSeq });
        });

        modelBuilder.Entity<ApprovalRequestEvent>(b =>
        {
            b.ToTable("approval_request_events");
            b.HasKey(x => x.Id);
            b.Property(x => x.Action).IsRequired().HasMaxLength(16);
            b.Property(x => x.ByName).HasMaxLength(120);
            b.Property(x => x.Note).HasMaxLength(500);
            b.HasOne(x => x.Request).WithMany().HasForeignKey(x => x.RequestId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.RequestId, x.AtSeq });
        });

        modelBuilder.Entity<TenantApprovalRules>(b =>
        {
            b.ToTable("tenant_approval_rules");
            b.HasKey(x => x.TenantId);
            b.Property(x => x.UpdatedByName).HasMaxLength(120);
            b.HasOne(x => x.Tenant).WithOne().HasForeignKey<TenantApprovalRules>(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderFulfillment>(b =>
        {
            b.ToTable("order_fulfillments");
            b.HasKey(x => x.Id);
            b.Property(x => x.OrderNo).IsRequired().HasMaxLength(64);
            b.Property(x => x.CustomerCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.CustomerName).IsRequired().HasMaxLength(200);
            b.Property(x => x.SalespersonName).IsRequired().HasMaxLength(120);
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.ItemQuantity).HasPrecision(18, 3);
            b.Property(x => x.ItemsJson).HasColumnType("jsonb");
            b.Property(x => x.Status).IsRequired().HasMaxLength(16);
            b.Property(x => x.AssigneeName).HasMaxLength(120);
            b.Property(x => x.VehiclePlate).HasMaxLength(16);
            b.Property(x => x.ErpState).IsRequired().HasMaxLength(16);
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Job>().WithMany().HasForeignKey(x => x.SourceJobId).OnDelete(DeleteBehavior.Cascade);
            // One order per job: a document retried by the phone or posted by an approval is queued once.
            b.HasIndex(x => new { x.TenantId, x.SourceJobId }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Status, x.QueuedSeq });
            b.HasIndex(x => new { x.TenantId, x.UpdatedSeq });
            b.HasIndex(x => new { x.TenantId, x.AssigneeUserId, x.PackedAtUtc });
        });

        modelBuilder.Entity<OrderFulfillmentEvent>(b =>
        {
            b.ToTable("order_fulfillment_events");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();
            b.Property(x => x.FromStatus).HasMaxLength(16);
            b.Property(x => x.ToStatus).IsRequired().HasMaxLength(16);
            b.Property(x => x.Action).IsRequired().HasMaxLength(24);
            b.Property(x => x.ActorName).IsRequired().HasMaxLength(120);
            b.Property(x => x.DeviceId).HasMaxLength(128);
            b.Property(x => x.Note).HasMaxLength(500);
            b.HasOne(x => x.Fulfillment).WithMany().HasForeignKey(x => x.FulfillmentId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.FulfillmentId, x.OccurredAtUtc });
            // Faz 50: the reports read a range of days.
            b.HasIndex(x => new { x.TenantId, x.OccurredAtUtc });
            b.HasIndex(x => new { x.TenantId, x.ActorUserId, x.OccurredAtUtc });
        });

        modelBuilder.Entity<TenantWarehouseSettings>(b =>
        {
            b.ToTable("tenant_warehouse_settings");
            b.HasKey(x => x.TenantId);
            b.HasOne(x => x.Tenant).WithOne().HasForeignKey<TenantWarehouseSettings>(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DisplayDevice>(b =>
        {
            b.ToTable("display_devices");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(80);
            b.Property(x => x.TokenHash).IsRequired().HasMaxLength(64).IsFixedLength();
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.TenantId);
        });

        modelBuilder.Entity<DisplayPairingCode>(b =>
        {
            b.ToTable("display_pairing_codes");
            b.HasKey(x => x.Code);
            b.Property(x => x.Code).HasMaxLength(6).IsFixedLength();
            b.Property(x => x.PairingSecretHash).IsRequired().HasMaxLength(64).IsFixedLength();
        });

        modelBuilder.Entity<NativeAuditLogEntry>(b =>
        {
            b.ToTable("native_audit_log");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserName).IsRequired().HasMaxLength(200);
            b.Property(x => x.Entity).IsRequired().HasMaxLength(40);
            b.Property(x => x.EntityKey).IsRequired().HasMaxLength(128);
            b.Property(x => x.Action).IsRequired().HasMaxLength(20);
            b.Property(x => x.Summary).IsRequired().HasMaxLength(500);
            b.Property(x => x.BeforeJson).HasColumnType("jsonb");
            b.Property(x => x.AfterJson).HasColumnType("jsonb");
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            // "Geçmiş" for one card/movement: every entry for its key, newest first.
            b.HasIndex(x => new { x.TenantId, x.Entity, x.EntityKey, x.CreatedAtUtc });
            // /denetim: the company-wide list, newest first.
            b.HasIndex(x => new { x.TenantId, x.CreatedAtUtc });
        });

        modelBuilder.Entity<NativeStockLevel>(b =>
        {
            b.ToTable("native_stock_levels");
            b.HasKey(x => new { x.TenantId, x.StockCode, x.WarehouseNo });
            b.Property(x => x.StockCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NativeCustomerBalance>(b =>
        {
            b.ToTable("native_customer_balances");
            b.HasKey(x => new { x.TenantId, x.CustomerCode });
            b.Property(x => x.CustomerCode).IsRequired().HasMaxLength(64);
            b.Property(x => x.Balance).HasPrecision(18, 2);
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MobileUser>(b =>
        {
            b.ToTable("mobile_users");
            b.HasKey(x => x.Id);
            b.Property(x => x.Username).IsRequired().HasMaxLength(64);
            b.Property(x => x.FullName).IsRequired().HasMaxLength(120);
            b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(100);
            b.Property(x => x.Role).IsRequired().HasMaxLength(16);
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            // A deleted user's name is free again; live names stay unique.
            b.HasIndex(x => new { x.TenantId, x.Username }).IsUnique().HasFilter("\"DeletedAtUtc\" IS NULL");
            b.HasIndex(x => new { x.TenantId, x.IsActive });
            b.HasMany(x => x.Roles).WithOne(r => r.User).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MobileUserRole>(b =>
        {
            b.ToTable("mobile_user_roles");
            b.HasKey(x => new { x.UserId, x.Role });
            b.Property(x => x.Role).IsRequired().HasMaxLength(16);
            b.HasIndex(x => x.Role);
        });

        modelBuilder.Entity<MobileDevice>(b =>
        {
            b.ToTable("mobile_devices");
            b.HasKey(x => x.Id);
            b.Property(x => x.DeviceId).IsRequired().HasMaxLength(128);
            b.Property(x => x.AppVersion).HasMaxLength(64);
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.LastUser).WithMany().HasForeignKey(x => x.LastUserId).OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => new { x.TenantId, x.DeviceId }).IsUnique();
            b.HasIndex(x => x.LastUserId);
        });

        modelBuilder.Entity<TenantSubscription>(b =>
        {
            b.ToTable("tenant_subscriptions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Source).IsRequired().HasMaxLength(32);
            b.Property(x => x.Reference).HasMaxLength(128);
            b.Property(x => x.Note).HasMaxLength(500);
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.TenantId).IsUnique().HasFilter("\"IsCurrent\" = true");
            b.HasIndex(x => new { x.TenantId, x.CreatedAtUtc });
        });

        modelBuilder.Entity<Tenant>(b =>
        {
            b.ToTable("tenants");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.MaxDeviceCount).HasDefaultValue(1);
            b.HasIndex(x => x.Name).IsUnique();
            b.Property(x => x.Code).HasMaxLength(16);
            b.Property(x => x.DataSource).IsRequired().HasMaxLength(16).HasDefaultValue(TenantDataSources.Erp);
            b.HasIndex(x => x.Code).IsUnique();
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

        modelBuilder.Entity<AgentHeartbeatLogEntry>(b =>
        {
            b.ToTable("agent_heartbeat_log");
            b.HasKey(x => x.Id);
            b.Property(x => x.Status).HasMaxLength(64);
            b.Property(x => x.LastSyncResult).HasMaxLength(32);
            b.Property(x => x.LastErrorCode).HasMaxLength(64);
            b.Property(x => x.LastError).HasMaxLength(1024);
            b.Property(x => x.AppVersion).HasMaxLength(64);
            b.Property(x => x.HostKind).HasMaxLength(32);
            b.Property(x => x.ErpKind).HasMaxLength(32);
            b.Property(x => x.ErpVersion).HasMaxLength(64);
            // The one query this table exists for: "what did this agent report, newest first".
            b.HasIndex(x => new { x.AgentId, x.ReceivedAtUtc });
            b.HasIndex(x => new { x.TenantId, x.ReceivedAtUtc });
        });

        modelBuilder.Entity<Agent>(b =>
        {
            b.Property(x => x.LastAppVersion).HasMaxLength(64);
            b.Property(x => x.LastHostKind).HasMaxLength(32);
            b.Property(x => x.LastErpKind).HasMaxLength(32);
            b.Property(x => x.LastErpVersion).HasMaxLength(64);
            b.Property(x => x.LastSyncResult).HasMaxLength(32);
            b.Property(x => x.LastErrorCode).HasMaxLength(64);
            b.Property(x => x.LastError).HasMaxLength(1024);
        });

        modelBuilder.Entity<ApprovalRequest>(b =>
            b.Property(x => x.CorrelationId).HasMaxLength(ErpBridge.CentralApi.LogCenter.CorrelationId.MaxLength));

        modelBuilder.Entity<Job>(b =>
        {
            b.Property(x => x.CorrelationId).HasMaxLength(ErpBridge.CentralApi.LogCenter.CorrelationId.MaxLength);
            b.ToTable("jobs");
            b.Property(x => x.ErpType).IsRequired().HasMaxLength(32).HasDefaultValue("Mikro");
            b.HasKey(x => x.Id);
            b.Property(x => x.ExternalId).IsRequired().HasMaxLength(128);
            b.Property(x => x.DocumentType).IsRequired().HasMaxLength(64);
            b.Property(x => x.PayloadJson).HasColumnType("jsonb");
            // Kiralama alanlari eszamanlilik anahtari: iki ajan (Windows servisi ve tepsi
            // uygulamasi) ayni kiracida ayni anda is cekerse UPDATE'in WHERE'i satirin hala
            // okundugu gibi oldugunu dogrular. Kaybeden taraf 0 satir gunceller ve isi almaz;
            // boylece ayni belge Mikro'ya iki kez yazilmaz.
            b.Property(x => x.Status).HasConversion<int>().IsConcurrencyToken();
            b.Property(x => x.LeasedUntilMs).IsConcurrencyToken();
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

        modelBuilder.Entity<BootstrapSnapshot>(b =>
        {
            b.ToTable("bootstrap_snapshots");
            b.HasKey(x => x.Id);
            b.Property(x => x.SourceDatabase).IsRequired().HasMaxLength(128);
            b.HasIndex(x => x.TenantId).HasFilter("\"IsActive\" = true").IsUnique();
            b.HasIndex(x => new { x.TenantId, x.PulledAtUtc });
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BootstrapSnapshotChunk>(b =>
        {
            b.ToTable("bootstrap_snapshot_chunks");
            b.HasKey(x => x.Id);
            b.Property(x => x.Section).IsRequired().HasMaxLength(64);
            b.Property(x => x.PayloadJson).HasColumnType("jsonb");
            b.HasIndex(x => new { x.SnapshotId, x.Section, x.ChunkIndex }).IsUnique();
            b.HasOne(x => x.Snapshot).WithMany("Chunks").HasForeignKey(x => x.SnapshotId).OnDelete(DeleteBehavior.Cascade);
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

        // Admin refresh-token rotation handles. The unique index on
        // (AdminUserId, TokenHash) makes a duplicate row a no-op (defence in
        // depth — the raw token is 32 random bytes). The secondary index on
        // ExpiresAtUtc supports a future cleanup worker that purges expired
        // rows in batches.
        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.ToTable("refresh_tokens");
            b.HasKey(x => x.Id);
            b.Property(x => x.TokenHash).IsRequired().HasMaxLength(64);
            b.Property(x => x.CreatedByIp).IsRequired().HasMaxLength(64);
            b.HasIndex(x => new { x.AdminUserId, x.TokenHash }).IsUnique();
            b.HasIndex(x => x.ExpiresAtUtc);
        });

        modelBuilder.Entity<ApiKey>(b =>
        {
            b.ToTable("api_keys");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.KeyPrefix).IsRequired().HasMaxLength(32);
            b.Property(x => x.KeyHash).IsRequired().HasMaxLength(64);
            b.Property(x => x.KeySalt).IsRequired().HasMaxLength(64);
            b.Property(x => x.VaultCiphertext).HasColumnType("bytea");
            b.Property(x => x.VaultNonce).HasMaxLength(12);
            b.Property(x => x.VaultTag).HasMaxLength(16);
            b.Property(x => x.Scopes).HasColumnType("text[]");
            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.KeyPrefix });
        });

        modelBuilder.Entity<ErpCompany>(b =>
        {
            b.ToTable("erp_companies");
            b.HasKey(x => x.Id);
            b.Property(x => x.Code).IsRequired().HasMaxLength(64);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.SourceDatabase).IsRequired().HasMaxLength(128);
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AgentCompanyAssignment>(b =>
        {
            b.ToTable("agent_company_assignments");
            b.HasKey(x => new { x.AgentId, x.ErpCompanyId });
            b.HasOne(x => x.Agent).WithMany(x => x.CompanyAssignments).HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ErpCompany).WithMany(x => x.AgentAssignments).HasForeignKey(x => x.ErpCompanyId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.ErpCompanyId);
        });

        modelBuilder.Entity<ApiKeySecretAccessAudit>(b =>
        {
            b.ToTable("api_key_secret_access_audits");
            b.HasKey(x => x.Id);
            b.Property(x => x.Action).IsRequired().HasMaxLength(32);
            b.Property(x => x.RemoteIp).HasMaxLength(64);
            b.HasOne(x => x.ApiKey)
                .WithMany()
                .HasForeignKey(x => x.ApiKeyId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.ApiKeyId, x.AccessedAtUtc });
            b.HasIndex(x => x.AdminUserId);
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

        // Log Merkezi L0: one table for every source (phone, Windows agent, portal, admin, this API).
        // Column bounds live in LogEventWriter; times are filtered and sorted on the *Ms columns.
        modelBuilder.Entity<LogEvent>(b =>
        {
            b.ToTable("log_events");
            b.HasKey(x => x.Id);
            b.Property(x => x.EventId).IsRequired().HasMaxLength(LogEventWriter.EventIdMax);
            b.Property(x => x.Source).IsRequired().HasMaxLength(LogEventWriter.SourceMax);
            b.Property(x => x.Severity).IsRequired().HasMaxLength(LogEventWriter.SeverityMax);
            b.Property(x => x.Kind).IsRequired().HasMaxLength(LogEventWriter.KindMax);
            b.Property(x => x.Operation).IsRequired().HasMaxLength(LogEventWriter.OperationMax);
            b.Property(x => x.Screen).IsRequired().HasMaxLength(LogEventWriter.ScreenMax);
            b.Property(x => x.Message).IsRequired().HasMaxLength(LogEventWriter.MessageMax);
            b.Property(x => x.ExceptionType).IsRequired().HasMaxLength(LogEventWriter.ExceptionTypeMax);
            b.Property(x => x.StackTrace).IsRequired().HasMaxLength(LogEventWriter.StackTraceMax);
            b.Property(x => x.AppVersion).IsRequired().HasMaxLength(LogEventWriter.AppVersionMax);
            b.Property(x => x.OsVersion).IsRequired().HasMaxLength(LogEventWriter.OsVersionMax);
            b.Property(x => x.DeviceModel).IsRequired().HasMaxLength(LogEventWriter.DeviceModelMax);
            b.Property(x => x.DeviceId).HasMaxLength(LogEventWriter.DeviceIdMax);
            b.Property(x => x.SessionId).HasMaxLength(LogEventWriter.SessionIdMax);
            b.Property(x => x.CorrelationId).HasMaxLength(LogEventWriter.CorrelationIdMax);
            b.Property(x => x.HttpMethod).HasMaxLength(LogEventWriter.HttpMethodMax);
            b.Property(x => x.HttpRoute).HasMaxLength(LogEventWriter.HttpRouteMax);
            b.Property(x => x.PropertiesJson).IsRequired().HasColumnType("jsonb");
            b.Property(x => x.BreadcrumbsJson).IsRequired().HasColumnType("jsonb");
            // Not (TenantId, Source, EventId): PostgreSQL treats NULL tenants as distinct, so tenantless
            // retries would slip past. Event ids are GUIDs; source + id is unique enough.
            b.HasIndex(x => new { x.Source, x.EventId }).IsUnique();
            b.HasIndex(x => x.OccurredAtMs);
            b.HasIndex(x => new { x.TenantId, x.OccurredAtMs });
            b.HasIndex(x => new { x.Source, x.Severity, x.OccurredAtMs });
            b.HasIndex(x => new { x.FingerprintId, x.OccurredAtMs });
            b.HasIndex(x => x.CorrelationId);
            b.HasIndex(x => new { x.DeviceId, x.OccurredAtMs });
            b.HasIndex(x => new { x.UserId, x.OccurredAtMs });
            b.HasIndex(x => new { x.AgentId, x.OccurredAtMs });
            b.HasIndex(x => new { x.Severity, x.ReceivedAtMs });
        });

        modelBuilder.Entity<LogErrorGroup>(b =>
        {
            b.ToTable("log_error_groups");
            b.HasKey(x => x.Id);
            b.Property(x => x.Fingerprint).IsRequired().HasMaxLength(64);
            b.Property(x => x.Source).IsRequired().HasMaxLength(LogEventWriter.SourceMax);
            b.Property(x => x.Kind).IsRequired().HasMaxLength(LogEventWriter.KindMax);
            b.Property(x => x.ExceptionType).IsRequired().HasMaxLength(LogEventWriter.ExceptionTypeMax);
            b.Property(x => x.Operation).IsRequired().HasMaxLength(LogEventWriter.OperationMax);
            b.Property(x => x.Severity).IsRequired().HasMaxLength(LogEventWriter.SeverityMax);
            b.Property(x => x.SampleMessage).IsRequired().HasMaxLength(LogEventWriter.MessageMax);
            b.Property(x => x.TopFrame).IsRequired().HasMaxLength(300);
            b.Property(x => x.LastAppVersion).IsRequired().HasMaxLength(LogEventWriter.AppVersionMax);
            b.Property(x => x.Status).IsRequired().HasMaxLength(16);
            b.Property(x => x.StatusChangedBy).HasMaxLength(120);
            b.Property(x => x.Note).HasMaxLength(1000);
            b.HasIndex(x => x.Fingerprint).IsUnique();
            b.HasIndex(x => new { x.Status, x.LastSeenMs });
            b.HasIndex(x => x.LastSeenMs);
        });

        modelBuilder.Entity<LogSettings>(b =>
        {
            b.ToTable("log_settings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(x => x.UpdatedBy).HasMaxLength(120);
        });

        // Goal ERP yazım Y1a: how phone documents are written into the company's ERP.
        modelBuilder.Entity<ErpWriteSettings>(b =>
        {
            b.ToTable("erp_write_settings");
            b.HasKey(x => x.TenantId);
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.SalesDocumentKind).IsRequired().HasMaxLength(16).HasDefaultValue(SalesDocumentKinds.Order);
            b.Property(x => x.OrderApprovalMode).IsRequired().HasMaxLength(16).HasDefaultValue(OrderApprovalModes.Approved);
            foreach (var series in new[] { nameof(Domain.ErpWriteSettings.OrderSeries), nameof(Domain.ErpWriteSettings.DispatchSeries), nameof(Domain.ErpWriteSettings.InvoiceSeries), nameof(Domain.ErpWriteSettings.ReturnSeries), nameof(Domain.ErpWriteSettings.CollectionSeries) })
                b.Property<string>(series).IsRequired().HasMaxLength(Domain.ErpWriteSettings.SeriesMaxLength).HasDefaultValue(string.Empty);
            foreach (var code in new[] { nameof(Domain.ErpWriteSettings.DefaultCashCode), nameof(Domain.ErpWriteSettings.DefaultCardBankCode), nameof(Domain.ErpWriteSettings.DefaultTransferBankCode), nameof(Domain.ErpWriteSettings.DefaultSalespersonCode), nameof(Domain.ErpWriteSettings.ResponsibilityCenterCode), nameof(Domain.ErpWriteSettings.ProjectCode) })
                b.Property<string?>(code).HasMaxLength(Domain.ErpWriteSettings.CodeMaxLength);
            b.Property(x => x.ChequePortfolioCode).IsRequired().HasMaxLength(Domain.ErpWriteSettings.CodeMaxLength).HasDefaultValue(Domain.ErpWriteSettings.DefaultChequePortfolioCode);
            b.Property(x => x.NotePortfolioCode).IsRequired().HasMaxLength(Domain.ErpWriteSettings.CodeMaxLength).HasDefaultValue(Domain.ErpWriteSettings.DefaultNotePortfolioCode);
        });

        modelBuilder.Entity<MobileUserErpMapping>(b =>
        {
            b.ToTable("mobile_user_erp_mappings");
            b.HasKey(x => x.UserId);
            b.HasOne(x => x.User).WithOne().HasForeignKey<MobileUserErpMapping>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => x.TenantId);
            foreach (var code in new[] { nameof(MobileUserErpMapping.SalespersonCode), nameof(MobileUserErpMapping.CashCode), nameof(MobileUserErpMapping.CardBankCode), nameof(MobileUserErpMapping.TransferBankCode) })
                b.Property<string?>(code).HasMaxLength(Domain.ErpWriteSettings.CodeMaxLength);
            foreach (var series in new[] { nameof(MobileUserErpMapping.OrderSeries), nameof(MobileUserErpMapping.DispatchSeries), nameof(MobileUserErpMapping.InvoiceSeries), nameof(MobileUserErpMapping.ReturnSeries), nameof(MobileUserErpMapping.CollectionSeries) })
                b.Property<string?>(series).HasMaxLength(Domain.ErpWriteSettings.SeriesMaxLength);
        });

        // Faz 13.1: ChangeSetRecord — per-table trigger-based change-set
        // payload. The unique index on
        // (TenantId, SourceDatabase, TableName, LastTriggerRecNo) makes
        // duplicate agent pushes a no-op.
        modelBuilder.Entity<ChangeSetRecord>(b =>
        {
            b.ToTable("change_sets");
            b.HasKey(x => x.Id);
            b.Property(x => x.ErpType).IsRequired().HasMaxLength(32).HasDefaultValue("Mikro");
            b.Property(x => x.SourceDatabase).IsRequired().HasMaxLength(128);
            b.Property(x => x.TableKey).IsRequired().HasMaxLength(128);
            b.Property(x => x.TableName).IsRequired().HasMaxLength(128);
            b.Property(x => x.PayloadJson).HasColumnType("jsonb");
            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.SourceDatabase, x.TableName, x.LastTriggerRecNo, x.LastDeleteRecNo }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.TableName, x.PulledAtUtc });
        });

        modelBuilder.Entity<MobileSyncQueueItem>(b =>
        {
            b.ToTable("mobile_sync_queue");
            b.HasKey(x => x.Sequence);
            b.Property(x => x.Sequence).ValueGeneratedOnAdd();
            b.Property(x => x.SourceDatabase).IsRequired().HasMaxLength(128);
            b.Property(x => x.TableName).IsRequired().HasMaxLength(128);
            b.Property(x => x.EntityType).IsRequired().HasMaxLength(32);
            b.Property(x => x.Operation).IsRequired().HasMaxLength(16);
            b.Property(x => x.RecordKey).IsRequired().HasMaxLength(255);
            b.Property(x => x.SourceRecordKey).HasMaxLength(255);
            b.Property(x => x.PayloadJson).HasColumnType("jsonb");
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.Sequence });
            b.HasIndex(x => new { x.TenantId, x.SourceDatabase, x.TableName, x.SourceRecordKey });
            b.HasIndex(x => new { x.TenantId, x.SourceDatabase, x.TableName, x.TriggerRecNo, x.Operation, x.RecordKey }).IsUnique();
        });

        // Faz 26: the table mobile devices page through. The unique index on
        // (TenantId, UpdatedSeq) is what lets a device resume from a cursor: it
        // orders the feed and, being unique, guarantees a page boundary can never
        // split two rows that share a position.
        modelBuilder.Entity<MobileRecord>(b =>
        {
            b.ToTable("mobile_records");
            b.HasKey(x => new { x.TenantId, x.Entity, x.RecordKey });
            b.Property(x => x.Entity).IsRequired().HasMaxLength(32);
            b.Property(x => x.RecordKey).IsRequired().HasMaxLength(255);
            b.Property(x => x.StockKey).HasMaxLength(255);
            b.Property(x => x.CustomerKey).HasMaxLength(255);
            b.Property(x => x.SourceRecordKey).HasMaxLength(255);
            b.Property(x => x.SourceDatabase).HasMaxLength(255);
            b.Property(x => x.PayloadJson).HasColumnType("jsonb");
            b.Property(x => x.PayloadSha256).HasMaxLength(64);
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.UpdatedSeq }).IsUnique();
            // Cascading a stock or customer deletion to the record's children is
            // an indexed lookup rather than a scan of every section.
            b.HasIndex(x => new { x.TenantId, x.StockKey });
            b.HasIndex(x => new { x.TenantId, x.CustomerKey });
            // A delete event is translated from the ERP identity to the code
            // with one indexed lookup.
            b.HasIndex(x => new { x.TenantId, x.Entity, x.SourceDatabase, x.SourceRecordKey });
            // Tombstone retention sweeps by age.
            b.HasIndex(x => new { x.TenantId, x.IsDeleted, x.UpdatedAtUtc });
        });

        modelBuilder.Entity<TenantSyncCounter>(b =>
        {
            b.ToTable("tenant_sync_counter");
            b.HasKey(x => x.TenantId);
            b.Property(x => x.TenantId).ValueGeneratedNever();
            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Cascade);
        });

        // Faz 15.6: append-only audit log. The unique index on
        // (TenantId, IdempotencyKey, Direction) makes a duplicate bundle a
        // no-op for the audit side too — the (IdempotencyKey, Direction)
        // tuple identifies one row, not three.
        modelBuilder.Entity<ChangeSetAuditEntry>(b =>
        {
            b.ToTable("change_set_audit_log");
            b.HasKey(x => x.Id);
            b.Property(x => x.ErpType).IsRequired().HasMaxLength(32).HasDefaultValue("Mikro");
            b.Property(x => x.SourceDatabase).IsRequired().HasMaxLength(128);
            b.Property(x => x.TableKey).IsRequired().HasMaxLength(128);
            b.Property(x => x.TableName).IsRequired().HasMaxLength(128);
            b.Property(x => x.Direction).IsRequired().HasMaxLength(16);
            b.Property(x => x.PayloadJson).HasColumnType("jsonb");
            b.Property(x => x.PayloadSha256).IsRequired().HasMaxLength(64);
            b.Property(x => x.AgentId).HasMaxLength(128);
            b.Property(x => x.IdempotencyKey).IsRequired().HasMaxLength(128);
            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.TenantId, x.IdempotencyKey, x.Direction }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.TableName, x.ReceivedAtUtc });
            b.HasIndex(x => new { x.TenantId, x.TableName, x.LastTriggerRecNo });
        });

        // Faz 15.5: parameters mirror. Unique on (TenantId, SourceDatabase,
        // ParametreProgram, ParametreUser, ParametreID) lets the agent's push
        // be a no-op for unchanged rows.
        modelBuilder.Entity<ParameterCatalogEntry>(b =>
        {
            b.ToTable("parameter_catalog_entries");
            b.HasKey(x => x.Id);
            b.Property(x => x.Program).IsRequired().HasMaxLength(40);
            b.Property(x => x.CatalogMethod).IsRequired().HasMaxLength(64);
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.DefaultValue).IsRequired();
            b.Property(x => x.DefaultSource).HasMaxLength(64);
            b.Property(x => x.ScopeKind).IsRequired().HasMaxLength(32);
            b.Property(x => x.ScopeFields).IsRequired().HasMaxLength(64);
            b.Property(x => x.User).IsRequired().HasMaxLength(100);
            b.Property(x => x.AnaGrubu).IsRequired().HasMaxLength(100);
            b.Property(x => x.AltGrubu).IsRequired().HasMaxLength(100);
            b.Property(x => x.Editor).HasMaxLength(32);
            b.Property(x => x.ReferenceKind).HasMaxLength(32);
            b.Property(x => x.SecretSource).HasMaxLength(16);
            b.Property(x => x.Label).HasMaxLength(512);
            b.Property(x => x.TabPath).HasMaxLength(512);
            b.Property(x => x.SourceBuild).IsRequired().HasMaxLength(64);

            // ParametreID is the real key and it is unique only inside a set: a program can own
            // several sets, and the same name appears in different sets with different ids.
            b.HasIndex(x => new { x.CatalogMethod, x.ParametreId }).IsUnique();

            // The panel lists a program's parameters in editor order.
            b.HasIndex(x => new { x.Program, x.EditorOrder });
        });

        modelBuilder.Entity<ParameterValue>(b =>
        {
            b.ToTable("parameter_values");
            b.HasKey(x => x.Id);
            b.Property(x => x.Scope1).IsRequired().HasMaxLength(100);
            b.Property(x => x.Scope2).IsRequired().HasMaxLength(100);
            b.Property(x => x.Value).IsRequired();

            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // A company is never deleted out from under its values; removing one has to be a
            // deliberate act that deals with them first.
            b.HasOne(x => x.ErpCompany)
                .WithMany()
                .HasForeignKey(x => x.ErpCompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.CatalogEntry)
                .WithMany()
                .HasForeignKey(x => x.ParameterCatalogEntryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Deleting a mobile user keeps their values for history; they simply stop being
            // published (D5b). Cascading would erase the record of what was configured.
            b.HasOne(x => x.MobileUser)
                .WithMany()
                .HasForeignKey(x => x.MobileUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // One value per parameter per scope instance per company. The catalogue entry stands
            // for (program, AnaGrubu, AltGrubu, ParametreID), which alone is not unique across
            // sets — 994 of the 3,679 (program, id) pairs address more than one parameter.
            b.HasIndex(x => new
            {
                x.TenantId,
                x.ErpCompanyId,
                x.ParameterCatalogEntryId,
                x.MobileUserId,
                x.Scope1,
                x.Scope2,
            }).IsUnique();

            // The panel and the mirror both read "everything for this company".
            b.HasIndex(x => new { x.TenantId, x.ErpCompanyId });
        });

        modelBuilder.Entity<ParameterRevision>(b =>
        {
            b.ToTable("parameter_revisions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Scope1).IsRequired().HasMaxLength(100);
            b.Property(x => x.Scope2).IsRequired().HasMaxLength(100);

            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ErpCompany).WithMany().HasForeignKey(x => x.ErpCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.MobileUser).WithMany().HasForeignKey(x => x.MobileUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // One counter per scope — the same tuple the values themselves are addressed by.
            b.HasIndex(x => new { x.TenantId, x.ErpCompanyId, x.MobileUserId, x.Scope1, x.Scope2 })
                .IsUnique();
        });

        modelBuilder.Entity<ParameterAuditEntry>(b =>
        {
            b.ToTable("parameter_audit");
            b.HasKey(x => x.Id);
            b.Property(x => x.Scope1).IsRequired().HasMaxLength(100);
            b.Property(x => x.Scope2).IsRequired().HasMaxLength(100);
            b.Property(x => x.Outcome).IsRequired().HasMaxLength(16);
            b.Property(x => x.Source).IsRequired().HasMaxLength(32);
            b.Property(x => x.Actor).IsRequired().HasMaxLength(200);

            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            // The catalogue entry is never deleted (withdrawn ones are marked), so the trail
            // keeps pointing at a real parameter for as long as it is retained.
            b.HasOne(x => x.CatalogEntry).WithMany().HasForeignKey(x => x.ParameterCatalogEntryId)
                .OnDelete(DeleteBehavior.Restrict);

            // "What happened to this parameter" and "what changed lately" are the two questions.
            b.HasIndex(x => new { x.TenantId, x.ErpCompanyId, x.ParameterCatalogEntryId, x.AtUtc });
            b.HasIndex(x => new { x.TenantId, x.AtUtc });
        });

        modelBuilder.Entity<ParameterMirrorReport>(b =>
        {
            b.ToTable("parameter_mirror_reports");
            b.HasKey(x => x.Id);
            b.Property(x => x.ErrorText).HasMaxLength(2000);

            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ErpCompany).WithMany().HasForeignKey(x => x.ErpCompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // The agent may be removed and re-registered; the history of what it wrote stays.
            b.HasOne(x => x.Agent).WithMany().HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.SetNull);

            // "How is this company's mirror doing" is the question the panel asks.
            b.HasIndex(x => new { x.TenantId, x.ErpCompanyId, x.AtUtc });
        });

        modelBuilder.Entity<ParameterMirrorDrift>(b =>
        {
            b.ToTable("parameter_mirror_drifts");
            b.HasKey(x => x.Id);
            b.Property(x => x.Scope1).IsRequired().HasMaxLength(100);
            b.Property(x => x.Scope2).IsRequired().HasMaxLength(100);

            b.HasOne(x => x.Report).WithMany(r => r.Drifts).HasForeignKey(x => x.ParameterMirrorReportId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.CatalogEntry).WithMany().HasForeignKey(x => x.ParameterCatalogEntryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Restrict, like the values themselves: a removed user's drift stays explainable.
            b.HasOne(x => x.MobileUser).WithMany().HasForeignKey(x => x.MobileUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.ParameterMirrorReportId);
        });

        modelBuilder.Entity<ForaImportBatch>(b =>
        {
            b.ToTable("fora_import_batches");
            b.HasKey(x => x.Id);
            b.Property(x => x.State).IsRequired().HasMaxLength(16);

            b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.ErpCompany).WithMany().HasForeignKey(x => x.ErpCompanyId)
                .OnDelete(DeleteBehavior.Cascade);

            // The agent may be removed and re-registered; what it scanned stays.
            b.HasOne(x => x.Agent).WithMany().HasForeignKey(x => x.AgentId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => new { x.TenantId, x.ErpCompanyId, x.ScannedAtUtc });
        });

        modelBuilder.Entity<ForaImportRow>(b =>
        {
            b.ToTable("fora_import_rows");
            b.HasKey(x => x.Id);
            b.Property(x => x.ParametreProgram).IsRequired().HasMaxLength(40);
            b.Property(x => x.ParametreUser).IsRequired().HasMaxLength(40);
            b.Property(x => x.AnaGrubu).IsRequired().HasMaxLength(100);
            b.Property(x => x.AltGrubu).IsRequired().HasMaxLength(100);
            b.Property(x => x.ParametreAdi).IsRequired().HasMaxLength(100);

            b.HasOne(x => x.Batch).WithMany(x => x.Rows).HasForeignKey(x => x.ForaImportBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Both nullable on purpose: a row the catalogue does not declare, or a username that
            // matches no active user, is exactly what the reviewer has to see.
            b.HasOne(x => x.CatalogEntry).WithMany().HasForeignKey(x => x.ParameterCatalogEntryId)
                .OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.MobileUser).WithMany().HasForeignKey(x => x.MobileUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.ForaImportBatchId);
        });

        modelBuilder.Entity<ParameterRecord>(b =>
        {
            b.ToTable("parameter_records");
            b.HasKey(x => x.Id);
            b.Property(x => x.SourceDatabase).IsRequired().HasMaxLength(128);
            b.Property(x => x.ParametreProgram).IsRequired().HasMaxLength(25);
            b.Property(x => x.ParametreUser).IsRequired().HasMaxLength(25);
            b.Property(x => x.ParametreAnaGrubu).IsRequired().HasMaxLength(40);
            b.Property(x => x.ParametreAltGrubu).IsRequired().HasMaxLength(40);
            b.Property(x => x.ParametreID).IsRequired().HasMaxLength(40);
            b.Property(x => x.ParametreAdi).IsRequired().HasMaxLength(127);
            b.Property(x => x.ParametreDegeri).IsRequired().HasMaxLength(255);
            b.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new
            {
                x.TenantId,
                x.SourceDatabase,
                x.ParametreProgram,
                x.ParametreUser,
                x.ParametreID,
            }).IsUnique();
        });
    }
}
