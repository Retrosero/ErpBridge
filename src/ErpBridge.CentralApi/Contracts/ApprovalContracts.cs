using System.Text.Json;
using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

/// <summary>An approval request as a list shows it.</summary>
public sealed class ApprovalRequestDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("externalId")] public string ExternalId { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("counterpartyName")] public string CounterpartyName { get; set; } = string.Empty;
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("summary")] public JsonElement Summary { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("replacesRequestId")] public Guid? ReplacesRequestId { get; set; }
    [JsonPropertyName("requestedByUserId")] public Guid? RequestedByUserId { get; set; }
    [JsonPropertyName("requestedByName")] public string? RequestedByName { get; set; }
    [JsonPropertyName("requestedAtUtc")] public DateTimeOffset RequestedAtUtc { get; set; }
    [JsonPropertyName("updatedSeq")] public long UpdatedSeq { get; set; }
    [JsonPropertyName("decidedByName")] public string? DecidedByName { get; set; }
    [JsonPropertyName("decidedAtUtc")] public DateTimeOffset? DecidedAtUtc { get; set; }
    [JsonPropertyName("decisionNote")] public string? DecisionNote { get; set; }
}

/// <summary>One request with what it would post, its history and stock warnings.</summary>
public sealed class ApprovalRequestDetailDto
{
    [JsonPropertyName("request")] public ApprovalRequestDto Request { get; set; } = new();
    [JsonPropertyName("documents")] public JsonElement Documents { get; set; }
    [JsonPropertyName("events")] public ApprovalEventDto[] Events { get; set; } = Array.Empty<ApprovalEventDto>();

    /// <summary>Lines whose quantity exceeds the stock on hand. A warning; approval stays possible.</summary>
    [JsonPropertyName("warnings")] public StockWarningDto[] Warnings { get; set; } = Array.Empty<StockWarningDto>();
}

/// <summary>A product a request would sell more of than is in stock.</summary>
public sealed class StockWarningDto
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("requested")] public decimal Requested { get; set; }
    [JsonPropertyName("onHand")] public decimal OnHand { get; set; }
}

public sealed class ApprovalEventDto
{
    [JsonPropertyName("action")] public string Action { get; set; } = string.Empty;
    [JsonPropertyName("byName")] public string? ByName { get; set; }
    [JsonPropertyName("atUtc")] public DateTimeOffset AtUtc { get; set; }
    [JsonPropertyName("note")] public string? Note { get; set; }
}

/// <summary>Approve / reject / reopen / withdraw body.</summary>
public sealed class ApprovalDecisionRequest
{
    [JsonPropertyName("note")] public string? Note { get; set; }
}

/// <summary>What a phone needs to decide whether to refresh and notify.</summary>
public sealed class ApprovalSummaryDto
{
    /// <summary>Pending requests the caller can see: all of them for an approver, their own otherwise.</summary>
    [JsonPropertyName("pendingCount")] public int PendingCount { get; set; }

    /// <summary>Highest <c>updatedSeq</c> among the requests the caller can see; 0 when there are none.</summary>
    [JsonPropertyName("latestUpdatedSeq")] public long LatestUpdatedSeq { get; set; }

    [JsonPropertyName("canApprove")] public bool CanApprove { get; set; }
}

/// <summary>The company's approval rules.</summary>
public sealed class ApprovalRulesDto
{
    [JsonPropertyName("rules")] public Dictionary<string, bool> Rules { get; set; } = new();
    [JsonPropertyName("canManage")] public bool CanManage { get; set; }
    [JsonPropertyName("updatedByName")] public string? UpdatedByName { get; set; }
    [JsonPropertyName("updatedAtUtc")] public DateTimeOffset? UpdatedAtUtc { get; set; }
}

/// <summary>PUT rules body; kinds left out keep their value.</summary>
public sealed class UpdateApprovalRulesRequest
{
    [JsonPropertyName("rules")] public Dictionary<string, bool>? Rules { get; set; }
}
