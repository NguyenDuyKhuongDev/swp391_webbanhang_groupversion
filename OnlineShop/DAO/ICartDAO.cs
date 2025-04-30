using OnlineShop.Models;
using OnlineShop.ViewModel;


namespace OnlineShop.DAO
{
    public interface ICartDAO
    {
        Task<List<CartItemViewModel>> GetCartAsync();
        Task<bool> AddToCartAsync(CartItemViewModel item, int productId, int? categorySizeId);
        Task<(bool success, string message)> UpdateQuantityAsync(string productId, int quantity);
        Task RemoveFromCartAsync(string productId);
        Task ClearCartAsync();
    }
}