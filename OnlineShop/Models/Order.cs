using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Order
    {
        [Key]
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DeliveryAddress { get; set; } // Thêm trường địa chỉ giao hàng
        public string UserId { get; set; } // Liên kết với ApplicationUser
        public ApplicationUser User { get; set; } // Navigation property
        public List<OrderDetail> OrderItems { get; set; } = new List<OrderDetail>();
    }
}
