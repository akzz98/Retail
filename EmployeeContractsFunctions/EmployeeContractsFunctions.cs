using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

public class EmployeeContractFunctions
{
    private readonly FileStorageService _fileStorageService;
    private readonly ILogger<EmployeeContractFunctions> _logger;

    public EmployeeContractFunctions(FileStorageService fileStorageService, ILogger<EmployeeContractFunctions> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [Function("ListEmployeeContracts")]
    public async Task<HttpResponseData> ListEmployeeContracts([HttpTrigger(AuthorizationLevel.Function, "get", Route = "contracts")] HttpRequestData req)
    {
        _logger.LogInformation("Listing employee contracts");

        // List files from the "employeecontracts" directory
        var contracts = await _fileStorageService.ListFilesAsync("employeecontracts");

        // Return a JSON response
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(contracts);
        return response;
    }

    [Function("UploadEmployeeContract")]
    public async Task<HttpResponseData> UploadEmployeeContract([HttpTrigger(AuthorizationLevel.Function, "post", Route = "contracts/upload")] HttpRequestData req)
    {
        var form = await ParseFormAsync(req); // Use the new method to parse form data
        var fileName = form["fileName"];
        var fileStream = req.Body;

        _logger.LogInformation($"Uploading contract {fileName}");

        // Upload the file to the "employeecontracts" directory
        await _fileStorageService.UploadFileAsync("employeecontracts", fileName, fileStream);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("File uploaded successfully.");
        return response;
    }

    [Function("DownloadEmployeeContract")]
    public async Task<HttpResponseData> DownloadEmployeeContract([HttpTrigger(AuthorizationLevel.Function, "get", Route = "contracts/download/{fileName}")] HttpRequestData req, string fileName)
    {
        _logger.LogInformation($"Downloading contract {fileName}");

        // Download the file from the "employeecontracts" directory
        var fileStream = await _fileStorageService.DownloadFileAsync("employeecontracts", fileName);

        if (fileStream == null)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync("File not found.");
            return notFoundResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");
        response.Headers.Add("Content-Type", "application/octet-stream");

        await fileStream.CopyToAsync(response.Body);
        return response;
    }

    [Function("DeleteEmployeeContract")]
    public async Task<HttpResponseData> DeleteEmployeeContract([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "contracts/{fileName}")] HttpRequestData req, string fileName)
    {
        _logger.LogInformation($"Deleting contract {fileName}");

        // Delete the file from the "employeecontracts" directory
        var deleteSuccess = await _fileStorageService.DeleteFileAsync("employeecontracts", fileName);

        var response = req.CreateResponse(deleteSuccess ? HttpStatusCode.OK : HttpStatusCode.NotFound);
        await response.WriteStringAsync(deleteSuccess ? "File deleted successfully." : "File not found.");
        return response;
    }

    private async Task<Dictionary<string, string>> ParseFormAsync(HttpRequestData req)
    {
        var form = new Dictionary<string, string>();
        using (var reader = new StreamReader(req.Body))
        {
            var content = await reader.ReadToEndAsync();
            var pairs = content.Split('&');
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    form[keyValue[0]] = keyValue[1];
                }
            }
        }
        return form;
    }
}
