using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public interface ISliderDAO
    {
        Task<List<Slider>> GetAllSlidersAsync(bool onlyActive = false);
        Task<Slider> GetSliderByIdAsync(int id);
        Task AddSliderAsync(Slider slider);
        Task UpdateSliderAsync(Slider slider);
        Task DeleteSliderAsync(int id);
    }
}
