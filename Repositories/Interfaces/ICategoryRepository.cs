using AgroBazaar.Models.Entities;

namespace AgroBazaar.Repositories.Interfaces
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<Category?> GetWithProductsAsync(int categoryId);
        Task<IEnumerable<Category>> GetCategoriesWithProductCountAsync();
        Task<bool> HasProductsAsync(int categoryId);
        Task<int> GetProductCountAsync(int categoryId);
    }
}
