using Microsoft.EntityFrameworkCore;
using Retail.Data;
using Retail.Entities;

namespace Retail.Services
{
    public class CategorySqlService
    {
        private readonly ApplicationDbContext _context;

        public CategorySqlService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategorySqlEntity>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<CategorySqlEntity> GetCategoryAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task AddCategoryAsync(CategorySqlEntity category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(CategorySqlEntity category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await GetCategoryAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}
