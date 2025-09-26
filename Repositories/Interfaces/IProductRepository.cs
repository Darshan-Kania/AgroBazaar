using AgroBazaar.Models.Entities;

namespace AgroBazaar.Repositories.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetByFarmerIdAsync(string farmerId);
        Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Product>> GetAvailableProductsAsync();
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<Product?> GetWithDetailsAsync(int productId);
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count = 10);
        Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<bool> IsProductAvailableAsync(int productId, int requestedQuantity);
        Task UpdateQuantityAsync(int productId, int newQuantity);
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold = 10);
    }
}
