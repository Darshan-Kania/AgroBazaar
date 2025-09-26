using AgroBazaar.Repositories.Interfaces;

namespace AgroBazaar.Repositories.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        ICartRepository Carts { get; }
        ICategoryRepository Categories { get; }
        IProductRatingRepository ProductRatings { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
