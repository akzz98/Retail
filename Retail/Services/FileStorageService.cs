using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Files.Shares;
using Azure;
using Microsoft.Extensions.Logging;

public class FileStorageService
{
    private readonly ShareClient _shareClient;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(string connectionString, string shareName, ILogger<FileStorageService> logger)
    {
        _shareClient = new ShareClient(connectionString, shareName);
        _shareClient.CreateIfNotExists();
        _logger = logger;
    }

    public async Task<List<string>> ListFilesAsync(string directoryName)
    {
        var fileNames = new List<string>();
        _logger.LogInformation($"Attempting to list files in directory: {directoryName}");

        // Get a reference to the directory
        ShareDirectoryClient directoryClient = _shareClient.GetDirectoryClient(directoryName);

        // Ensure the directory exists
        if (await directoryClient.ExistsAsync())
        {
            _logger.LogInformation($"Directory {directoryName} exists. Listing files...");
            // List all files in the directory
            await foreach (ShareFileItem item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (item.IsDirectory == false)
                {
                    fileNames.Add(item.Name);
                }
            }
        }
        else
        {
            _logger.LogError($"Directory {directoryName} does not exist.");
            throw new Exception($"Directory {directoryName} does not exist.");
        }

        return fileNames;
    }

    public async Task UploadFileAsync(string directoryName, string fileName, Stream fileStream)
    {
        _logger.LogInformation($"Uploading file {fileName} to directory {directoryName}");

        var directoryClient = _shareClient.GetDirectoryClient(directoryName);
        var fileClient = directoryClient.GetFileClient(fileName);

        await fileClient.CreateAsync(fileStream.Length);
        await fileClient.UploadRangeAsync(new HttpRange(0, fileStream.Length), fileStream);
        _logger.LogInformation($"File {fileName} uploaded successfully.");
    }

    public async Task<Stream> DownloadFileAsync(string directoryName, string fileName)
    {
        // Trim any leading or trailing whitespace from fileName
        fileName = fileName?.Trim();

        _logger.LogInformation($"Attempting to download file '{fileName}' from directory '{directoryName}'.");

        var directoryClient = _shareClient.GetDirectoryClient(directoryName);

        if (!await directoryClient.ExistsAsync())
        {
            _logger.LogError($"Directory '{directoryName}' does not exist.");
            throw new Exception($"Directory '{directoryName}' does not exist.");
        }

        var fileClient = directoryClient.GetFileClient(fileName);

        if (!await fileClient.ExistsAsync())
        {
            _logger.LogError($"File '{fileName}' does not exist in directory '{directoryName}'.");
            throw new Exception($"File '{fileName}' does not exist in directory '{directoryName}'.");
        }

        try
        {
            var downloadInfo = await fileClient.DownloadAsync();
            _logger.LogInformation($"Successfully downloaded file '{fileName}' from directory '{directoryName}'.");
            return downloadInfo.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error occurred while downloading file '{fileName}' from directory '{directoryName}': {ex.Message}");
            throw;
        }
    }


        public async Task DeleteFileAsync(string directoryName, string fileName)
    {
        _logger.LogInformation($"Deleting file {fileName} from directory {directoryName}");

        var directoryClient = _shareClient.GetDirectoryClient(directoryName);
        var fileClient = directoryClient.GetFileClient(fileName);

        if (await fileClient.ExistsAsync())
        {
            await fileClient.DeleteAsync();
            _logger.LogInformation($"File {fileName} deleted successfully.");
        }
        else
        {
            _logger.LogError($"File {fileName} does not exist in {directoryName}.");
            throw new Exception($"File {fileName} does not exist in {directoryName}.");
        }
    }
}
