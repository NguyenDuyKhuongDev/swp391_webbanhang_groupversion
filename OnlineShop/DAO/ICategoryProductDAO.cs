

using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public interface ICategoryProductDAO
    {
        Task<List<CategoryProduct>> GetAllCategoryProductsAsync();
        Task<CategoryProduct> GetCategoryProductByIdAsync(int id);
    }
}
