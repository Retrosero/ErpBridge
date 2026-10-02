using System.Net;
using ErpBridge.CentralApi.Storage;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErpBridge.CentralApi.Tests.Storage;

/// <summary>
/// GOAL_DEPOLAMA_R2 S7 (SSRF): the server's outside downloads refuse local and private targets before any connection,
/// follow redirects themselves (each target checked again, at most three), keep to the byte limit while streaming and
/// send the last ETag back. The handler here stands in for the network; the real one also pins each connection to a
/// checked address (<see cref="SafeHttpFetcher.CreateHandler"/>).
/// </summary>
public sealed class SafeHttpFetcherTests
{
    [Theory]
    [InlineData("http://127.0.0.1/feed.xml")]
    [InlineData("http://10.0.0.1/feed.xml")]
    [InlineData("http://192.168.1.20:8080/feed.xml")]
    [InlineData("http://169.254.169.254/latest/meta-data/")]
    [InlineData("http://100.64.0.1/x.jpg")]
    [InlineData("http://0.0.0.0/x.jpg")]
    [InlineData("http://2130706433/x.jpg")]
    [InlineData("http://[::1]/x.jpg")]
    [InlineData("http://[::ffff:10.0.0.1]/x.jpg")]
    [InlineData("http://[::ffff:127.0.0.1]/x.jpg")]
    [InlineData("http://[64:ff9b::a00:1]/x.jpg")]
    [InlineData("http://[fe80::1]/x.jpg")]
    [InlineData("http://[fd00::1]/x.jpg")]
    [InlineData("http://localhost/x.jpg")]
    [InlineData("http://img.localhost/x.jpg")]
    [InlineData("http://printer.local/x.jpg")]
    [InlineData("ftp://cdn.example.com/x.jpg")]
    [InlineData("file:///etc/passwd")]
    [InlineData("https://user:secret@cdn.example.com/x.jpg")]
    public async Task Local_private_and_odd_addresses_are_refused_without_a_request(string url)
    {
        SafeHttpFetcher.CheckAddress(new Uri(url)).Should().NotBeNull();
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var result = await Fetcher(handler).GetAsync(new Uri(url), FetchRequest.Image(), new MemoryStream(), CancellationToken.None);
        result.Status.Should().Be(FetchStatus.Failed);
        handler.Requests.Should().BeEmpty();
    }

    [Theory]
    [InlineData("https://cdn.example.com/a.jpg")]
    [InlineData("http://8.8.8.8:8080/a.jpg")]
    [InlineData("http://[2606:4700:4700::1111]/a.jpg")]
    public void Public_addresses_may_be_asked(string url) => SafeHttpFetcher.CheckAddress(new Uri(url)).Should().BeNull();

    [Fact]
    public async Task A_redirect_to_a_private_address_is_refused_without_following_it()
    {
        var handler = new StubHandler(_ => Redirect("http://10.0.0.5/secret.jpg"));
        var result = await Fetcher(handler).GetAsync(new Uri("https://cdn.example.com/a.jpg"), FetchRequest.Image(), new MemoryStream(), CancellationToken.None);
        result.Status.Should().Be(FetchStatus.Failed);
        handler.Requests.Select(r => r.Host).Should().Equal("cdn.example.com");
    }

    [Fact]
    public async Task Redirects_are_followed_three_times_at_most()
    {
        var hops = new StubHandler(r => r.AbsolutePath switch
        {
            "/0" => Redirect("/1"),
            "/1" => Redirect("https://mirror.example.com/2"),
            "/2" => Redirect("/3"),
            _ => Body([1, 2, 3]),
        });
        var body = new MemoryStream();
        var ok = await Fetcher(hops).GetAsync(new Uri("https://cdn.example.com/0"), FetchRequest.Image(), body, CancellationToken.None);
        ok.Status.Should().Be(FetchStatus.Ok);
        body.ToArray().Should().Equal(1, 2, 3);
        hops.Requests.Select(r => r.ToString()).Should().Equal(
            "https://cdn.example.com/0", "https://cdn.example.com/1", "https://mirror.example.com/2", "https://mirror.example.com/3");

        var endless = new StubHandler(r => Redirect("/x" + r.AbsolutePath.Length));
        var refused = await Fetcher(endless).GetAsync(new Uri("https://cdn.example.com/0"), FetchRequest.Image(), new MemoryStream(), CancellationToken.None);
        refused.Status.Should().Be(FetchStatus.Failed);
        refused.Reason.Should().Be("too many redirects");
        endless.Requests.Should().HaveCount(SafeHttpFetcher.MaxRedirects + 1);
    }

    [Fact]
    public async Task The_byte_limit_holds_with_and_without_a_declared_length()
    {
        var limit = new FetchRequest(10, TimeSpan.FromSeconds(5));
        var declared = await Fetcher(new StubHandler(_ => Body(new byte[11]))).GetAsync(new Uri("https://cdn.example.com/a"), limit, new MemoryStream(), CancellationToken.None);
        declared.Status.Should().Be(FetchStatus.TooLarge);

        var streamed = await Fetcher(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new NoLengthStream(new byte[11])),
        })).GetAsync(new Uri("https://cdn.example.com/a"), limit, new MemoryStream(), CancellationToken.None);
        streamed.Status.Should().Be(FetchStatus.TooLarge);

        var fits = await Fetcher(new StubHandler(_ => Body(new byte[10]))).GetAsync(new Uri("https://cdn.example.com/a"), limit, new MemoryStream(), CancellationToken.None);
        fits.Status.Should().Be(FetchStatus.Ok);
        fits.Bytes.Should().Be(10);
    }

    [Fact]
    public async Task The_last_etag_and_date_are_sent_and_304_and_404_are_told_apart()
    {
        HttpRequestMessage? seen = null;
        var notModified = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NotModified)) { OnRequest = r => seen = r };
        var result = await Fetcher(notModified).GetAsync(new Uri("https://cdn.example.com/a"), FetchRequest.Image("\"v1\"", "Thu, 01 Oct 2026 10:00:00 GMT"),
            new MemoryStream(), CancellationToken.None);
        result.Should().Match<FetchResult>(r => r.Status == FetchStatus.NotModified && r.ETag == "\"v1\"");
        seen!.Headers.IfNoneMatch.Single().Tag.Should().Be("\"v1\"");
        seen.Headers.IfModifiedSince.Should().Be(new DateTimeOffset(2026, 10, 1, 10, 0, 0, TimeSpan.Zero));

        (await Fetcher(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound))).GetAsync(new Uri("https://cdn.example.com/a"), FetchRequest.Image(),
            new MemoryStream(), CancellationToken.None)).Status.Should().Be(FetchStatus.NotFound);
        var error = await Fetcher(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError))).GetAsync(new Uri("https://cdn.example.com/a"),
            FetchRequest.Image(), new MemoryStream(), CancellationToken.None);
        error.Should().Match<FetchResult>(r => r.Status == FetchStatus.Failed && r.HttpStatus == 500);

        var tagged = Body([7]);
        tagged.Headers.ETag = new System.Net.Http.Headers.EntityTagHeaderValue("\"v2\"");
        tagged.Content.Headers.LastModified = new DateTimeOffset(2026, 10, 2, 8, 0, 0, TimeSpan.Zero);
        var fresh = await Fetcher(new StubHandler(_ => tagged)).GetAsync(new Uri("https://cdn.example.com/a"), FetchRequest.Image(), new MemoryStream(), CancellationToken.None);
        fresh.Should().Match<FetchResult>(r => r.ETag == "\"v2\"" && r.LastModified == "Fri, 02 Oct 2026 08:00:00 GMT");
    }

    [Fact]
    public async Task A_slow_source_times_out()
    {
        var slow = new StubHandler(_ => Body([1])) { Delay = TimeSpan.FromSeconds(5) };
        var result = await Fetcher(slow).GetAsync(new Uri("https://cdn.example.com/a"), new FetchRequest(10, TimeSpan.FromMilliseconds(100)), new MemoryStream(), CancellationToken.None);
        result.Should().Match<FetchResult>(r => r.Status == FetchStatus.Failed && r.Reason == "timeout");
    }

    [Fact]
    public void The_real_handler_pins_connections_and_follows_no_redirect()
    {
        using var handler = SafeHttpFetcher.CreateHandler();
        handler.UseProxy.Should().BeFalse();
        handler.AllowAutoRedirect.Should().BeFalse();
        handler.ConnectCallback.Should().NotBeNull();
    }

    private static SafeHttpFetcher Fetcher(HttpMessageHandler handler) => new(new HttpClient(handler), NullLogger<SafeHttpFetcher>.Instance);

    private static HttpResponseMessage Redirect(string location) =>
        new(HttpStatusCode.Found) { Headers = { Location = new Uri(location, UriKind.RelativeOrAbsolute) } };

    private static HttpResponseMessage Body(byte[] data) => new(HttpStatusCode.OK) { Content = new ByteArrayContent(data) };

    private sealed class StubHandler(Func<Uri, HttpResponseMessage> answer) : HttpMessageHandler
    {
        public List<Uri> Requests { get; } = [];

        public Action<HttpRequestMessage>? OnRequest { get; init; }

        public TimeSpan Delay { get; init; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!);
            OnRequest?.Invoke(request);
            if (Delay > TimeSpan.Zero) await Task.Delay(Delay, cancellationToken);
            return answer(request.RequestUri!);
        }
    }

    /// <summary>A body whose length is not known up front (chunked).</summary>
    private sealed class NoLengthStream(byte[] data) : MemoryStream(data)
    {
        public override bool CanSeek => false;
    }
}
