using Microsoft.EntityFrameworkCore;
using AgroBazaar.Data;
using AgroBazaar.Models.Entities;
using AgroBazaar.Repositories.Interfaces;

namespace AgroBazaar.Repositories.Implementations
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Cart?> GetByUserIdAsync(string userId)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart?> GetWithItemsAsync(string userId)
        {
            return await _dbSet
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Farmer)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<CartItem?> GetCartItemAsync(int cartId, int productId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
        }

        public async Task AddItemToCartAsync(string userId, int productId, int quantity)
        {
            var cart = await GetByUserIdAsync(userId);
            
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                await AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = await GetCartItemAsync(cart.Id, productId);
            var product = await _context.Products.FindAsync(productId);

            if (product == null) return;

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };
                await _context.CartItems.AddAsync(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
        }

        public async Task UpdateCartItemQuantityAsync(int cartId, int productId, int quantity)
        {
            var cartItem = await GetCartItemAsync(cartId, productId);
            if (cartItem != null)
            {
                if (quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                    cartItem.UpdatedAt = DateTime.UtcNow;
                    _context.CartItems.Update(cartItem);
                }

                var cart = await _dbSet.FindAsync(cartId);
                if (cart != null)
                {
                    cart.UpdatedAt = DateTime.UtcNow;
                    Update(cart);
                }
            }
        }

        public async Task RemoveItemFromCartAsync(int cartId, int productId)
        {
            var cartItem = await GetCartItemAsync(cartId, productId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                
                var cart = await _dbSet.FindAsync(cartId);
                if (cart != null)
                {
                    cart.UpdatedAt = DateTime.UtcNow;
                    Update(cart);
                }
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await GetWithItemsAsync(userId);
            if (cart != null && cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedAt = DateTime.UtcNow;
                Update(cart);
            }
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            var cart = await GetWithItemsAsync(userId);
            return cart?.TotalAmount ?? 0;
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            var cart = await GetWithItemsAsync(userId);
            return cart?.TotalItems ?? 0;
        }

        public async Task<bool> IsProductInCartAsync(string userId, int productId)
        {
            var cart = await GetByUserIdAsync(userId);
            if (cart == null) return false;

            return await _context.CartItems
                .AnyAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);
        }
    }
}
