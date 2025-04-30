using Microsoft.EntityFrameworkCore;
using OnlineShop.DAO;
using OnlineShop.Data;
using OnlineShop.Models;


namespace OnlineShop.DAO
{
    public class ProductDAO : IProductDAO
    {
        private readonly ApplicationDbContext _context;

        public ProductDAO(ApplicationDbContext context)
        {
            _context = context;

        }


        // Lay list san pham(bao gom search and filter)
        public async Task<(List<Product> Products, int TotalItems)> GetProductsAsync(int page, int pageSize, string? searchName = null, bool? status = null, string? priceSort = null, int? categoryId = null)
        {
            var query = _context.Products
                .Include(p => p.CategoryProduct)
                .AsQueryable();

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(searchName))
            {
                query = query.Where(p => p.ProductName.Contains(searchName));
            }

            // Lọc theo trạng thái
            if (status.HasValue)
            {
                query = query.Where(p => p.ProductStatus == status.Value);
            }

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryProductId == categoryId.Value);
            }

            // Sắp xếp theo giá
            if (!string.IsNullOrEmpty(priceSort))
            {
                if (priceSort == "asc")
                {
                    query = query.OrderBy(p => p.ProductPrice);
                }
                else if (priceSort == "desc")
                {
                    query = query.OrderByDescending(p => p.ProductPrice);
                }
            }
            else
            {
                query = query.OrderByDescending(p => p.ProductCreatedDate); // Mặc định sắp xếp theo ID giảm dần
            }

            // Tính tổng số sản phẩm
            int totalItems = await query.CountAsync();

            // Phân trang
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalItems);
        }


        public async Task<bool> CheckNameProductAsync(string productName, int? productId = null)
        {
            //return await _context.Products
            //    .AnyAsync(p => p.ProductName == productName && (productId == null || p.ProductID != productId));
            var query = _context.Products.AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(p => p.ProductID != productId.Value);
            }

            return await query.AnyAsync(p => p.ProductName == productName);
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            try
            {
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateProductAsync(Product product, Dictionary<int, int> sizeQuantities)
        {
            try
            {
                var existingProduct = await _context.Products
                    .Include(p => p.ProductSizes)
                    .FirstOrDefaultAsync(p => p.ProductID == product.ProductID);

                if (existingProduct == null)
                {
                    return false;
                }

                // Cập nhật các trường
                existingProduct.ProductName = product.ProductName;
                existingProduct.ProductDescription = product.ProductDescription;
                existingProduct.ProductPrice = product.ProductPrice;
                // Tính tổng ProductQuantity từ sizeQuantities
                existingProduct.ProductQuantity = sizeQuantities.Values.Sum();
                existingProduct.CategoryProductId = product.CategoryProductId;
                existingProduct.ProductImage = product.ProductImage;

                await _context.SaveChangesAsync();

                // Cập nhật ProductSizes
                var productSizeDAO = new ProductSizeDAO(_context);
                return await productSizeDAO.UpdateProductSizesAsync(product.ProductID, sizeQuantities);

            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleProductStatusAsync(int productId)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == productId);
                if (product == null)
                {
                    return false;
                }

                product.ProductStatus = !product.ProductStatus;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products
                .Include(p => p.CategoryProduct)
                .Include(p => p.ProductSizes)
                    .ThenInclude(ps => ps.CategorySize)
                .FirstOrDefaultAsync(p => p.ProductID == productId);
        }
        public async Task<ProductSize> GetProductSizeAsync(int productId, int categorySizeId)
        {
            return await _context.ProductSizes
                .FirstOrDefaultAsync(ps => ps.ProductID == productId && ps.CategorySizeID == categorySizeId);
        }

        public async Task UpdateProductSizeAsync(ProductSize productSize)
        {
            _context.ProductSizes.Update(productSize);
            await _context.SaveChangesAsync();
        }
    }
}
