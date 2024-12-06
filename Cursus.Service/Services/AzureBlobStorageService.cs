using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Cursus.ServiceContract.Interfaces;


public class AzureBlobStorageService : IAzureBlobStorageService
{
    private readonly string _connectionString;
    private readonly string _containerName;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        _connectionString = configuration.GetSection("AzureBlobStorage:ConnectionString").Value;
        _containerName = configuration.GetSection("AzureBlobStorage:ContainerName").Value;
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(_containerName);
        await blobContainerClient.CreateIfNotExistsAsync();

        var blobClient = blobContainerClient.GetBlobClient(file.FileName);
        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
        }

        return blobClient.Uri.ToString();
    }
}
