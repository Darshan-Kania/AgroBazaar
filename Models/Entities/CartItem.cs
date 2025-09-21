using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroBazaar.Models.Entities
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Keys
        [Required]
        public int CartId { get; set; }
        
        [Required]
        public int ProductId { get; set; }

        // Navigation Properties
        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; } = null!;
        
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        // Computed Property
        [NotMapped]
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
