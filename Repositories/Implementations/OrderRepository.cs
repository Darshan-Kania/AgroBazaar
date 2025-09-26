using Microsoft.EntityFrameworkCore;
using AgroBazaar.Data;
using AgroBazaar.Models.Entities;
using AgroBazaar.Repositories.Interfaces;

namespace AgroBazaar.Repositories.Implementations
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber);
        }

        public async Task<Order?> GetWithItemsAsync(int orderId)
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Farmer)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetPendingOrdersAsync()
        {
            return await GetByStatusAsync("Pending");
        }

        public async Task<IEnumerable<Order>> GetOrdersForFarmerAsync(string farmerId)
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderItems.Any(oi => oi.Product.FarmerId == farmerId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalSalesByFarmerAsync(string farmerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbSet
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.OrderItems.Any(oi => oi.Product.FarmerId == farmerId) && 
                           (o.Status == "Delivered" || o.Status == "Shipped"));

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(o => o.OrderDate <= endDate.Value);

            var orders = await query.ToListAsync();
            
            return orders.Sum(o => o.OrderItems
                .Where(oi => oi.Product.FarmerId == farmerId)
                .Sum(oi => oi.TotalPrice));
        }

        public async Task<int> GetOrderCountByStatusAsync(string status)
        {
            return await _dbSet.CountAsync(o => o.Status == status);
        }

        public async Task UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            var order = await _dbSet.FindAsync(orderId);
            if (order != null)
            {
                order.Status = newStatus;
                order.UpdatedAt = DateTime.UtcNow;
                
                if (newStatus == "Delivered")
                    order.DeliveryDate = DateTime.UtcNow;
                    
                _dbSet.Update(order);
            }
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var lastOrder = await _dbSet
                .OrderByDescending(o => o.Id)
                .FirstOrDefaultAsync();

            var orderCount = (lastOrder?.Id ?? 0) + 1;
            return $"ORD{DateTime.UtcNow:yyyyMMdd}{orderCount:D6}";
        }
    }
}
