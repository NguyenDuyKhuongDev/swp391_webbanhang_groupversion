using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public interface IOrderDAO
    {
        Task SaveOrderAsync(Order order);
        Task<Order> GetOrderByIdAsync(string orderId);
        Task UpdateOrderAsync(Order order);
        Task<List<Order>> GetOrdersByUserIdAsync(string userId);
    }
}
