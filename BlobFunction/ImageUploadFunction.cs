using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BlobFunction
{
    public class ImageUploadFunction
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<ImageUploadFunction> _logger;

        // Constructor injection for ILogger and BlobServiceClient
        public ImageUploadFunction(BlobServiceClient blobServiceClient, ILogger<ImageUploadFunction> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        [Function("UploadImage")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
        {
            log.LogInformation("UploadImage function triggered.");

            IFormFile file = req.Form.Files["image"];
            if (file == null || file.Length == 0)
            {
                log.LogWarning("No image file provided.");
                return new BadRequestObjectResult("No image uploaded.");
            }

            log.LogInformation("Image received: {FileName}, size: {FileSize} bytes", file.FileName, file.Length);

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient("images");
                await containerClient.CreateIfNotExistsAsync();
                log.LogInformation("Blob container 'images' created or exists.");

                var blobClient = containerClient.GetBlobClient(file.FileName);

                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                    log.LogInformation("File uploaded to Blob Storage: {BlobUri}", blobClient.Uri);
                }

                var imageUrl = blobClient.Uri.ToString();
                return new OkObjectResult(new { imageUrl });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred during image upload.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
