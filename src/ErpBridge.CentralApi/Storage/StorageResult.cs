using System.Text.Json.Serialization;
using ErpBridge.CentralApi.Json;

namespace ErpBridge.CentralApi.Storage;

/// <summary>A file store outcome: the value, or the error an endpoint answers with.</summary>
public sealed record StorageResult<T>(T? Value, StorageError? Error)
{
    public bool Succeeded => Error is null;

    public static StorageResult<T> Ok(T value) => new(value, null);

    public static StorageResult<T> Fail(StorageError error) => new(default, error);
}

/// <summary>A storage error; <see cref="UsedBytes"/>/<see cref="QuotaBytes"/> only for the quota.</summary>
public sealed record StorageError(int Status, string Code, string Message, long? UsedBytes = null, long? QuotaBytes = null)
{
    /// <summary>The JSON answer: <c>{errorCode, message, traceId, usedBytes?, quotaBytes?}</c>.</summary>
    public IResult ToResult(HttpContext? http = null) => JsonResults.Status(Status, new StorageErrorResponse
    {
        ErrorCode = Code,
        Message = Message,
        TraceId = http?.TraceIdentifier,
        UsedBytes = UsedBytes,
        QuotaBytes = QuotaBytes,
    });
}

/// <summary>The <c>ApiError</c> shape plus the quota figures of <c>413 STORAGE_QUOTA_EXCEEDED</c>.</summary>
public sealed class StorageErrorResponse
{
    [JsonPropertyName("errorCode")] public string ErrorCode { get; set; } = string.Empty;

    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;

    [JsonPropertyName("traceId")] public string? TraceId { get; set; }

    [JsonPropertyName("usedBytes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? UsedBytes { get; set; }

    [JsonPropertyName("quotaBytes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? QuotaBytes { get; set; }
}

/// <summary>The storage error codes (docs/api-contracts.md "Merkezi dosya deposu").</summary>
public static class StorageErrors
{
    public const string QuotaExceededCode = "STORAGE_QUOTA_EXCEEDED";
    public const string UnavailableCode = "STORAGE_UNAVAILABLE";

    public static StorageError QuotaExceeded(long usedBytes, long quotaBytes) => new(StatusCodes.Status413PayloadTooLarge, QuotaExceededCode,
        "Firmanızın depolama alanı doldu. Yöneticiniz panelden alan açabilir.", usedBytes, quotaBytes);

    public static StorageError Unavailable() => new(StatusCodes.Status503ServiceUnavailable, UnavailableCode,
        "Dosya deposuna şu an ulaşılamıyor; biraz sonra yeniden deneyin.");

    public static StorageError InvalidImage() => new(StatusCodes.Status415UnsupportedMediaType, "INVALID_IMAGE",
        "Yalnız JPEG, PNG ya da WEBP görsel yüklenebilir.");

    public static StorageError FileNotFound() => new(StatusCodes.Status404NotFound, "STORED_FILE_NOT_FOUND", "Dosya bulunamadı.");

    public static StorageError TenantNotFound() => new(StatusCodes.Status404NotFound, "TENANT_NOT_FOUND", "Firma bulunamadı.");
}
