using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroBazaar.Models.Entities
{
    public class Cart
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Key
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Computed Property
        [NotMapped]
        public decimal TotalAmount => CartItems.Sum(item => item.TotalPrice);

        [NotMapped]
        public int TotalItems => CartItems.Sum(item => item.Quantity);
    }
}
