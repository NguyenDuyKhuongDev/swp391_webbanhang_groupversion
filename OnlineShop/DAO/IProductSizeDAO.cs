

using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public interface IProductSizeDAO
    {
        Task<List<ProductSize>> GetAllProductSizesAsync();
        Task<ProductSize> GetProductSizeByIdAsync(int id);
        Task<bool> AddProductSizesAsync(int productId, Dictionary<int, int> sizeQuantities);
        Task<bool> UpdateProductSizesAsync(int productId, Dictionary<int, int> sizeQuantities);

    }
}
