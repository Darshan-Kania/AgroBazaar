using Microsoft.EntityFrameworkCore;
using AgroBazaar.Data;
using AgroBazaar.Models.Entities;
using AgroBazaar.Repositories.Interfaces;

namespace AgroBazaar.Repositories.Implementations
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetByFarmerIdAsync(string farmerId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.FarmerId == farmerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _dbSet
                .Include(p => p.Farmer)
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsActive && p.QuantityAvailable > 0)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAvailableProductsAsync()
        {
            return await _dbSet
                .Include(p => p.Farmer)
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.QuantityAvailable > 0)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            return await _dbSet
                .Include(p => p.Farmer)
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.QuantityAvailable > 0 && 
                           (p.Name.Contains(searchTerm) || 
                            p.Description!.Contains(searchTerm) ||
                            p.Category.Name.Contains(searchTerm)))
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetWithDetailsAsync(int productId)
        {
            return await _dbSet
                .Include(p => p.Farmer)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10)
        {
            return await _dbSet
                .Include(p => p.Farmer)
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.QuantityAvailable > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _dbSet
                .Include(p => p.Farmer)
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.QuantityAvailable > 0 && 
                           p.Price >= minPrice && p.Price <= maxPrice)
                .OrderBy(p => p.Price)
                .ToListAsync();
        }

        public async Task<bool> IsProductAvailableAsync(int productId, int requestedQuantity)
        {
            var product = await _dbSet.FindAsync(productId);
            return product != null && product.IsActive && product.QuantityAvailable >= requestedQuantity;
        }

        public async Task UpdateQuantityAsync(int productId, int newQuantity)
        {
            var product = await _dbSet.FindAsync(productId);
            if (product != null)
            {
                product.QuantityAvailable = newQuantity;
                product.UpdatedAt = DateTime.UtcNow;
                _dbSet.Update(product);
            }
        }

        public async Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _dbSet
                .Include(p => p.Farmer)
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.QuantityAvailable <= threshold && p.QuantityAvailable > 0)
                .OrderBy(p => p.QuantityAvailable)
                .ToListAsync();
        }
    }
}
