using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.ViewModel
{
    public class OrderConfirmationViewModel
    {
        public string OrderId { get; set; }
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DeliveryAddress { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public List<OrderDetail> OrderItems { get; set; }
    }
}
