using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public class FeedbackDAO : IFeedbackDAO
    {
        private readonly Data.ApplicationDbContext _context;

        public FeedbackDAO(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Feedback>> GetCommentsByProductIdAsync(int productId)
        {
            if (productId == 0)
            {
                return await _context.Feedbacks
                    .Include(c => c.Product)
                    .ToListAsync();
            }
            return await _context.Feedbacks
                .Include(c => c.Product)
                .Where(c => c.ProductID == productId)
                .ToListAsync();
        }

        public async Task<Feedback> GetCommentByIdAsync(int commentId)
        {
            return await _context.Feedbacks
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.CommentID == commentId);
        }

        public async Task AddCommentAsync(Feedback comment)
        {
            _context.Feedbacks.Add(comment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCommentAsync(Feedback comment)
        {
            _context.Feedbacks.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _context.Feedbacks.FindAsync(commentId);
            if (comment != null)
            {
                _context.Feedbacks.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasPurchasedProductAsync(string userId, int productId)
        {
            return await _context.OrderItems
                .Join(_context.Orders,
                      oi => oi.OrderId,
                      o => o.OrderId,
                      (oi, o) => new { OrderItem = oi, Order = o })
                .AnyAsync(x => x.Order.UserId == userId
                            && x.OrderItem.ProductId == productId
                            && x.Order.Status == "Completed");
        }
    }
}
