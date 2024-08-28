using Azure.Data.Tables;
using Retail.Entities;

namespace Retail.Services
{
    public class CategoryStorageService
    {
        private readonly TableClient _tableClient;

        public CategoryStorageService(string connectionString, string tableName)
        {
            var tableServiceClient = new TableServiceClient(connectionString);
            var tableClient = tableServiceClient.GetTableClient(tableName);
            tableClient.CreateIfNotExists(); // Ensure the table exists
            _tableClient = tableClient;
        }

        public async Task<IEnumerable<CategoryEntity>> GetAllCategoriesAsync()
        {
            var categories = _tableClient.QueryAsync<CategoryEntity>();
            var categoryList = new List<CategoryEntity>();

            await foreach (var category in categories)
            {
                categoryList.Add(category);
            }

            return categoryList;
        }


        public async Task<CategoryEntity> GetCategoryAsync(string partitionKey, string rowKey)
        {
            return await _tableClient.GetEntityAsync<CategoryEntity>(partitionKey, rowKey);
        }

        public async Task AddCategoryAsync(CategoryEntity category)
        {
            await _tableClient.AddEntityAsync(category);
        }

        public async Task UpdateCategoryAsync(CategoryEntity category)
        {
            var existingCategory = await GetCategoryAsync(category.PartitionKey, category.RowKey);

            if (existingCategory != null)
            {
                // Use the ETag of the existing entity
                existingCategory.Name = category.Name;

                await _tableClient.UpdateEntityAsync(existingCategory, existingCategory.ETag);
            }
            else
            {
                throw new InvalidOperationException("Category to update not found.");
            }
        }


        public async Task DeleteCategoryAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}
