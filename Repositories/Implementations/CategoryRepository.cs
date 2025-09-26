using Microsoft.EntityFrameworkCore;
using AgroBazaar.Data;
using AgroBazaar.Models.Entities;
using AgroBazaar.Repositories.Interfaces;

namespace AgroBazaar.Repositories.Implementations
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetWithProductsAsync(int categoryId)
        {
            return await _dbSet
                .Include(c => c.Products.Where(p => p.IsActive && p.QuantityAvailable > 0))
                .ThenInclude(p => p.Farmer)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithProductCountAsync()
        {
            return await _dbSet
                .Include(c => c.Products.Where(p => p.IsActive && p.QuantityAvailable > 0))
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.Products
                .AnyAsync(p => p.CategoryId == categoryId && p.IsActive && p.QuantityAvailable > 0);
        }

        public async Task<int> GetProductCountAsync(int categoryId)
        {
            return await _context.Products
                .CountAsync(p => p.CategoryId == categoryId && p.IsActive && p.QuantityAvailable > 0);
        }
    }
}
