using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Polly;
using Polly.Timeout;
using Polly.CircuitBreaker;

var assetsDir = Path.Combine(Directory.GetCurrentDirectory(), "StaticAssets");
assetsDir = Path.GetFullPath(assetsDir);

Console.WriteLine($"Static assets folder: {assetsDir}");

if (!Directory.Exists(assetsDir))
{
    Console.WriteLine("StaticAssets directory not found. Create it and add files to upload.");
    return;
}

// Connection string: prefer env var, otherwise assume Azurite default
var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING")
    ?? "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFe...==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";

var blobServiceClient = new BlobServiceClient(connectionString);
var containerName = "static-assets";
var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

// Polly policies
var retryPolicy = Policy
    .Handle<Exception>()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2), onRetry: (ex, span, retryCount, ctx) =>
    {
        Console.WriteLine($"Retry {retryCount} after {span.TotalSeconds}s due to: {ex.Message}");
    });

var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(10), TimeoutStrategy.Pessimistic);

// Apply per-attempt timeout and outer retry so each attempt is bounded
var finalPolicy = Policy.WrapAsync(retryPolicy, timeoutPolicy);

await finalPolicy.ExecuteAsync(async () =>
{
    await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
    Console.WriteLine($"Container '{containerName}' ready.");

    foreach (var file in Directory.GetFiles(assetsDir))
    {
        var fileName = Path.GetFileName(file);
        var blobClient = containerClient.GetBlobClient(fileName);
        using var stream = File.OpenRead(file);
        Console.WriteLine($"Uploading {fileName}...");
        await blobClient.UploadAsync(stream, overwrite: true);
        Console.WriteLine($"Uploaded {fileName} -> {blobClient.Uri}");
    }
});

Console.WriteLine("All assets uploaded.");
