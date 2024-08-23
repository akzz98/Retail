using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;

namespace Retail.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(string connectionString, string containerName)
        {
            var blobServiceClient = new BlobServiceClient(connectionString);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            _containerClient.CreateIfNotExists(); // Ensure the container exists
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(imageStream, overwrite: true);
            return blobClient.Uri.ToString(); // Return the URL of the uploaded image
        }
    }
}