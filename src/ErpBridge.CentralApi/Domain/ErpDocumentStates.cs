using System.Globalization;

namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Where a job's document stands in the ERP, in the words the phone and the portal show (goal ERP yazım Y4d, Y5a).
/// </summary>
public static class ErpDocumentStates
{
    /// <summary>Queued or being written.</summary>
    public const string Pending = "pending";

    /// <summary>The ERP was unreachable; the job is taken again at its next attempt time.</summary>
    public const string Retrying = "retrying";

    public const string Written = "written";

    /// <summary>Will not be tried again unless an administrator retries it.</summary>
    public const string Failed = "failed";

    public static readonly IReadOnlyList<string> All = [Pending, Retrying, Written, Failed];

    public static string Of(JobStatus status, long? nextAttemptAtMs, long nowMs) => status switch
    {
        JobStatus.Succeeded => Written,
        JobStatus.Failed or JobStatus.DeadLetter => Failed,
        JobStatus.Pending when nextAttemptAtMs is { } next && next > nowMs => Retrying,
        _ => Pending,
    };

    /// <summary>The job statuses a state can come from, for filtering in SQL.</summary>
    public static JobStatus[] Statuses(string state) => state switch
    {
        Written => [JobStatus.Succeeded],
        Failed => [JobStatus.Failed, JobStatus.DeadLetter],
        Retrying => [JobStatus.Pending],
        _ => [JobStatus.Pending, JobStatus.Processing],
    };

    /// <summary><c>series-number</c>, the number alone without a series, null before the ERP numbered it.</summary>
    public static string? DocumentNo(string? series, int? number) => number switch
    {
        null => null,
        _ when string.IsNullOrWhiteSpace(series) => number.Value.ToString(CultureInfo.InvariantCulture),
        _ => $"{series.Trim()}-{number.Value.ToString(CultureInfo.InvariantCulture)}",
    };
}
