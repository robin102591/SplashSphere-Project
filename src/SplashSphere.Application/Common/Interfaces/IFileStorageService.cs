namespace SplashSphere.Application.Common.Interfaces;

/// <summary>
/// Abstraction over object storage. The production implementation targets
/// Cloudflare R2 (S3-compatible). The interface is shape-compatible with
/// AWS S3, MinIO, or any other S3-API backend, so swapping providers later
/// is a DI registration change.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads <paramref name="content"/> to the configured bucket under
    /// the given <paramref name="key"/>, overwriting any existing object.
    /// Returns the public URL the file is reachable at — the URL shape is
    /// implementation-specific (custom CDN domain, R2 public bucket, etc.).
    /// </summary>
    /// <param name="key">Object key, e.g. "tenants/abc/logo.png". Must be deterministic for a given resource so re-uploads overwrite cleanly without orphaning files.</param>
    /// <param name="content">Image bytes. Caller owns the stream and is responsible for disposal (or for passing a freshly-created stream).</param>
    /// <param name="contentType">MIME type used for the object's <c>Content-Type</c> header.</param>
    Task<string> UploadAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the object stored at <paramref name="key"/>. Idempotent —
    /// missing keys do not throw.
    /// </summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds the public URL the storage backend serves for <paramref name="key"/>.
    /// Pure helper that does not hit the network — useful when the caller
    /// already knows what the canonical key for a resource is.
    /// </summary>
    string PublicUrl(string key);
}
