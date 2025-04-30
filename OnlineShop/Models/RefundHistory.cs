using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class RefundHistory
    {
        [Key]
        public int RefundId { get; set; }

        [Required]
        public string OrderId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public decimal RefundAmount { get; set; }

        [Required]
        public DateTime RefundDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string RefundStatus { get; set; } = "Pending";

        [StringLength(500)]
        public string Reason { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
