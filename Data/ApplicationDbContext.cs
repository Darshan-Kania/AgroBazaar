using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AgroBazaar.Models.Entities;

namespace AgroBazaar.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            // Enable lazy loading for better performance
            ChangeTracker.LazyLoadingEnabled = true;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
        }

        // DbSets for all entities
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ProductRating> ProductRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure indexes for better query performance
            
            // ApplicationUser indexes for authentication optimization
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
                
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.UserType)
                .HasDatabaseName("IX_Users_UserType");
                
            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => new { u.IsActive, u.UserType })
                .HasDatabaseName("IX_Users_IsActive_UserType");

            // Product indexes for better search performance
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.FarmerId)
                .HasDatabaseName("IX_Products_FarmerId");
                
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CategoryId)
                .HasDatabaseName("IX_Products_CategoryId");
                
            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.IsActive, p.CategoryId })
                .HasDatabaseName("IX_Products_IsActive_CategoryId");
                
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CreatedAt)
                .HasDatabaseName("IX_Products_CreatedAt");

            // Order indexes for dashboard performance
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.CustomerId)
                .HasDatabaseName("IX_Orders_CustomerId");
                
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderDate)
                .HasDatabaseName("IX_Orders_OrderDate");
                
            modelBuilder.Entity<Order>()
                .HasIndex(o => new { o.CustomerId, o.OrderDate })
                .HasDatabaseName("IX_Orders_CustomerId_OrderDate");

            // Cart indexes
            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .IsUnique()
                .HasDatabaseName("IX_Carts_UserId");

            // ProductRating relationships and indexes
            modelBuilder.Entity<ProductRating>()
                .HasIndex(r => r.ProductId)
                .HasDatabaseName("IX_ProductRatings_ProductId");

            modelBuilder.Entity<ProductRating>()
                .HasIndex(r => r.UserId)
                .HasDatabaseName("IX_ProductRatings_UserId");

            modelBuilder.Entity<ProductRating>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Ratings)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductRating>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationships and constraints
            
            // Product relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Farmer)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.FarmerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderItem relationships
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cart relationships
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CartItem relationships
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraints
            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            // Composite unique constraint for cart items (one product per cart)
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => new { ci.CartId, ci.ProductId })
                .IsUnique();

            // Decimal precision configuration
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(10, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TotalPrice)
                .HasPrecision(10, 2);

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasPrecision(10, 2);

            // Configure lazy loading proxies for navigation properties
            modelBuilder.Entity<ApplicationUser>()
                .Navigation(e => e.Products)
                .EnableLazyLoading();
                
            modelBuilder.Entity<ApplicationUser>()
                .Navigation(e => e.Orders)
                .EnableLazyLoading();
                
            modelBuilder.Entity<ApplicationUser>()
                .Navigation(e => e.Cart)
                .EnableLazyLoading();

            modelBuilder.Entity<Product>()
                .Navigation(e => e.Farmer)
                .EnableLazyLoading();
                
            modelBuilder.Entity<Product>()
                .Navigation(e => e.Category)
                .EnableLazyLoading();
                
            modelBuilder.Entity<Product>()
                .Navigation(e => e.OrderItems)
                .EnableLazyLoading();
                
            modelBuilder.Entity<Product>()
                .Navigation(e => e.CartItems)
                .EnableLazyLoading();

            modelBuilder.Entity<Order>()
                .Navigation(e => e.Customer)
                .EnableLazyLoading();
                
            modelBuilder.Entity<Order>()
                .Navigation(e => e.OrderItems)
                .EnableLazyLoading();

            modelBuilder.Entity<Cart>()
                .Navigation(e => e.User)
                .EnableLazyLoading();
                
            modelBuilder.Entity<Cart>()
                .Navigation(e => e.CartItems)
                .EnableLazyLoading();

            // Seed data for categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Vegetables", Description = "Fresh vegetables", CreatedAt = DateTime.UtcNow },
                new Category { Id = 2, Name = "Fruits", Description = "Fresh fruits", CreatedAt = DateTime.UtcNow },
                new Category { Id = 3, Name = "Grains", Description = "Cereals and grains", CreatedAt = DateTime.UtcNow },
                new Category { Id = 4, Name = "Dairy", Description = "Dairy products", CreatedAt = DateTime.UtcNow }
            );
        }

        // Override SaveChanges for performance optimization
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Automatically set timestamps
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                if (entityEntry.Entity is BaseEntity entity)
                {
                    if (entityEntry.State == EntityState.Added)
                    {
                        entity.CreatedAt = DateTime.UtcNow;
                    }
                    else if (entityEntry.State == EntityState.Modified)
                    {
                        entity.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        // Add a base entity interface for timestamp management
        public interface BaseEntity
        {
            DateTime CreatedAt { get; set; }
            DateTime? UpdatedAt { get; set; }
        }
    }
}
