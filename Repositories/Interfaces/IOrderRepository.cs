using AgroBazaar.Models.Entities;

namespace AgroBazaar.Repositories.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<Order>> GetByStatusAsync(string status);
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task<Order?> GetWithItemsAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Order>> GetPendingOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersForFarmerAsync(string farmerId);
        Task<decimal> GetTotalSalesByFarmerAsync(string farmerId, DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetOrderCountByStatusAsync(string status);
        Task UpdateOrderStatusAsync(int orderId, string newStatus);
        Task<string> GenerateOrderNumberAsync();
        Task<bool> CancelOrderAsync(int orderId, string? cancellationReason = null);
    }
}