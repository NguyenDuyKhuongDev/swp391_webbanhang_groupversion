using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.DAO;
using OnlineShop.Models;
using OnlineShop.ViewModel;
using System.Security.Claims;

namespace OnlineShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartDAO _cartDao;
        private readonly IProductDAO _productDao;
        private readonly IVNPayDAO _vnpayDao;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderDAO _orderDao;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserAddressDAO _userAddressDao;

        public CartController(
            ICartDAO cartDao,
            IProductDAO productDao,
            IVNPayDAO vnpayDao,
            IOrderDAO orderDao,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            IUserAddressDAO userAddressDao)
        {
            _cartDao = cartDao;
            _productDao = productDao;
            _vnpayDao = vnpayDao;
            _orderDao = orderDao;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _userAddressDao = userAddressDao;
        }

        // Hiển thị giỏ hàng
        public async Task<IActionResult> CartDetail()
        {
            var cart = await _cartDao.GetCartAsync();
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ hàng
        public async Task<IActionResult> AddToCart(int productId, int? categorySizeId, int quantity = 1, bool isAjax = false)
        {
            var product = await _productDao.GetProductByIdAsync(productId);
            if (product == null)
            {
                return HandleError("Sản phẩm không tồn tại.", isAjax, "Index", "Home");
            }

            string sizeName = null;
            if (categorySizeId.HasValue)
            {
                var size = product.ProductSizes.FirstOrDefault(ps => ps.CategorySizeID == categorySizeId.Value);
                if (size == null || size.QuantityBySize <= 0)
                {
                    return HandleError("Kích thước không tồn tại hoặc hết hàng.", isAjax, "Details", "Home", new { id = productId });
                }
                sizeName = size.CategorySize.CategorySizeName;
            }

            var cartItem = new CartItemViewModel
            {
                Id = categorySizeId.HasValue ? $"{productId}_{categorySizeId}" : productId.ToString(),
                ProductId = productId,
                CategorySizeId = categorySizeId,
                ProductName = product.ProductName + (sizeName != null ? $" ({sizeName})" : ""),
                UnitPrice = (decimal)product.ProductPrice,
                Quantity = quantity,
                ImageUrl = product.ProductImage
            };

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

        // Thêm sản phẩm vào giỏ hàng và chuyển đến trang thanh toán

        public async Task<IActionResult> AddToCartAndCheckout(int productId, int? categorySizeId, int quantity = 1)
        {
            var product = await _productDao.GetProductByIdAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

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

            var cartItem = new CartItemViewModel
            {
                Id = categorySizeId.HasValue ? $"{productId}_{categorySizeId}" : productId.ToString(),
                ProductId = productId,
                CategorySizeId = categorySizeId,
                ProductName = product.ProductName + (sizeName != null ? $" ({sizeName})" : ""),
                UnitPrice = (decimal)product.ProductPrice,
                Quantity = quantity,
                ImageUrl = product.ProductImage
            };

            bool added = await _cartDao.AddToCartAsync(cartItem, productId, categorySizeId);
            if (!added)
            {
                TempData["Error"] = "Số lượng vượt quá tồn kho hoặc có lỗi xảy ra.";
                return RedirectToAction("Details", "Home", new { id = productId });
            }

            return RedirectToAction("CheckoutConfirmation");
        }

        // Cập nhật số lượng sản phẩm trong giỏ hàng

        public async Task<IActionResult> UpdateQuantity(string productId, int quantity)
        {
            var (success, message) = await _cartDao.UpdateQuantityAsync(productId, quantity);
            if (Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                if (success)
                {
                    var cart = await _cartDao.GetCartAsync();
                    var item = cart.FirstOrDefault(x => x.Id == productId);
                    decimal unitPrice = item?.UnitPrice ?? 0;
                    return Json(new { success = true, message, unitPrice });
                }
                return Json(new { success = false, message });
            }

            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction("CartDetail");
        }

        // Xóa sản phẩm khỏi giỏ hàng

        public async Task<IActionResult> RemoveFromCart(string productId)
        {
            await _cartDao.RemoveFromCartAsync(productId);
            TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToAction("CartDetail");
        }

        // Hiển thị trang xác nhận thanh toán
        public async Task<IActionResult> CheckoutConfirmation()
        {
            var cart = await _cartDao.GetCartAsync();
            if (!cart.Any())
            {
                TempData["Message"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("CartDetail");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Message"] = "Vui lòng đăng nhập để tiếp tục.";
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userManager.FindByIdAsync(userId);
            var addresses = await _userAddressDao.GetAddressesByUserIdAsync(userId);
            var addressViewModels = addresses.Select(address =>
            {
                var parts = address.Address?.Split(", ") ?? new string[] { "", "", "" };
                return new UserAddressViewModel
                {
                    Id = address.Id,
                    UserId = address.UserId,
                    Province = parts.Length > 0 ? parts[0] : "",
                    District = parts.Length > 1 ? parts[1] : "",
                    Ward = parts.Length > 2 ? parts[2] : "",
                    AddressDetail = address.AddressDetail,
                    IsDefault = address.IsDefault,
                    FullName = user?.FullName,
                    PhoneNumber = user?.PhoneNumber
                };
            }).ToList();

            var cartItemViewModels = cart.Select(item => new CartItemViewModel
            {
                Id = item.Id,
                ProductId = item.ProductId,
                CategorySizeId = item.CategorySizeId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                ImageUrl = item.ImageUrl
            }).ToList();

            int? selectedAddressId = HttpContext.Session.GetInt32("SelectedAddressId");
            if (selectedAddressId.HasValue)
            {
                var selectedAddress = addresses.FirstOrDefault(a => a.Id == selectedAddressId.Value);
                if (selectedAddress == null)
                {
                    HttpContext.Session.Remove("SelectedAddressId");
                    selectedAddressId = null;
                }
            }

            var model = new CheckoutConfirmationViewModel
            {
                CartItems = cartItemViewModels,
                TotalAmount = cartItemViewModels.Sum(item => item.TotalPrice),
                Addresses = addressViewModels,
                SelectedAddressId = selectedAddressId ?? addressViewModels.FirstOrDefault(a => a.IsDefault)?.Id,
                Message = TempData["Message"]?.ToString(),
                IsSuccess = TempData["IsSuccess"]?.ToString() == "true"
            };

            return View(model);
        }
        // Chọn địa chỉ giao hàng

        public async Task<IActionResult> SelectAddress(int selectedAddressId, string returnUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để chọn địa chỉ." });
                }
                TempData["Message"] = "Vui lòng đăng nhập để chọn địa chỉ.";
                TempData["IsSuccess"] = "false";
                return RedirectToAction("Login", "Auth");
            }

            var address = await _userAddressDao.GetAddressByIdAsync(selectedAddressId, userId);
            if (address == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Địa chỉ không hợp lệ hoặc không thuộc về bạn." });
                }
                TempData["Message"] = "Địa chỉ không hợp lệ hoặc không thuộc về bạn.";
                TempData["IsSuccess"] = "false";
            }
            else
            {
                HttpContext.Session.SetInt32("SelectedAddressId", selectedAddressId);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "Đã chọn địa chỉ giao hàng." });
                }
                TempData["Message"] = "Đã chọn địa chỉ giao hàng.";
                TempData["IsSuccess"] = "true";
            }

            return Redirect(returnUrl ?? Url.Action("CheckoutConfirmation"));
        }

        // Thanh toán đơn hàng

        public async Task<IActionResult> Checkout(int? selectedAddressId)
        {
            var cart = await _cartDao.GetCartAsync();
            if (!cart.Any())
            {
                TempData["Message"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("CartDetail");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Message"] = "Vui lòng đăng nhập để thanh toán.";
                return RedirectToAction("Login", "Auth");
            }

            if (!selectedAddressId.HasValue)
            {
                TempData["Message"] = "Vui lòng chọn địa chỉ giao hàng.";
                return RedirectToAction("CheckoutConfirmation");
            }

            var address = await _userAddressDao.GetAddressByIdAsync(selectedAddressId.Value, userId);
            if (address == null)
            {
                TempData["Message"] = "Địa chỉ giao hàng không hợp lệ.";
                return RedirectToAction("CheckoutConfirmation");
            }

            var orderId = DateTime.Now.Ticks.ToString();
            var totalAmount = cart.Sum(item => item.TotalPrice);
            var orderInfo = $"Order_{orderId}";

            var order = new Models.Order
            {
                OrderId = orderId,
                Amount = totalAmount,
                OrderInfo = orderInfo,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                UserId = userId,
                DeliveryAddress = $"{address.Address}, {address.AddressDetail}"
            };

            order.OrderItems = cart.Select(item => new OrderDetail
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = orderId,
                ProductId = item.ProductId,
                CategorySizeId = item.CategorySizeId,
                ProductName = item.ProductName,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                ImageUrl = item.ImageUrl
            }).ToList();

            await _orderDao.SaveOrderAsync(order);
            HttpContext.Session.SetString("CurrentOrderId", orderId);
            HttpContext.Session.Remove("SelectedAddressId");

            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var paymentUrl = _vnpayDao.CreatePaymentUrl(totalAmount, orderId, orderInfo, ipAddress);
            return Redirect(paymentUrl);
        }

        // Xử lý kết quả thanh toán từ VNPay
        public async Task<IActionResult> Return()
        {
            var queryParams = Request.Query.ToDictionary(k => k.Key, k => k.Value.ToString());
            if (!_vnpayDao.ValidateResponse(queryParams))
            {
                TempData["Message"] = "Giao dịch không hợp lệ!";
                return RedirectToAction("CartDetail");
            }

            var orderId = queryParams["vnp_TxnRef"];
            var responseCode = queryParams["vnp_ResponseCode"];
            var vnpAmount = Convert.ToInt64(queryParams["vnp_Amount"]) / 100;

            var order = await _orderDao.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                TempData["Message"] = "Đơn hàng không tồn tại!";
                return RedirectToAction("CartDetail");
            }

            if (order.Amount != vnpAmount)
            {
                TempData["Message"] = "Số tiền không khớp!";
                return RedirectToAction("CartDetail");
            }

            if (order.Status != "Pending")
            {
                TempData["Message"] = "Đơn hàng đã được xử lý!";
                return RedirectToAction("CartDetail");
            }

            if (responseCode == "00")
            {
                order.Status = "Success";
                await _cartDao.ClearCartAsync();

                // Trừ số lượng sản phẩm trong ProductSize
                foreach (var item in order.OrderItems)
                {
                    if (item.CategorySizeId.HasValue)
                    {
                        var productSize = await _productDao.GetProductSizeAsync(item.ProductId, item.CategorySizeId.Value);
                        if (productSize != null)
                        {
                            productSize.QuantityBySize -= item.Quantity;
                            if (productSize.QuantityBySize < 0)
                                productSize.QuantityBySize = 0; // Đảm bảo không âm
                            await _productDao.UpdateProductSizeAsync(productSize);
                        }
                    }
                }

                TempData["Message"] = "Thanh toán thành công!";
                TempData["IsSuccess"] = "true";
            }
            else
            {
                order.Status = "Failed";
                TempData["Message"] = $"Thanh toán thất bại! Mã lỗi: {responseCode}";
                TempData["IsSuccess"] = "false";
            }

            await _orderDao.UpdateOrderAsync(order);
            return RedirectToAction("OrderConfirmation", new { orderId });
        }

        // Hiển thị trang xác nhận đơn hàng
        public async Task<IActionResult> OrderConfirmation(string orderId)
        {
            var order = await _orderDao.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                TempData["Message"] = "Đơn hàng không tồn tại!";
                return RedirectToAction("CartDetail");
            }

            var user = await _userManager.FindByIdAsync(order.UserId); 
            var model = new OrderConfirmationViewModel
            {
                OrderId = order.OrderId,
                Amount = order.Amount,
                OrderInfo = order.OrderInfo,
                CreatedDate = order.CreatedDate,
                OrderItems = order.OrderItems,
                DeliveryAddress = order.DeliveryAddress,
                IsSuccess = order.Status == "Success",
                Message = TempData["Message"]?.ToString(),
                CustomerName = user?.FullName, // Gán tên khách hàng
                PhoneNumber = user?.PhoneNumber // Gán số điện thoại
            };

            return View(model);
        }

        // Phương thức tiện ích xử lý lỗi
        private IActionResult HandleError(string message, bool isAjax, string action, string controller, object routeValues = null)
        {
            if (isAjax)
            {
                return Json(new { success = false, message });
            }
            TempData["Error"] = message;
            return RedirectToAction(action, controller, routeValues);
        }

        // Phương thức tiện ích xử lý thành công
        private IActionResult HandleSuccess(string message, bool isAjax, string action, string controller, object routeValues = null)
        {
            if (isAjax)
            {
                return Json(new { success = true, message });
            }
            TempData["Success"] = message;
            return RedirectToAction(action, controller, routeValues);
        }
    }
}
