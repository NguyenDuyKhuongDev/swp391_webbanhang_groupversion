using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public interface IFeedbackDAO
    {
        Task<List<Feedback>> GetCommentsByProductIdAsync(int productId);
        Task<Feedback> GetCommentByIdAsync(int commentId);
        Task AddCommentAsync(Feedback comment);
        Task UpdateCommentAsync(Feedback comment);
        Task DeleteCommentAsync(int commentId);
        Task<bool> HasPurchasedProductAsync(string userId, int productId);
    }
}
