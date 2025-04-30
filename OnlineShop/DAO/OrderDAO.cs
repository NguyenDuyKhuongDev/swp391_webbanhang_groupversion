using Microsoft.EntityFrameworkCore;

using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public class OrderDAO : IOrderDAO
    {
        private readonly Data.ApplicationDbContext _context;

        public OrderDAO(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task<Order> GetOrderByIdAsync(string orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }
    }
}
