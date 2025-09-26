using Microsoft.EntityFrameworkCore;
using AgroBazaar.Data;
using AgroBazaar.Models.Entities;
using AgroBazaar.Repositories.Interfaces;

namespace AgroBazaar.Repositories.Implementations
{
    public class ProductRatingRepository : GenericRepository<ProductRating>, IProductRatingRepository
    {
        public ProductRatingRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<ProductRating>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductRatings
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProductRating?> GetUserRatingAsync(string userId, int productId)
        {
            return await _context.ProductRatings
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);
        }

        public async Task<double> GetAverageRatingAsync(int productId)
        {
            var query = _context.ProductRatings.Where(r => r.ProductId == productId);
            var count = await query.CountAsync();
            if (count == 0) return 0d;
            var sum = await query.SumAsync(r => r.Rating);
            return (double)sum / count;
        }

        public async Task AddOrUpdateAsync(string userId, int productId, int rating, string? comment)
        {
            var existing = await GetUserRatingAsync(userId, productId);
            if (existing == null)
            {
                var pr = new ProductRating
                {
                    UserId = userId,
                    ProductId = productId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.ProductRatings.AddAsync(pr);
            }
            else
            {
                existing.Rating = rating;
                existing.Comment = comment;
                existing.CreatedAt = DateTime.UtcNow;
                _context.ProductRatings.Update(existing);
            }
        }
    }
}

