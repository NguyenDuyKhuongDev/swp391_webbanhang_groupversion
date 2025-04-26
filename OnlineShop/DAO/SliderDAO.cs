using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public class SliderDAO : ISliderDAO
    {
        private readonly ApplicationDbContext _dbContext;

        public SliderDAO(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Slider>> GetAllSlidersAsync(bool onlyActive = false)
        {
            var query = _dbContext.Sliders.AsQueryable();
            if (onlyActive)
            {
                query = query.Where(s => s.IsActive);
            }
            return await query.OrderBy(s => s.DisplayOrder).ToListAsync();
        }

        public async Task<Slider> GetSliderByIdAsync(int id)
        {
            return await _dbContext.Sliders.FindAsync(id);
        }

        public async Task AddSliderAsync(Slider slider)
        {
            await _dbContext.Sliders.AddAsync(slider);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateSliderAsync(Slider slider)
        {
            _dbContext.Sliders.Update(slider);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteSliderAsync(int id)
        {
            var slider = await GetSliderByIdAsync(id);
            if (slider != null)
            {
                _dbContext.Sliders.Remove(slider);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
