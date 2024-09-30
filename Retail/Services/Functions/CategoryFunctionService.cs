using Retail.Entities;
using System.Text;
using System.Text.Json;

public class CategoryFunctionService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public CategoryFunctionService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        // Base URL from appsettings.json
        _baseUrl = configuration["AzureFunctionSettings:BaseUrl"];
    }

    // Fetch all categories
    public async Task<IEnumerable<CategoryEntity>> GetAllCategoriesAsync()
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/GetAllCategories");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<CategoryEntity>>();
    }

    // Fetch a specific category
    public async Task<CategoryEntity> GetCategoryAsync(string partitionKey, string rowKey)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}/GetCategory/{partitionKey}/{rowKey}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CategoryEntity>();
    }

    // Create a new category
    public async Task CreateCategoryAsync(CategoryEntity category)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/CreateCategory")
        {
            Content = new StringContent(JsonSerializer.Serialize(category), Encoding.UTF8, "application/json") // Set Content-Type
        };

        var response = await _httpClient.SendAsync(requestMessage);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to create category. Status code: {response.StatusCode}, Error: {error}");
        }
    }


    // Update an existing category
    public async Task UpdateCategoryAsync(CategoryEntity category)
    {
        var response = await _httpClient.PutAsJsonAsync($"{_baseUrl}/UpdateCategory", category);
        response.EnsureSuccessStatusCode();
    }

    // Delete a category
    public async Task DeleteCategoryAsync(string partitionKey, string rowKey)
    {
        var response = await _httpClient.DeleteAsync($"{_baseUrl}/DeleteCategory/{partitionKey}/{rowKey}");
        response.EnsureSuccessStatusCode();
    }
}
