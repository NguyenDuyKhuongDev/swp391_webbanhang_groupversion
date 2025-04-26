using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;


namespace OnlineShop.DAO
{
    public class CategoryProductDAO : ICategoryProductDAO
    {
        private readonly ApplicationDbContext _context;

        public CategoryProductDAO(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryProduct>> GetAllCategoryProductsAsync()
        {
            return await _context.CategoryProducts.OrderBy(p => p.CategoryProductID).ToListAsync();
        }

        public async Task<CategoryProduct> GetCategoryProductByIdAsync(int id)
        {
            return await _context.CategoryProducts.FindAsync(id);
        }
    }
}
