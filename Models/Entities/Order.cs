using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgroBazaar.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered, Cancelled

        [Required]
        [StringLength(20)]
        public string PaymentMethod { get; set; } = "COD"; // Cash on Delivery

        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Failed, Cancelled

        // Delivery Address
        [Required]
        [StringLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string PinCode { get; set; } = string.Empty;

        [StringLength(15)]
        public string? ContactNumber { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveryDate { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Foreign Key
        [Required]
        public string CustomerId { get; set; } = string.Empty;

        // Navigation Properties
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser Customer { get; set; } = null!;
        
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
