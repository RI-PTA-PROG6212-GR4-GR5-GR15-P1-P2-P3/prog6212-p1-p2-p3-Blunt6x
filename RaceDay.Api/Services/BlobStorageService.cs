using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
namespace RaceDay.Api.Services;

/// <summary>Stores user-supplied images in Azure Blob Storage. Only the API talks to Azure; the MVC app goes through the API.</summary>
public interface IBlobStorageService {
    /// <summary>Uploads an image into the given virtual folder (e.g. "banners") and returns its public URL.</summary>
    Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken ct = default);
    /// <summary>Best-effort delete of a blob previously returned by <see cref="UploadImageAsync"/>.</summary>
    Task DeleteByUrlAsync(string? url, CancellationToken ct = default);
}

public class BlobStorageNotConfiguredException(string message) : InvalidOperationException(message);

public class BlobStorageService : IBlobStorageService {
    private readonly BlobContainerClient? _container;
    private readonly ILogger<BlobStorageService> _log;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _containerReady;

    public BlobStorageService(IConfiguration config, ILogger<BlobStorageService> log) {
        _log = log;
        var connectionString = config["AzureBlobStorage:ConnectionString"];
        var containerName = config["AzureBlobStorage:ContainerName"] ?? "raceday-images";
        if (!string.IsNullOrWhiteSpace(connectionString)) _container = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder, CancellationToken ct = default) {
        var container = await GetContainerAsync(ct);
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var blob = container.GetBlobClient($"{folder}/{Guid.NewGuid():N}{extension}");
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType } }, ct);
        return blob.Uri.ToString();
    }

    public async Task DeleteByUrlAsync(string? url, CancellationToken ct = default) {
        if (_container is null || string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri)) return;
        try {
            // Blob URL path = /{container}/{folder}/{file} (Azurite adds the account name in front); find the container segment.
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var index = Array.IndexOf(segments, _container.Name);
            if (index < 0 || index == segments.Length - 1) return;
            var blobName = Uri.UnescapeDataString(string.Join('/', segments[(index + 1)..]));
            await _container.GetBlobClient(blobName).DeleteIfExistsAsync(cancellationToken: ct);
        } catch (RequestFailedException ex) {
            _log.LogWarning(ex, "Could not delete blob {Url}", url);
        }
    }

    private async Task<BlobContainerClient> GetContainerAsync(CancellationToken ct) {
        if (_container is null) throw new BlobStorageNotConfiguredException("Image storage is not configured. Set AzureBlobStorage:ConnectionString on the API.");
        if (_containerReady) return _container;
        await _gate.WaitAsync(ct);
        try {
            if (!_containerReady) {
                // Public read access at blob level lets the browser display images straight from the URL.
                await _container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);
                _containerReady = true;
            }
        } finally { _gate.Release(); }
        return _container;
    }
}
