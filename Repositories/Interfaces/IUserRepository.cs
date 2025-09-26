using AgroBazaar.Models.Entities;

namespace AgroBazaar.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<ApplicationUser?> GetByUsernameAsync(string username);
        Task<IEnumerable<ApplicationUser>> GetByUserTypeAsync(string userType);
        Task<IEnumerable<ApplicationUser>> GetFarmersAsync();
        Task<IEnumerable<ApplicationUser>> GetConsumersAsync();
        Task<ApplicationUser?> GetWithProductsAsync(string userId);
        Task<ApplicationUser?> GetWithOrdersAsync(string userId);
        Task<ApplicationUser?> GetWithCartAsync(string userId);
        Task<bool> IsEmailTakenAsync(string email, string? excludeUserId = null);
        Task<bool> IsUsernameTakenAsync(string username, string? excludeUserId = null);
    }
}
