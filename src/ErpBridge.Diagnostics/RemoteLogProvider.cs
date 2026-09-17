using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Diagnostics;

public sealed class RemoteLogOptions : BufferedLogOptions
{
    public required Uri Endpoint { get; init; }
    public required string Key { get; init; }

    /// <summary><c>portal</c> or <c>admin</c> — the CentralApi accepts no other source on this route.</summary>
    public required string Source { get; init; }

    public string? AppVersion { get; init; }
}

/// <summary>
/// Log Merkezi L2d: ships the Portal's and the Admin console's warning+ log lines to the CentralApi
/// (<c>POST /api/v1/internal/logs</c>, <see cref="InternalLogContract.KeyHeader"/>). Uses its own
/// <see cref="HttpClient"/>, not the host's factory, so its traffic never produces log lines of its own.
/// </summary>
public sealed class RemoteLogProvider : BufferedLogProvider
{
    private readonly RemoteLogOptions _options;
    private readonly HttpClient _http;

    public RemoteLogProvider(RemoteLogOptions options) : this(options, new SocketsHttpHandler { PooledConnectionLifetime = TimeSpan.FromMinutes(5) }) { }

    public RemoteLogProvider(RemoteLogOptions options, HttpMessageHandler handler) : base(options)
    {
        _options = options;
        _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
    }

    protected override async Task SendAsync(IReadOnlyList<ShippedLog> batch, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
        {
            Content = JsonContent.Create(new InternalLogBatch
            {
                Source = _options.Source,
                Events = batch.Select(line => new InternalLogEvent
                {
                    EventId = line.EventId,
                    OccurredAtUtc = line.OccurredAtUtc,
                    Severity = line.Severity,
                    Category = line.Category,
                    Message = line.Message,
                    ExceptionType = line.ExceptionType,
                    StackTrace = line.StackTrace,
                    AppVersion = _options.AppVersion,
                    Properties = line.Properties,
                }).ToList(),
            }),
        };
        request.Headers.Add(InternalLogContract.KeyHeader, _options.Key);
        using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}

/// <summary>Wire format of <c>POST /api/v1/internal/logs</c>, shared by the shipper and the CentralApi.</summary>
public static class InternalLogContract
{
    public const string Route = "/api/v1/internal/logs";
    public const string KeyHeader = "X-Internal-Log-Key";
    public const string KeyConfig = "Logs:InternalIngestKey";

    /// <summary>A shorter key is treated as not configured (both ends refuse to use it).</summary>
    public const int MinKeyLength = 32;
    public const int MaxBatch = 200;
}

public sealed class InternalLogBatch
{
    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;
    [JsonPropertyName("events")] public List<InternalLogEvent> Events { get; set; } = [];
}

public sealed class InternalLogEvent
{
    [JsonPropertyName("eventId")] public string EventId { get; set; } = string.Empty;
    [JsonPropertyName("occurredAtUtc")] public DateTimeOffset? OccurredAtUtc { get; set; }
    [JsonPropertyName("severity")] public string? Severity { get; set; }
    [JsonPropertyName("category")] public string? Category { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("exceptionType")] public string? ExceptionType { get; set; }
    [JsonPropertyName("stackTrace")] public string? StackTrace { get; set; }
    [JsonPropertyName("appVersion")] public string? AppVersion { get; set; }
    [JsonPropertyName("properties")] public IReadOnlyDictionary<string, string>? Properties { get; set; }
}

public static class RemoteLogShippingExtensions
{
    /// <summary>
    /// Ships warning+ lines (<c>Logs:MinimumLevel</c> to change) to the CentralApi at <c>CentralApi:BaseUrl</c> when
    /// <c>Logs:InternalIngestKey</c> is set. Without a key nothing changes: logs stay on the console (plan D6).
    /// </summary>
    public static ILoggingBuilder AddRemoteLogShipping(this ILoggingBuilder logging, IConfiguration configuration, string source)
    {
        var key = configuration[InternalLogContract.KeyConfig];
        if (string.IsNullOrWhiteSpace(key) || key.Length < InternalLogContract.MinKeyLength) return logging;
        if (!Uri.TryCreate(configuration["CentralApi:BaseUrl"], UriKind.Absolute, out var baseUrl)) return logging;

        var minimum = Enum.TryParse<LogLevel>(configuration["Logs:MinimumLevel"], ignoreCase: true, out var level) ? level : LogLevel.Warning;
        logging.AddProvider(new RemoteLogProvider(new RemoteLogOptions
        {
            Endpoint = new Uri(baseUrl, InternalLogContract.Route),
            Key = key,
            Source = source,
            MinimumLevel = minimum,
            AppVersion = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString(),
            // HttpClient factory logs every call at Information; nothing below Warning ships anyway, but a failing
            // request to the log target itself must not be shipped to the log target.
            ExcludedCategoryPrefixes = ["System.Net.Http.HttpClient"],
        }));
        return logging;
    }
}
