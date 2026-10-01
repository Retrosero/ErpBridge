using System.Net;
using System.Runtime.CompilerServices;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using ErpBridge.CentralApi.Domain;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Storage;

/// <summary>
/// Cloudflare R2 through its S3 API (GOAL_DEPOLAMA_R2 T6, <c>AWSSDK.S3</c>): endpoint
/// <c>https://{AccountId}.r2.cloudflarestorage.com</c>, path-style addresses, signing region <c>auto</c>. Checksums are
/// computed only when an operation requires them and payload signing is off for uploads — R2 does not take the SDK's
/// newer streaming checksum trailers. The SDK's own request/response logging stays off; the key pair is held by the
/// client only and never written anywhere.
/// </summary>
public sealed class R2ObjectStore : IObjectStore, IDisposable
{
    private readonly AmazonS3Client _client;
    private readonly StorageOptions _options;

    public R2ObjectStore(IOptions<StorageOptions> options)
    {
        _options = options.Value;
        if (!_options.IsConfigured) throw new InvalidOperationException("R2ObjectStore needs every Storage:* connection setting.");

        // Process-wide switches of the SDK: nothing it does is logged (requests carry signed headers).
        AWSConfigs.LoggingConfig.LogTo = LoggingOptions.None;
        AWSConfigs.LoggingConfig.LogResponses = ResponseLoggingOption.Never;
        AWSConfigs.LoggingConfig.LogMetrics = false;

        var config = new AmazonS3Config
        {
            ServiceURL = $"https://{_options.AccountId.Trim()}.r2.cloudflarestorage.com",
            ForcePathStyle = true,
            AuthenticationRegion = "auto",
            RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED,
            ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,
            Timeout = TimeSpan.FromSeconds(30),
            MaxErrorRetry = 2,
        };
        _client = new AmazonS3Client(new BasicAWSCredentials(_options.AccessKeyId.Trim(), _options.SecretAccessKey.Trim()), config);
    }

    public bool IsAvailable => true;

    public async Task PutAsync(string bucket, string key, byte[] data, string contentType, CancellationToken ct)
    {
        using var body = new MemoryStream(data, writable: false);
        await Call(() => _client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = BucketName(bucket),
            Key = key,
            InputStream = body,
            AutoCloseStream = false,
            ContentType = contentType,
            DisablePayloadSigning = true,
            DisableDefaultChecksumValidation = true,
        }, ct)).ConfigureAwait(false);
    }

    public async Task<StoredObject?> GetAsync(string bucket, string key, CancellationToken ct)
    {
        try
        {
            var response = await _client.GetObjectAsync(new GetObjectRequest { BucketName = BucketName(bucket), Key = key }, ct).ConfigureAwait(false);
            return new StoredObject(response.ResponseStream, response.ContentLength, response.Headers.ContentType);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex) when (ex is AmazonServiceException or AmazonClientException or HttpRequestException)
        {
            throw Unavailable(ex);
        }
    }

    public Task DeleteAsync(string bucket, string key, CancellationToken ct) =>
        Call(() => _client.DeleteObjectAsync(new DeleteObjectRequest { BucketName = BucketName(bucket), Key = key }, ct));

    public async Task<StoredObjectInfo?> HeadAsync(string bucket, string key, CancellationToken ct)
    {
        try
        {
            var response = await _client.GetObjectMetadataAsync(new GetObjectMetadataRequest { BucketName = BucketName(bucket), Key = key }, ct).ConfigureAwait(false);
            return new StoredObjectInfo(key, response.ContentLength, response.Headers.ContentType, ToOffset(response.LastModified));
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex) when (ex is AmazonServiceException or AmazonClientException or HttpRequestException)
        {
            throw Unavailable(ex);
        }
    }

    public async IAsyncEnumerable<StoredObjectInfo> ListAsync(string bucket, string prefix, [EnumeratorCancellation] CancellationToken ct)
    {
        string? continuation = null;
        do
        {
            ListObjectsV2Response page;
            try
            {
                page = await _client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = BucketName(bucket),
                    Prefix = prefix,
                    ContinuationToken = continuation,
                    MaxKeys = 1000,
                }, ct).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is AmazonServiceException or AmazonClientException or HttpRequestException)
            {
                throw Unavailable(ex);
            }
            foreach (var item in page.S3Objects ?? [])
                yield return new StoredObjectInfo(item.Key, item.Size ?? 0, null, ToOffset(item.LastModified));
            continuation = page.IsTruncated == true ? page.NextContinuationToken : null;
        }
        while (continuation is not null);
    }

    public async Task<Uri> PresignGetAsync(string bucket, string key, TimeSpan validFor, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        // Signed locally: no request leaves the server.
        var url = await _client.GetPreSignedURLAsync(new GetPreSignedUrlRequest
        {
            BucketName = BucketName(bucket),
            Key = key,
            Verb = HttpVerb.GET,
            Protocol = Protocol.HTTPS,
            Expires = DateTime.UtcNow.Add(validFor),
        }).ConfigureAwait(false);
        return new Uri(url);
    }

    public void Dispose() => _client.Dispose();

    private string BucketName(string bucket) => bucket switch
    {
        StorageBuckets.Public => _options.PublicBucket.Trim(),
        StorageBuckets.Private => _options.PrivateBucket.Trim(),
        _ => throw new ArgumentOutOfRangeException(nameof(bucket), bucket, "Unknown storage bucket."),
    };

    private static async Task Call<T>(Func<Task<T>> call)
    {
        try
        {
            await call().ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is AmazonServiceException or AmazonClientException or HttpRequestException)
        {
            throw Unavailable(ex);
        }
    }

    /// <summary>
    /// The SDK's message only (its status and code): the exception object carries the request with signed headers,
    /// so it is not passed on as an inner exception.
    /// </summary>
    private static StorageUnavailableException Unavailable(Exception ex) =>
        new($"R2 request failed: {ex.GetType().Name} {(ex as AmazonServiceException)?.StatusCode} {(ex as AmazonServiceException)?.ErrorCode}".TrimEnd());

    private static DateTimeOffset? ToOffset(DateTime? value) =>
        value is { } time ? new DateTimeOffset(DateTime.SpecifyKind(time, time.Kind == DateTimeKind.Unspecified ? DateTimeKind.Utc : time.Kind)) : null;
}
