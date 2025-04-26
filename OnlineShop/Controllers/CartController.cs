using Microsoft.AspNetCore.Mvc;
using OnlineShop.DAO;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartDAO _cartDao;
        private readonly IProductDAO _productDao;
        private readonly IVNPayDAO _vnpayDao;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartController(
            ICartDAO cartDao,
            IProductDAO productDao,
            IVNPayDAO vnpayDao,
            IHttpContextAccessor httpContextAccessor)
        {
            _cartDao = cartDao;
            _productDao = productDao;
            _vnpayDao = vnpayDao;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> CartDetail()
        {
            var cart = await _cartDao.GetCartAsync();
            return View(cart);
        }

        public async Task<IActionResult> AddToCart(int productId, int? categorySizeId, int quantity = 1, bool isAjax = false)
        {
            // Lấy thông tin sản phẩm từ cơ sở dữ liệu
            var product = await _productDao.GetProductByIdAsync(productId);
            if (product == null)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Sản phẩm không tồn tại." });
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra kích thước nếu có
            string sizeName = null;
            if (categorySizeId.HasValue)
            {
                var size = product.ProductSizes.FirstOrDefault(ps => ps.CategorySizeID == categorySizeId.Value);
                if (size == null || size.QuantityBySize <= 0)
                {
                    if (isAjax)
                        return Json(new { success = false, message = "Kích thước không tồn tại hoặc hết hàng." });
                    TempData["Error"] = "Kích thước không tồn tại hoặc hết hàng.";
                    return RedirectToAction("Details", "Home", new { id = productId });
                }
                sizeName = size.CategorySize.CategorySizeName;
            }

            var cartItem = new CartItem
            {
                Id = categorySizeId.HasValue ? $"{productId}_{categorySizeId}" : productId.ToString(),
                ProductId = productId,
                CategorySizeId = categorySizeId,
                ProductName = product.ProductName + (sizeName != null ? $" ({sizeName})" : ""),
                UnitPrice = (decimal)product.ProductPrice,
                Quantity = quantity,
                ImageUrl = product.ProductImage
            };

            // Thêm vào giỏ hàng thông qua CartDAO
            bool added = await _cartDao.AddToCartAsync(cartItem, productId, categorySizeId);
            if (!added)
            {
                if (isAjax)
                    return Json(new { success = false, message = "Số lượng vượt quá tồn kho hoặc có lỗi xảy ra." });
                TempData["Error"] = "Số lượng vượt quá tồn kho hoặc có lỗi xảy ra.";
                return RedirectToAction("Details", "Home", new { id = productId });
            }

            if (isAjax)
                return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng!" });

            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("Details", "Home", new { id = productId });
        }

        public async Task<IActionResult> AddToCartAndCheckout(int productId, int? categorySizeId, int quantity = 1)
        {
            // Lấy thông tin sản phẩm từ cơ sở dữ liệu
            var product = await _productDao.GetProductByIdAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra kích thước nếu có
            string sizeName = null;
            if (categorySizeId.HasValue)
            {
                var size = product.ProductSizes.FirstOrDefault(ps => ps.CategorySizeID == categorySizeId.Value);
                if (size == null || size.QuantityBySize <= 0)
                {
                    TempData["Error"] = "Kích thước không tồn tại hoặc hết hàng.";
                    return RedirectToAction("Details", "Home", new { id = productId });
                }
                sizeName = size.CategorySize.CategorySizeName;
            }

            // Tạo CartItem
            var cartItem = new CartItem
            {
                Id = categorySizeId.HasValue ? $"{productId}_{categorySizeId}" : productId.ToString(),
                ProductId = productId,
                CategorySizeId = categorySizeId,
                ProductName = product.ProductName + (sizeName != null ? $" ({sizeName})" : ""),
                UnitPrice = (decimal)product.ProductPrice,
                Quantity = quantity,
                ImageUrl = product.ProductImage
            };

            // Thêm vào giỏ hàng thông qua CartDAO
            bool added = await _cartDao.AddToCartAsync(cartItem, productId, categorySizeId);
            if (!added)
            {
                TempData["Error"] = "Số lượng vượt quá tồn kho hoặc có lỗi xảy ra.";
                return RedirectToAction("Details", "Home", new { id = productId });
            }

            // Chuyển đến Checkout
            return RedirectToAction("Checkout");
        }

        public async Task<IActionResult> UpdateQuantity(string productId, int quantity)
        {
            var (success, message) = await _cartDao.UpdateQuantityAsync(productId, quantity);
            if (Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                // Xử lý AJAX
                if (success)
                {
                    var cart = await _cartDao.GetCartAsync();
                    var item = cart.FirstOrDefault(x => x.Id == productId);
                    decimal unitPrice = item?.UnitPrice ?? 0;
                    return Json(new { success = true, message, unitPrice });
                }
                return Json(new { success = false, message });
            }

            // Xử lý yêu cầu thông thường
            if (!success)
            {
                TempData["Error"] = message;
            }
            else if (quantity > 0)
            {
                TempData["Success"] = message;
            }
            return RedirectToAction("CartDetail");
        }

        public async Task<IActionResult> RemoveFromCart(string productId)
        {
            await _cartDao.RemoveFromCartAsync(productId);
            TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToAction("CartDetail");
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = await _cartDao.GetCartAsync();
            if (!cart.Any())
            {
                TempData["Message"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("CartDetail");
            }

            var totalAmount = cart.Sum(item => item.TotalPrice);
            var orderId = DateTime.Now.Ticks.ToString();
            var orderInfo = $"Order_{orderId}";
            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            // Lưu orderId vào Session để sử dụng sau khi thanh toán
            HttpContext.Session.SetString("CurrentOrderId", orderId);

            var paymentUrl = _vnpayDao.CreatePaymentUrl(totalAmount, orderId, orderInfo, ipAddress);
            return Redirect(paymentUrl);
        }

        public async Task<IActionResult> Return()
        {
            var queryParams = Request.Query.ToDictionary(k => k.Key, k => k.Value.ToString());

            if (!_vnpayDao.ValidateResponse(queryParams))
            {
                TempData["Message"] = "Giao dịch không hợp lệ!";
                return RedirectToAction("CartDetail");
            }

            var responseCode = queryParams["vnp_ResponseCode"];
            if (responseCode == "00")
            {
                await _cartDao.ClearCartAsync();
                TempData["Message"] = "Thanh toán thành công! Giỏ hàng đã được xóa.";
            }
            else
            {
                TempData["Message"] = "Thanh toán thất bại! Vui lòng thử lại.";
            }

            return RedirectToAction("CartDetail");
        }
    }
}
