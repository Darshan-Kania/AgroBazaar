using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using AgroBazaar.Data;
using AgroBazaar.Repositories.Interfaces;
using AgroBazaar.Repositories.UnitOfWork;

namespace AgroBazaar.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        // Repository instances
        private IUserRepository? _users;
        private IProductRepository? _products;
        private IOrderRepository? _orders;
        private ICartRepository? _carts;
        private ICategoryRepository? _categories;
        private IProductRatingRepository? _productRatings;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IProductRepository Products => _products ??= new ProductRepository(_context);
        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
        public ICartRepository Carts => _carts ??= new CartRepository(_context);
        public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
        public IProductRatingRepository ProductRatings => _productRatings ??= new ProductRatingRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
