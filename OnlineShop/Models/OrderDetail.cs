using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class OrderDetail
    {
        [Key]
        public string Id { get; set; } // ID mục đơn hàng (tương tự CartItem.Id)
        public string OrderId { get; set; } // Liên kết với Order
        public int ProductId { get; set; } // ID sản phẩm
        public int? CategorySizeId { get; set; } // ID kích thước (nếu có)
        public string ProductName { get; set; } // Tên sản phẩm
        public decimal UnitPrice { get; set; } // Giá đơn vị
        public int Quantity { get; set; } // Số lượng
        public string ImageUrl { get; set; } // URL hình ảnh sản phẩm

        public Order Order { get; set; } // Navigation property
        public Product Product { get; set; } 
    }
}
