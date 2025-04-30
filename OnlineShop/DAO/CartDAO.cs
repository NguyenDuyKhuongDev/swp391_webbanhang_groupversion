using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using OnlineShop.ViewModel;
using System.Collections.Generic;
using System.Text.Json;

namespace OnlineShop.DAO
{
    public class CartDAO : ICartDAO
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductDAO _productDao;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartDAO(
            IHttpContextAccessor httpContextAccessor,
            IProductDAO productDao,
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _productDao = productDao;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        private ISession Session => _httpContextAccessor.HttpContext.Session;
        private string SessionId => _httpContextAccessor.HttpContext.Session.Id;
        private HttpContext Context => _httpContextAccessor.HttpContext;

        private async Task<bool> IsCustomerAsync()
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (user == null) return false; // Guest
            return await _userManager.IsInRoleAsync(user, "Customer");
        }

        // Ánh xạ từ CartItemEntity sang CartItem
        private CartItemViewModel MapToCartItem(CartItemEntity entity)
        {
            return new CartItemViewModel
            {
                Id = entity.CategorySizeId.HasValue ? $"{entity.ProductId}_{entity.CategorySizeId}" : entity.ProductId.ToString(),
                ProductId = entity.ProductId,
                CategorySizeId = entity.CategorySizeId,
                ProductName = entity.ProductName,
                UnitPrice = entity.UnitPrice,
                Quantity = entity.Quantity,
                ImageUrl = entity.ImageUrl
            };
        }

        // Ánh xạ từ CartItem sang CartItemEntity
        private CartItemEntity MapToCartItemEntity(CartItemViewModel item, string userId)
        {
            return new CartItemEntity
            {
                UserId = userId,
                ProductId = item.ProductId,
                CategorySizeId = item.CategorySizeId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                ImageUrl = item.ImageUrl
            };
        }

        public async Task<List<CartItemViewModel>> GetCartAsync()
        {
            if (await IsCustomerAsync())
            {
                // Lấy giỏ hàng từ DB cho Customer
                var userId = _userManager.GetUserId(Context.User);
                var cartItems = await _dbContext.CartItems
                    .Where(ci => ci.UserId == userId)
                    .ToListAsync();

                return cartItems.Select(MapToCartItem).ToList();
            }
            else
            {
                // Lấy giỏ hàng từ session cho Guest
                var cartJson = Session.GetString($"Cart_{SessionId}");
                return string.IsNullOrEmpty(cartJson)
                    ? new List<CartItemViewModel>()
                    : JsonSerializer.Deserialize<List<CartItemViewModel>>(cartJson);
            }
        }

        public async Task<bool> AddToCartAsync(CartItemViewModel item, int productId, int? categorySizeId)
        {
            // Lấy thông tin sản phẩm từ cơ sở dữ liệu
            var product = await _productDao.GetProductByIdAsync(productId);
            if (product == null)
            {
                return false; // Sản phẩm không tồn tại
            }

            // Kiểm tra số lượng tồn kho
            int maxQuantity = product.ProductQuantity;
            if (categorySizeId.HasValue)
            {
                var size = product.ProductSizes.FirstOrDefault(ps => ps.CategorySizeID == categorySizeId.Value);
                if (size == null || size.QuantityBySize <= 0)
                {
                    return false; // Kích thước không tồn tại hoặc hết hàng
                }
                maxQuantity = size.QuantityBySize;
            }

            if (await IsCustomerAsync())
            {
                // Lưu vào DB cho Customer
                var userId = _userManager.GetUserId(Context.User);
                var existingItem = await _dbContext.CartItems
                    .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId && ci.CategorySizeId == categorySizeId);

                int newQuantity = item.Quantity + (existingItem?.Quantity ?? 0);
                if (newQuantity > maxQuantity)
                {
                    return false; // Số lượng vượt quá tồn kho
                }

                if (existingItem != null)
                {
                    existingItem.Quantity = newQuantity;
                }
                else
                {
                    var entity = MapToCartItemEntity(item, userId);
                    _dbContext.CartItems.Add(entity);
                }

                await _dbContext.SaveChangesAsync();
                return true;
            }
            else
            {
                // Lưu vào session cho Guest
                var cart = await GetCartAsync();
                var existingItem = cart.FirstOrDefault(x => x.Id == item.Id);

                int newQuantity = item.Quantity + (existingItem?.Quantity ?? 0);
                if (newQuantity > maxQuantity)
                {
                    return false; // Số lượng vượt quá tồn kho
                }

                if (existingItem != null)
                {
                    existingItem.Quantity = newQuantity;
                }
                else
                {
                    cart.Add(item);
                }

                Session.SetString($"Cart_{SessionId}", JsonSerializer.Serialize(cart));
                return true;
            }
        }

        public async Task<(bool success, string message)> UpdateQuantityAsync(string productId, int quantity)
        {
            if (await IsCustomerAsync())
            {
                // Cập nhật trong DB cho Customer
                var userId = _userManager.GetUserId(Context.User);
                var idParts = productId.Split('_');
                int productIdInt = int.Parse(idParts[0]);
                int? categorySizeId = idParts.Length > 1 ? int.Parse(idParts[1]) : (int?)null;

                var item = await _dbContext.CartItems
                    .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productIdInt && ci.CategorySizeId == categorySizeId);

                if (item == null)
                {
                    return (false, "Sản phẩm không tồn tại trong giỏ hàng.");
                }

                var product = await _productDao.GetProductByIdAsync(productIdInt);
                if (product == null)
                {
                    return (false, "Sản phẩm không tồn tại.");
                }

                int maxQuantity = product.ProductQuantity;
                if (categorySizeId.HasValue)
                {
                    var size = product.ProductSizes.FirstOrDefault(ps => ps.CategorySizeID == categorySizeId.Value);
                    if (size == null)
                    {
                        return (false, "Kích thước không tồn tại.");
                    }
                    maxQuantity = size.QuantityBySize;
                }

                if (quantity > maxQuantity)
                {
                    return (false, $"Số lượng vượt quá tồn kho. Tối đa: {maxQuantity}.");
                }

                if (quantity <= 0)
                {
                    _dbContext.CartItems.Remove(item);
                    await _dbContext.SaveChangesAsync();
                    return (true, "Sản phẩm đã được xóa khỏi giỏ hàng.");
                }

                item.Quantity = quantity;
                await _dbContext.SaveChangesAsync();
                return (true, "Cập nhật số lượng thành công.");
            }
            else
            {
                // Cập nhật trong session cho Guest
                var cart = await GetCartAsync();
                var item = cart.FirstOrDefault(x => x.Id == productId);
                if (item == null)
                {
                    return (false, "Sản phẩm không tồn tại trong giỏ hàng.");
                }

                var idParts = productId.Split('_');
                int productIdInt = int.Parse(idParts[0]);
                int? categorySizeId = idParts.Length > 1 ? int.Parse(idParts[1]) : (int?)null;

                var product = await _productDao.GetProductByIdAsync(productIdInt);
                if (product == null)
                {
                    return (false, "Sản phẩm không tồn tại.");
                }

                int maxQuantity = product.ProductQuantity;
                if (categorySizeId.HasValue)
                {
                    var size = product.ProductSizes.FirstOrDefault(ps => ps.CategorySizeID == categorySizeId.Value);
                    if (size == null)
                    {
                        return (false, "Kích thước không tồn tại.");
                    }
                    maxQuantity = size.QuantityBySize;
                }

                if (quantity > maxQuantity)
                {
                    return (false, $"Số lượng vượt quá tồn kho. Tối đa: {maxQuantity}.");
                }

                if (quantity <= 0)
                {
                    cart.Remove(item);
                    Session.SetString($"Cart_{SessionId}", JsonSerializer.Serialize(cart));
                    return (true, "Sản phẩm đã được xóa khỏi giỏ hàng.");
                }

                item.Quantity = quantity;
                Session.SetString($"Cart_{SessionId}", JsonSerializer.Serialize(cart));
                return (true, "Cập nhật số lượng thành công.");
            }
        }

        public async Task RemoveFromCartAsync(string productId)
        {
            if (await IsCustomerAsync())
            {
                // Xóa trong DB cho Customer
                var userId = _userManager.GetUserId(Context.User);
                var idParts = productId.Split('_');
                int productIdInt = int.Parse(idParts[0]);
                int? categorySizeId = idParts.Length > 1 ? int.Parse(idParts[1]) : (int?)null;

                var item = await _dbContext.CartItems
                    .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productIdInt && ci.CategorySizeId == categorySizeId);

                if (item != null)
                {
                    _dbContext.CartItems.Remove(item);
                    await _dbContext.SaveChangesAsync();
                }
            }
            else
            {
                // Xóa trong session cho Guest
                var cart = await GetCartAsync();
                var item = cart.FirstOrDefault(x => x.Id == productId);
                if (item != null)
                {
                    cart.Remove(item);
                    Session.SetString($"Cart_{SessionId}", JsonSerializer.Serialize(cart));
                }
            }
        }

        public async Task ClearCartAsync()
        {
            if (await IsCustomerAsync())
            {
                // Xóa trong DB cho Customer
                var userId = _userManager.GetUserId(Context.User);
                var items = _dbContext.CartItems.Where(ci => ci.UserId == userId);
                _dbContext.CartItems.RemoveRange(items);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                // Xóa trong session cho Guest
                Session.Remove($"Cart_{SessionId}");
            }
        }
    }
}