namespace ErpBridge.CentralApi.Domain;

/// <summary>One Mikro company owned by a tenant. SQL credentials stay on its Windows agent.</summary>
public sealed class ErpCompany
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SourceDatabase { get; set; } = string.Empty;
    public int CompanyNo { get; set; }
    public int BranchNo { get; set; }
    public int WarehouseNo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<AgentCompanyAssignment> AgentAssignments { get; set; } = new List<AgentCompanyAssignment>();
}

/// <summary>Explicit authorization for an agent to synchronize an ERP company.</summary>
public sealed class AgentCompanyAssignment
{
    public Guid AgentId { get; set; }
    public Agent? Agent { get; set; }
    public Guid ErpCompanyId { get; set; }
    public ErpCompany? ErpCompany { get; set; }
    public DateTimeOffset AssignedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
