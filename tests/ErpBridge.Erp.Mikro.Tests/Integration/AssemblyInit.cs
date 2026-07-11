using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = false)]

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// xUnit assembly-level hook: load <c>.env</c> from the working directory
/// (or any parent up to 6 levels) before any test runs, so the live TULPAR
/// fixture can read <c>ERPBridge_TULPAR_*</c> without the operator having to
/// <c>$env:... = ...</c> manually before every <c>dotnet test</c>.
/// <para>Env vars already set in the process are NOT overwritten — CI secrets
/// injected by the pipeline keep their priority.</para>
/// </summary>
public sealed class AssemblyInit
{
    public static void LoadDotEnvOnce()
    {
        // xUnit calls this once per test assembly before any test class runs.
        DotEnvLoader.Load();
    }
}

internal static class AssemblyInitBootstrap
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Init()
    {
        // Module initializers run before any test method in this assembly —
        // earlier than CollectionBehavior / AssemblyFixture hooks.
        DotEnvLoader.Load();
    }
}
