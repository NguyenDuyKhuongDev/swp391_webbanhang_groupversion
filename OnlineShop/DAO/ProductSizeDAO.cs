using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public class ProductSizeDAO : IProductSizeDAO
    {
        private readonly ApplicationDbContext _context;

        public ProductSizeDAO(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductSize>> GetAllProductSizesAsync()
        {
            return await _context.ProductSizes.ToListAsync();
        }

        public async Task<ProductSize> GetProductSizeByIdAsync(int id)
        {
            return await _context.ProductSizes.FindAsync(id);
        }

        public async Task<bool> AddProductSizesAsync(int productId, Dictionary<int, int> sizeQuantities)
        {
            try
            {
                var productSizes = sizeQuantities.Select(size => new ProductSize
                {
                    ProductID = productId,
                    CategorySizeID = size.Key,
                    QuantityBySize = size.Value
                }).ToList();

                await _context.ProductSizes.AddRangeAsync(productSizes);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductSizesAsync(int productId, Dictionary<int, int> sizeQuantities)
        {
            try
            {
                // Xóa các ProductSize hiện tại của sản phẩm
                var existingSizes = await _context.ProductSizes
                    .Where(ps => ps.ProductID == productId)
                    .ToListAsync();
                _context.ProductSizes.RemoveRange(existingSizes);

                // Thêm các ProductSize mới
                var productSizes = sizeQuantities.Select(size => new ProductSize
                {
                    ProductID = productId,
                    CategorySizeID = size.Key,
                    QuantityBySize = size.Value
                }).ToList();

                await _context.ProductSizes.AddRangeAsync(productSizes);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
