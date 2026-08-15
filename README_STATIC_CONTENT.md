# Static content hosting (Azurite) – Local setup and uploader

This folder contains a small console uploader that demonstrates the Static Content Hosting pattern using Azurite (local Azure Blob emulator).

Prerequisites
- .NET 8 SDK
- Azurite (npm): `npm install -g azurite` or run container `mcr.microsoft.com/azure-storage/azurite`

Run Azurite locally (example):

```bash
# via npm-installed azurite
azurite --silent --location ./azurite --debug ./azurite/debug

# or using Docker
docker run -p 10000:10000 -p 10001:10001 mcr.microsoft.com/azure-storage/azurite
```

Default Azurite connection string (used by the uploader when `AZURE_STORAGE_CONNECTION_STRING` is not set):

```
DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFe...==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;
```

Usage
1. Add files into the `StaticAssets/` folder (sample `index.html`, `style.css`, `logo.png` are included).
2. From repo root run:

```bash
dotnet run --project StaticContentUploader
```

The uploader will create a container named `static-assets`, set blob access to public and upload files. The console prints the uploaded blob URIs.

Notes
- The uploader wraps the upload operations with Polly `Retry`, `CircuitBreaker` and `Timeout` policies to demonstrate integrating the same resiliency patterns used in the main API.
- For production hosting use Azure Blob Storage + CDN for static content hosting.
