using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Retail.Entities;
using Retail.Services;
using System.Net;
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
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(categories);
        return response;
    }

    [Function("GetCategory")]
    public async Task<HttpResponseData> GetCategory(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetCategory/{partitionKey}/{rowKey}")] HttpRequestData req,
        string partitionKey, string rowKey)
    {
        var category = await _categoryStorageService.GetCategoryAsync(partitionKey, rowKey);
        var response = req.CreateResponse(category == null ? HttpStatusCode.NotFound : HttpStatusCode.OK);
        if (category != null)
        {
            await response.WriteAsJsonAsync(category);
        }
        return response;
    }

    [Function("CreateCategory")]
    public async Task<HttpResponseData> CreateCategory(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "CreateCategory")] HttpRequestData req)
    {
        var logger = req.FunctionContext.GetLogger("CreateCategoryFunction");
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var category = JsonSerializer.Deserialize<CategoryEntity>(requestBody);

            // Ensure RowKey is generated if not provided
            if (string.IsNullOrEmpty(category.RowKey))
            {
                category.RowKey = Guid.NewGuid().ToString(); // Auto-generate RowKey
            }

            await _categoryStorageService.AddCategoryAsync(category);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync("Category created successfully");
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError($"Error creating category: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }

    [Function("UpdateCategory")]
    public async Task<HttpResponseData> UpdateCategory(
    [HttpTrigger(AuthorizationLevel.Function, "put", Route = "UpdateCategory")] HttpRequestData req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Console.WriteLine($"Request body: {requestBody}");

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Request body cannot be empty.");
                return badRequestResponse;
            }

            var category = JsonSerializer.Deserialize<CategoryEntity>(requestBody);

            if (category == null || string.IsNullOrEmpty(category.PartitionKey) || string.IsNullOrEmpty(category.RowKey))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid category data.");
                return badRequestResponse;
            }

            // Fetch the existing category
            var existingCategory = await _categoryStorageService.GetCategoryAsync(category.PartitionKey, category.RowKey);
            if (existingCategory == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Category not found.");
                return notFoundResponse;
            }

            // Ensure ETag and Timestamp are used for concurrency control
            category.ETag = existingCategory.ETag;
            category.Timestamp = existingCategory.Timestamp;

            await _categoryStorageService.UpdateCategoryAsync(category);
            var response = req.CreateResponse(HttpStatusCode.NoContent);
            return response;
        }
        catch (Exception ex)
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error: {ex.Message}");
            return errorResponse;
        }
    }


    [Function("DeleteCategory")]
    public async Task<HttpResponseData> DeleteCategory(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "DeleteCategory/{partitionKey}/{rowKey}")] HttpRequestData req,
        string partitionKey, string rowKey)
    {
        await _categoryStorageService.DeleteCategoryAsync(partitionKey, rowKey);
        var response = req.CreateResponse(HttpStatusCode.NoContent);
        return response;
    }
}
