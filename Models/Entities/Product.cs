using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AgroBazaar.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Unit is required")]
        [StringLength(20)]
        public string Unit { get; set; } = string.Empty; // kg, piece, liter, etc.

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be 0 or greater")]
        public int QuantityAvailable { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Keys
        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        
        // This will be set programmatically, not from form
        [BindNever]
        public string FarmerId { get; set; } = string.Empty;

        // Navigation Properties - Exclude from validation
        [BindNever]
        [ValidateNever]
        [ForeignKey("FarmerId")]
        public virtual ApplicationUser Farmer { get; set; } = null!;
        
        [BindNever]
        [ValidateNever]
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;
        
        [BindNever]
        [ValidateNever]
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        
        [BindNever]
        [ValidateNever]
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        
        [BindNever]
        [ValidateNever]
        public virtual ICollection<ProductRating> Ratings { get; set; } = new List<ProductRating>();
    }
}
