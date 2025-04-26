using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public class CategorySizeDAO : ICategorySizeDAO
    {
        private readonly ApplicationDbContext _context;

        public CategorySizeDAO(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategorySize>> GetAllCategorySizesAsync()
        {
            return await _context.CategorySizes.OrderBy(p => p.CategorySizeID).ToListAsync();
        }
        public async Task<CategorySize> GetCategorySizeByIdAsync(int id)
        {
            return await _context.CategorySizes.FindAsync(id);
        }

     
    }
}
