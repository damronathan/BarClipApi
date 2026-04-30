using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using BarClipApi.Models.Requests;

namespace BarClipApi.Core.Services;

public class StorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public StorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<(string, string)> DownloadVideoAsync(string fileName, string containerName)
    {
        string tempFilePath = Path.GetTempPath();

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient = containerClient.GetBlobClient(fileName);

        string videoFilePath = Path.Combine(tempFilePath, fileName);

        await blobClient.DownloadToAsync(videoFilePath);

        var properties = await blobClient.GetPropertiesAsync();

        var userId = GetRequiredGuidFromMetadata(properties.Value.Metadata, "userId");

        return (videoFilePath, userId);
    }

    public async Task UploadVideoAsync(Guid blobName, string filePath, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient = containerClient.GetBlobClient(blobName.ToString() + ".mp4");

        await blobClient.UploadAsync(filePath, overwrite: true);
    }

    public string GenerateUploadSasUrl(SasUrlRequest request)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(request.ContainerName);

        var blobClient = containerClient.GetBlobClient(request.Id.ToString() + request.Extension);

        var sasUri = blobClient.GenerateSasUri(BlobSasPermissions.Create, DateTimeOffset.UtcNow.AddHours(1));

        return sasUri.ToString();
    }

    public string GenerateDownloadSasUrl(SasUrlRequest request)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(request.ContainerName);

        var blobClient = containerClient.GetBlobClient(request.Id.ToString() + request.Extension);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = request.ContainerName,
            BlobName = request.Id.ToString() + request.Extension,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-15),
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(24)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri.ToString();
    }

    public async virtual Task<IDictionary<string, string>> GetMetaDataAsync(string  blobName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var properties = await blobClient.GetPropertiesAsync();
        return properties.Value.Metadata;
    }

    public async Task DeleteVideoAsync(Guid blobName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient = containerClient.GetBlobClient(blobName.ToString() + ".mov");

        var response = await blobClient.DeleteIfExistsAsync();
    }
    private static string GetRequiredGuidFromMetadata(IDictionary<string, string> metadata, string key)
    {
        if (!metadata.TryGetValue(key, out var value))
        {
            throw new Exception("No userId");
        }
        return value;
            



    }
}
