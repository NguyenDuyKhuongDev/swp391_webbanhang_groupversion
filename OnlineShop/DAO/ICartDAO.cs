using OnlineShop.Models;


namespace OnlineShop.DAO
{
    public interface ICartDAO
    {
        Task<List<CartItem>> GetCartAsync();
        Task<bool> AddToCartAsync(CartItem item, int productId, int? categorySizeId);
        Task<(bool success, string message)> UpdateQuantityAsync(string productId, int quantity);
        Task RemoveFromCartAsync(string productId);
        Task ClearCartAsync();
    }
}