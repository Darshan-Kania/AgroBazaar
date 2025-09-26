using Microsoft.EntityFrameworkCore;
using AgroBazaar.Data;
using AgroBazaar.Models.Entities;
using AgroBazaar.Repositories.Interfaces;

namespace AgroBazaar.Repositories.Implementations
{
    public class UserRepository : GenericRepository<ApplicationUser>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<IEnumerable<ApplicationUser>> GetByUserTypeAsync(string userType)
        {
            return await _dbSet.Where(u => u.UserType == userType && u.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<ApplicationUser>> GetFarmersAsync()
        {
            return await GetByUserTypeAsync("Farmer");
        }

        public async Task<IEnumerable<ApplicationUser>> GetConsumersAsync()
        {
            return await GetByUserTypeAsync("Consumer");
        }

        public async Task<ApplicationUser?> GetWithProductsAsync(string userId)
        {
            return await _dbSet
                .Include(u => u.Products)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<ApplicationUser?> GetWithOrdersAsync(string userId)
        {
            return await _dbSet
                .Include(u => u.Orders)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<ApplicationUser?> GetWithCartAsync(string userId)
        {
            return await _dbSet
                .Include(u => u.Cart)
                .ThenInclude(c => c!.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> IsEmailTakenAsync(string email, string? excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.Email == email);
            
            if (!string.IsNullOrEmpty(excludeUserId))
                query = query.Where(u => u.Id != excludeUserId);
                
            return await query.AnyAsync();
        }

        public async Task<bool> IsUsernameTakenAsync(string username, string? excludeUserId = null)
        {
            var query = _dbSet.Where(u => u.UserName == username);
            
            if (!string.IsNullOrEmpty(excludeUserId))
                query = query.Where(u => u.Id != excludeUserId);
                
            return await query.AnyAsync();
        }
    }
}
