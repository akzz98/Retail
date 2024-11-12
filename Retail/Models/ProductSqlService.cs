using Microsoft.EntityFrameworkCore;
using Retail.Data;
using Retail.Entities;

namespace Retail.Models
{
    public class ProductSqlService
    {
        private readonly ApplicationDbContext _context;

        public ProductSqlService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Retrieve all products from the SQL database
        public async Task<IEnumerable<ProductEntity>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        // Retrieve a single product by Id
        public async Task<ProductEntity> GetProductAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        // Add a new product to the SQL database
        public async Task AddProductAsync(ProductEntity product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        // Update an existing product in the SQL database
        public async Task UpdateProductAsync(ProductEntity product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        // Delete a product from the SQL database
        public async Task DeleteProductAsync(int id)
        {
            var product = await GetProductAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
    }
}
