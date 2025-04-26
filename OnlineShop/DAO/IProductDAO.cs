
using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public interface IProductDAO
    {
        Task<(List<Product> Products, int TotalItems)> GetProductsAsync(int page, int pageSize, string? searchName = null, bool? status = null, string? priceSort = null, int? categoryId = null);
        Task<Product?> GetProductByIdAsync(int productId);
        Task<bool> CheckNameProductAsync(string productName, int? productId = null);
        Task<bool> AddProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product, Dictionary<int, int> sizeQuantities);
        Task<bool> ToggleProductStatusAsync(int productId);

    }
}
