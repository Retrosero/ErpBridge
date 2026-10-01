using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Storage;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Storage;

/// <summary>
/// The R2 client's wiring without the network (GOAL_DEPOLAMA_R2 T6): a presigned address is signed locally, so it shows
/// the endpoint, path-style bucket, region <c>auto</c> and lifetime the client was built with.
/// </summary>
public sealed class R2ObjectStoreTests
{
    private static StorageOptions Options() => new()
    {
        AccountId = "acc123",
        AccessKeyId = "AKIDTEST",
        SecretAccessKey = "secret-test",
        PublicBucket = "siparis-cepte-public",
        PrivateBucket = "siparis-cepte-private",
        PublicBaseUrl = "https://img.appsgo.cloud",
    };

    [Fact]
    public async Task A_presigned_address_points_at_the_account_endpoint_with_path_style_and_region_auto()
    {
        using var store = new R2ObjectStore(Microsoft.Extensions.Options.Options.Create(Options()));

        var url = await store.PresignGetAsync(StorageBuckets.Private, "ABCD2345/task/2026/10/x-o.jpg", TimeSpan.FromMinutes(5), default);

        url.Scheme.Should().Be("https");
        url.Host.Should().Be("acc123.r2.cloudflarestorage.com");
        url.AbsolutePath.Should().Be("/siparis-cepte-private/ABCD2345/task/2026/10/x-o.jpg");
        var query = Uri.UnescapeDataString(url.Query);
        query.Should().Contain("X-Amz-Expires=300");
        query.Should().Contain("/auto/s3/aws4_request");
        query.Should().NotContain("secret-test");
    }

    [Fact]
    public void Options_report_the_missing_settings_by_name_only()
    {
        Options().IsConfigured.Should().BeTrue();
        var partial = Options();
        partial.SecretAccessKey = " ";
        partial.PrivateBucket = string.Empty;

        partial.IsConfigured.Should().BeFalse();
        partial.MissingSettings().Should().Equal("SecretAccessKey", "PrivateBucket");
        var build = () => new R2ObjectStore(Microsoft.Extensions.Options.Options.Create(partial));
        build.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void The_same_bucket_for_public_and_private_files_is_refused()
    {
        var same = Options();
        same.PrivateBucket = " " + same.PublicBucket.ToUpperInvariant();

        same.IsConfigured.Should().BeFalse("a private receipt must never be served by the CDN of the public bucket");
        same.MissingSettings().Should().ContainSingle().Which.Should().StartWith("PrivateBucket");
    }
}
