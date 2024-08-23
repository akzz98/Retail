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

        public async Task DeleteImageAsync(string imageUrl)
        {
            // Ensure the image URL is correctly decoded/encoded
            var uri = new Uri(imageUrl);
            var blobName = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/').Split('/').Last());

            var blobClient = _containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }
    }
}