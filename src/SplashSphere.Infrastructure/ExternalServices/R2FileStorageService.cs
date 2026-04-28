using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using SplashSphere.Application.Common.Interfaces;

namespace SplashSphere.Infrastructure.ExternalServices;

/// <summary>
/// Cloudflare R2 implementation of <see cref="IFileStorageService"/>.
/// Built on the AWS S3 SDK against R2's S3-compatible endpoint
/// (<c>https://{accountId}.r2.cloudflarestorage.com</c>). Public URLs are
/// composed from <see cref="R2Options.PublicBaseUrl"/> — typically a
/// custom domain bound to the bucket, or the auto-generated
/// <c>https://pub-xxxxxxxx.r2.dev</c> URL.
/// </summary>
/// <remarks>
/// Until real R2 credentials are populated in <c>appsettings</c>,
/// <see cref="UploadAsync"/> will fail with the SDK's invalid-credentials
/// error at runtime. The build, DI registration, and command pipeline
/// all work — only the network call to R2 fails. Swap the placeholder
/// values once a bucket is provisioned.
/// </remarks>
public sealed class R2FileStorageService(
    IAmazonS3 s3Client,
    R2Options options,
    ILogger<R2FileStorageService> logger)
    : IFileStorageService
{
    public async Task<string> UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName  = options.BucketName,
            Key         = key,
            InputStream = content,
            ContentType = contentType,
            // R2 does not honor the standard S3 "public-read" ACL — bucket
            // public-access is configured at the bucket level via Cloudflare
            // dashboard or by binding a custom domain. Setting a CannedACL
            // here would trigger an X-Amz-Acl unsupported error.
            DisablePayloadSigning = true,
        };

        await s3Client.PutObjectAsync(request, cancellationToken);

        var publicUrl = PublicUrl(key);
        logger.LogInformation("Uploaded {Key} to R2 bucket {Bucket}", key, options.BucketName);
        return publicUrl;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = options.BucketName,
                Key        = key,
            }, cancellationToken);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Idempotent delete — missing key is not an error.
        }
    }

    public string PublicUrl(string key)
        => $"{options.PublicBaseUrl.TrimEnd('/')}/{key}";
}

/// <summary>
/// Strongly-typed configuration for the R2 backend. Bound from the
/// <c>Cloudflare:R2</c> section of <c>appsettings.json</c>.
/// </summary>
public sealed class R2Options
{
    public const string SectionName = "Cloudflare:R2";

    public string AccountId       { get; set; } = string.Empty;
    public string AccessKeyId     { get; set; } = string.Empty;
    public string SecretAccessKey { get; set; } = string.Empty;
    public string BucketName      { get; set; } = string.Empty;

    /// <summary>
    /// Origin used to build public URLs returned to clients. Either a custom
    /// domain bound to the bucket (<c>https://assets.example.com</c>) or the
    /// generated <c>https://pub-xxxxxxxx.r2.dev</c> URL.
    /// </summary>
    public string PublicBaseUrl   { get; set; } = string.Empty;

    /// <summary>S3-compatible endpoint Cloudflare exposes for this account.</summary>
    public string ServiceUrl
        => $"https://{AccountId}.r2.cloudflarestorage.com";
}
