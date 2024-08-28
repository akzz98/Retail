using Azure;
using Azure.Data.Tables;
using Retail.Entities;
using Retail.Models;

namespace Retail.Services
{
    public class TableStorageService
    {
        private readonly TableClient _productTableClient;

        public TableStorageService(string connectionString, string tableName)
        {
            _productTableClient = new TableClient(connectionString, tableName);
            _productTableClient.CreateIfNotExists();
        }

        public async Task<ProductEntity> GetProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _productTableClient.GetEntityAsync<ProductEntity>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // Handle not found
            }
            catch (Exception ex)
            {
                // Log and/or rethrow as needed
                throw new ApplicationException("An error occurred while retrieving the product.", ex);
            }
        }

        public async Task AddProductAsync(ProductEntity product)
        {
            try
            {
                await _productTableClient.AddEntityAsync(product);
            }
            catch (RequestFailedException ex) when (ex.Status == 409)
            {
                // Handle conflict (entity already exists)
                await UpdateProductAsync(product);
            }
            catch (Exception ex)
            {
                // Log and/or rethrow as needed
                throw new ApplicationException("An error occurred while adding the product.", ex);
            }
        }

        public async Task UpdateProductAsync(ProductEntity product)
        {
            try
            {
                await _productTableClient.UpdateEntityAsync(product, ETag.All, TableUpdateMode.Replace);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Handle not found
                throw new ApplicationException("The product to update does not exist.", ex);
            }
            catch (Exception ex)
            {
                // Log and/or rethrow as needed
                throw new ApplicationException("An error occurred while updating the product.", ex);
            }
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            try
            {
                await _productTableClient.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Handle not found
                throw new ApplicationException("The product to delete does not exist.", ex);
            }
            catch (Exception ex)
            {
                // Log and/or rethrow as needed
                throw new ApplicationException("An error occurred while deleting the product.", ex);
            }
        }

        public async Task<IEnumerable<ProductEntity>> GetAllProductsAsync()
        {
            var products = new List<ProductEntity>();
            try
            {
                await foreach (var entity in _productTableClient.QueryAsync<ProductEntity>())
                {
                    products.Add(entity);
                }
            }
            catch (Exception ex)
            {
                // Log and/or rethrow as needed
                throw new ApplicationException("An error occurred while retrieving all products.", ex);
            }
            return products;
        }       
    }
}