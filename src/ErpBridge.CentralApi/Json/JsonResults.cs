using System.Text.Json;

namespace ErpBridge.CentralApi.Json;

/// <summary>
/// Centralized JSON result helpers that sidestep a <c>PipeWriter.UnflushedBytes</c>
/// incompatibility between ASP.NET Core 8 <see cref="Results.Json"/> /
/// <see cref="Results.Ok"/> and the .NET 9 runtime. See <c>Program.cs</c> for
/// the rationale. Every endpoint should serialize through <c>ToJsonResult</c>
/// rather than <see cref="Results.Ok"/> / <see cref="Results.Json"/>.
/// </summary>
public static class JsonResults
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    /// <summary>Wrap <paramref name="value"/> as an HTTP 200 JSON result.</summary>
    public static IResult Ok(object? value) =>
        Raw(200, value);

    /// <summary>Wrap <paramref name="value"/> as a JSON result with the supplied status code.</summary>
    public static IResult Status(int statusCode, object? value) =>
        Raw(statusCode, value);

    private static IResult Raw(int statusCode, object? value)
    {
        var json = value is null ? "null" : JsonSerializer.Serialize(value, Web);
        return Results.Text(json, "application/json", statusCode: statusCode);
    }
}
