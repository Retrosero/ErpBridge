using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ErpBridge.CentralApi.Tests.Support;

/// <summary>
/// Tiny test helpers for serializing objects as JSON bodies and inserting a
/// bearer token. Kept separate from <see cref="CentralApiFactory"/> so unit
/// tests that don't need a full host can still use them.
/// </summary>
internal static class HttpClientJsonExtensions
{
    private static readonly JsonSerializerOptions WebOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Serialize <paramref name="value"/> as JSON and POST it. Bearer token optional.</summary>
    public static async Task<HttpResponseMessage> PostJsonAsync(this HttpClient http, string path, object value, string? bearerToken = null)
    {
        var json = JsonSerializer.Serialize(value, WebOptions);
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        if (!string.IsNullOrEmpty(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return await http.SendAsync(request);
    }

    /// <summary>Send a GET with optional bearer token.</summary>
    public static async Task<HttpResponseMessage> GetAsync(this HttpClient http, string path, string? bearerToken = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (!string.IsNullOrEmpty(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return await http.SendAsync(request);
    }

    /// <summary>Serialize <paramref name="value"/> as JSON and PATCH it. Bearer token optional.</summary>
    public static async Task<HttpResponseMessage> PatchAsync(this HttpClient http, string path, object value, string? bearerToken = null)
    {
        var json = JsonSerializer.Serialize(value, WebOptions);
        var request = new HttpRequestMessage(HttpMethod.Patch, path)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        if (!string.IsNullOrEmpty(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return await http.SendAsync(request);
    }

    /// <summary>Send a DELETE with optional bearer token.</summary>
    public static async Task<HttpResponseMessage> DeleteAsync(this HttpClient http, string path, string? bearerToken = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, path);
        if (!string.IsNullOrEmpty(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return await http.SendAsync(request);
    }

    /// <summary>Deserialize an <c>application/json</c> response into <typeparamref name="T"/>.</summary>
    public static async Task<T> ReadAsJsonAsync<T>(this HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(body, WebOptions)
            ?? throw new InvalidOperationException("Response body was null or empty.");
    }
}
