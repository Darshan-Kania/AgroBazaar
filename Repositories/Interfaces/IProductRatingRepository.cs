using AgroBazaar.Models.Entities;

namespace AgroBazaar.Repositories.Interfaces
{
    public interface IProductRatingRepository : IGenericRepository<ProductRating>
    {
        Task<IEnumerable<ProductRating>> GetByProductIdAsync(int productId);
        Task<ProductRating?> GetUserRatingAsync(string userId, int productId);
        Task<double> GetAverageRatingAsync(int productId);
        Task AddOrUpdateAsync(string userId, int productId, int rating, string? comment);
    }
}

