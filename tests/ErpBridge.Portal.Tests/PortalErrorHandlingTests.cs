using System.Text;
using Bunit;
using ErpBridge.Portal.Session;
using ErpBridge.Portal.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>Log Merkezi L2e: an exception on a page shows a support code and is logged under it.</summary>
public sealed class PortalErrorHandlingTests : PortalPageTestContext
{
    [Fact]
    public void A_crashing_page_shows_a_support_code_and_logs_it_with_company_and_user()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var logs = new CapturingLoggerProvider();
        Services.AddSingleton<ILoggerFactory>(new LoggerFactory([logs]));
        Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        PortalTestSetup.Register(this, PortalTestSetup.State(token: FakeJwt(tenantId, userId)) with { ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(1) });

        var cut = Render<PortalErrorBoundary>(parameters => parameters.AddChildContent<Crashing>());
        cut.Find("button#crash").Click();

        var code = cut.Find("#portal-error-code").TextContent;
        code.Should().HaveLength(10);
        cut.Find("#portal-error").TextContent.Should().Contain("Beklenmeyen bir hata oluştu").And.NotContain("kaboom");
        var entry = logs.Entries.Should().ContainSingle(e => e.Level == LogLevel.Error).Subject;
        entry.Exception.Should().BeOfType<InvalidOperationException>();
        entry.Scope.Should().Contain("CorrelationId", code).And.Contain("TenantId", tenantId).And.Contain("UserId", userId).And.Contain("Kind", "PORTAL_UNHANDLED");
    }

    [Fact]
    public void Token_claims_are_read_for_log_context_and_garbage_is_ignored()
    {
        var tenantId = Guid.NewGuid();
        var session = new PortalSession(TimeProvider.System);
        session.SignIn(PortalTestSetup.State(token: FakeJwt(tenantId, Guid.Empty)) with { ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(1) });
        session.TenantId.Should().Be(tenantId);

        var broken = new PortalSession(TimeProvider.System);
        broken.SignIn(PortalTestSetup.State(token: "not.a.jwt!") with { ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(1) });
        broken.TenantId.Should().BeNull();
        broken.UserId.Should().BeNull();
    }

    private static string FakeJwt(Guid tenantId, Guid userId)
    {
        static string Part(string json) => Convert.ToBase64String(Encoding.UTF8.GetBytes(json)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return $"{Part("{\"alg\":\"HS256\"}")}.{Part($"{{\"tenant\":\"{tenantId}\",\"sub\":\"{userId}\"}}")}.sig";
    }

    private sealed class Crashing : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "id", "crash");
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, () => throw new InvalidOperationException("kaboom")));
            builder.CloseElement();
        }
    }
}

/// <summary>Records log entries with the scope values active when each was written.</summary>
public sealed class CapturingLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private IExternalScopeProvider _scopes = new LoggerExternalScopeProvider();

    public List<(LogLevel Level, string Message, Exception? Exception, Dictionary<string, object?> Scope)> Entries { get; } = [];

    public ILogger CreateLogger(string categoryName) => new Logger(this);

    public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopes = scopeProvider;

    public void Dispose() { }

    private sealed class Logger(CapturingLoggerProvider owner) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => owner._scopes.Push(state);

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var scope = new Dictionary<string, object?>();
            owner._scopes.ForEachScope((value, target) =>
            {
                if (value is IEnumerable<KeyValuePair<string, object?>> pairs)
                    foreach (var (key, item) in pairs) target[key] = item;
            }, scope);
            lock (owner.Entries) owner.Entries.Add((logLevel, formatter(state, exception), exception, scope));
        }
    }
}
