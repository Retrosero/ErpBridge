namespace ErpBridge.CentralApi.Contracts;

/// <summary>A count of documents and their total amount.</summary>
public sealed class PortalMoneyLine
{
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>GET /api/v1/portal/summary — one business day of the whole company.</summary>
public sealed class PortalSummaryResponse
{
    public string Date { get; set; } = string.Empty;
    public string DataSource { get; set; } = string.Empty;
    public PortalMoneyLine Sales { get; set; } = new();
    public PortalMoneyLine Collections { get; set; } = new();
    public PortalMoneyLine Disbursements { get; set; } = new();
    public PortalMoneyLine Returns { get; set; } = new();
    public int VisitsPlanned { get; set; }
    public int VisitsCompleted { get; set; }
    public int VisitsSkipped { get; set; }
    public int PendingApprovals { get; set; }
}

/// <summary>One salesperson's work over a date range.</summary>
public sealed class PortalUserActivity
{
    /// <summary>Null for documents no signed-in user sent (API keys, rows from before Faz 41).</summary>
    public Guid? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public PortalMoneyLine Sales { get; set; } = new();
    public PortalMoneyLine Collections { get; set; } = new();
    public PortalMoneyLine Disbursements { get; set; } = new();
    public PortalMoneyLine Returns { get; set; } = new();
    public int VisitsCompleted { get; set; }
    public int VisitsSkipped { get; set; }
}

/// <summary>GET /api/v1/portal/activity</summary>
public sealed class PortalActivityResponse
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public List<PortalUserActivity> Users { get; set; } = [];
}

/// <summary>A planned stop and what happened at it, or a visit made outside the plan.</summary>
public sealed class PortalVisitRow
{
    public string Username { get; set; } = string.Empty;
    public string PlanId { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string StopId { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int VisitOrder { get; set; }

    /// <summary><c>PENDING</c> (planned, nothing recorded), <c>COMPLETED</c> or <c>SKIPPED</c>.</summary>
    public string Status { get; set; } = "PENDING";
    public string Note { get; set; } = string.Empty;
    public long? CompletedAt { get; set; }

    /// <summary>False for a visit recorded at a customer the day's plan did not list.</summary>
    public bool Planned { get; set; } = true;
}

/// <summary>GET /api/v1/portal/visits</summary>
public sealed class PortalVisitsResponse
{
    public string Date { get; set; } = string.Empty;
    public List<PortalVisitRow> Rows { get; set; } = [];
}

public sealed class PortalBalanceRow
{
    public string CustomerCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

/// <summary>GET /api/v1/portal/balances — customers who owe or are owed, largest first.</summary>
public sealed class PortalBalancesResponse
{
    public decimal TotalReceivable { get; set; }
    public decimal TotalPayable { get; set; }
    public List<PortalBalanceRow> Rows { get; set; } = [];
    public bool Truncated { get; set; }
}

public sealed class PortalStockRow
{
    public string StockCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
}

/// <summary>GET /api/v1/portal/stock</summary>
public sealed class PortalStockResponse
{
    public List<PortalStockRow> Rows { get; set; } = [];
    public bool Truncated { get; set; }
}
