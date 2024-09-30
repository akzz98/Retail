using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Retail.Entities;
using Retail.Services;
using System.Text.Json;

public class CategoryFunction
{
    private readonly CategoryStorageService _categoryStorageService;
    private readonly ILogger<CategoryFunction> _logger;

    public CategoryFunction(CategoryStorageService categoryStorageService, ILogger<CategoryFunction> logger)
    {
        _categoryStorageService = categoryStorageService;
        _logger = logger;
    }

    [Function("GetAllCategories")]
    public async Task<HttpResponseData> GetAllCategories([HttpTrigger(AuthorizationLevel.Function, "get", "GetAllCategories")] HttpRequestData req)
    {
        var categories = await _categoryStorageService.GetAllCategoriesAsync();
        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await response.WriteAsJsonAsync(categories);
        return response;
    }

    [Function("GetCategory")]
    public async Task<HttpResponseData> GetCategory(
        [HttpTrigger(AuthorizationLevel.Function, "get", "GetCategory/{partitionKey}/{rowKey}")] HttpRequestData req,
        string partitionKey, string rowKey)
    {
        var category = await _categoryStorageService.GetCategoryAsync(partitionKey, rowKey);
        var response = req.CreateResponse(category == null ? System.Net.HttpStatusCode.NotFound : System.Net.HttpStatusCode.OK);
        if (category != null)
        {
            await response.WriteAsJsonAsync(category);
        }
        return response;
    }

    [Function("CreateCategory")]
    public async Task<HttpResponseData> CreateCategory(
    [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Creating a new category.");

        var category = await req.ReadFromJsonAsync<CategoryEntity>();

        if (category == null || string.IsNullOrEmpty(category.Name))
        {
            var badRequestResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Category Name is required.");
            return badRequestResponse;
        }

        // Auto-generate RowKey using GUID
        category.PartitionKey = "Categories";
        category.RowKey = Guid.NewGuid().ToString();

        _logger.LogInformation("Generated RowKey: {RowKey}", category.RowKey);

        // Add the new category entity
        await _categoryStorageService.AddCategoryAsync(category);

        var response = req.CreateResponse(System.Net.HttpStatusCode.Created);
        await response.WriteAsJsonAsync(category);

        return response;
    }


    [Function("UpdateCategory")]
    public async Task<HttpResponseData> UpdateCategory([HttpTrigger(AuthorizationLevel.Function, "put", "UpdateCategory")] HttpRequestData req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var category = JsonSerializer.Deserialize<CategoryEntity>(requestBody);
        await _categoryStorageService.UpdateCategoryAsync(category);
        var response = req.CreateResponse(System.Net.HttpStatusCode.NoContent);
        return response;
    }

    [Function("DeleteCategory")]
    public async Task<HttpResponseData> DeleteCategory(
        [HttpTrigger(AuthorizationLevel.Function, "delete", "DeleteCategory/{partitionKey}/{rowKey}")] HttpRequestData req,
        string partitionKey, string rowKey)
    {
        await _categoryStorageService.DeleteCategoryAsync(partitionKey, rowKey);
        var response = req.CreateResponse(System.Net.HttpStatusCode.NoContent);
        return response;
    }
}
