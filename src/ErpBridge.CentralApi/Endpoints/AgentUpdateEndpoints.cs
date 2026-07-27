namespace ErpBridge.CentralApi.Endpoints;

/// <summary>Serves the signed Windows agent release from the license-server
/// volume. The manifest is public because its signature is verified locally
/// by the agent before any binary is accepted.</summary>
public static class AgentUpdateEndpoints
{
    public static IEndpointRouteBuilder MapAgentUpdateEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/v1/updates/windows/manifest", (IConfiguration configuration, IWebHostEnvironment environment) =>
        {
            var path = ManifestPath(configuration, environment);
            return File.Exists(path)
                ? Results.File(path, "application/json", enableRangeProcessing: false)
                : Results.NotFound();
        }).AllowAnonymous();

        routes.MapGet("/api/v1/updates/windows/package/{fileName}", (string fileName, IConfiguration configuration, IWebHostEnvironment environment) =>
        {
            if (!string.Equals(fileName, Path.GetFileName(fileName), StringComparison.Ordinal)
                || !fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) return Results.BadRequest();
            var directory = UpdateDirectory(configuration, environment);
            var path = Path.Combine(directory, fileName);
            return File.Exists(path)
                ? Results.File(path, "application/octet-stream", fileDownloadName: fileName, enableRangeProcessing: true)
                : Results.NotFound();
        }).AllowAnonymous();

        return routes;
    }

    private static string ManifestPath(IConfiguration configuration, IWebHostEnvironment environment) =>
        Path.Combine(UpdateDirectory(configuration, environment), "agent-update.json");

    private static string UpdateDirectory(IConfiguration configuration, IWebHostEnvironment environment) =>
        configuration["Updates:Directory"] ?? Path.Combine(environment.ContentRootPath, "updates");
}
