using Bunit;
using ErpBridge.Admin.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ErpBridge.Admin.Tests.Pages;

public sealed class AdminErrorBoundaryTests : BunitContext
{
    [Fact]
    public void A_crashing_page_shows_a_support_code_and_logs_the_exception_under_it()
    {
        var logs = new Recorder();
        Services.AddSingleton<ILoggerFactory>(new LoggerFactory([logs]));
        Services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        var cut = Render<AdminErrorBoundary>(parameters => parameters.AddChildContent<Crashing>());
        cut.Find("button#crash").Click();

        var code = cut.Find("#admin-error-code").TextContent;
        code.Should().HaveLength(10);
        cut.Find("#admin-error").TextContent.Should().NotContain("kaboom");
        logs.Messages.Should().ContainSingle(m => m.Contains(code) && m.Contains("InvalidOperationException"));
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

    private sealed class Recorder : ILoggerProvider
    {
        public List<string> Messages { get; } = [];
        public ILogger CreateLogger(string categoryName) => new L(this);
        public void Dispose() { }

        private sealed class L(Recorder owner) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Error) lock (owner.Messages) owner.Messages.Add(formatter(state, exception) + " " + exception?.GetType().Name);
            }
        }
    }
}
