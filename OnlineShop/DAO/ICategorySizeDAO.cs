using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public interface ICategorySizeDAO
    {
        Task<List<CategorySize>> GetAllCategorySizesAsync();
        //Task<ICollection<CategorySize>> GetAllCategorySizesAsync();
        Task<CategorySize> GetCategorySizeByIdAsync(int id);
    }
}
