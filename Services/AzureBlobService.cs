using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace CoffeeShopAPI.Services;

public interface IAzureBlobService
{
    Task<string> UploadFileAsync(IFormFile file);
    Task<BlobDownloadInfo> GetBlobAsync(string blobName);
}

public class AzureBlobService : IAzureBlobService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobService(string connectionString, string containerName)
    {
        _containerClient = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        // Ensure the container exists before uploading (do this lazily to avoid startup-time failures)
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

        var ext = Path.GetExtension(file.FileName);
        var blobName = $"coffee-{Guid.NewGuid()}{ext}";
        var blobClient = _containerClient.GetBlobClient(blobName);

        var headers = new BlobHttpHeaders { ContentType = file.ContentType };

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, headers);

        return blobName;
    }

    public async Task<BlobDownloadInfo> GetBlobAsync(string blobName)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        var resp = await blobClient.DownloadAsync();
        return resp.Value;
    }
}
