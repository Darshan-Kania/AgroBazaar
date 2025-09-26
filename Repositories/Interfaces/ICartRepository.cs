using AgroBazaar.Models.Entities;

namespace AgroBazaar.Repositories.Interfaces
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart?> GetByUserIdAsync(string userId);
        Task<Cart?> GetWithItemsAsync(string userId);
        Task<CartItem?> GetCartItemAsync(int cartId, int productId);
        Task AddItemToCartAsync(string userId, int productId, int quantity);
        Task UpdateCartItemQuantityAsync(int cartId, int productId, int quantity);
        Task RemoveItemFromCartAsync(int cartId, int productId);
        Task ClearCartAsync(string userId);
        Task<decimal> GetCartTotalAsync(string userId);
        Task<int> GetCartItemCountAsync(string userId);
        Task<bool> IsProductInCartAsync(string userId, int productId);
    }
}
