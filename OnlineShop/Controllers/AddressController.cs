using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.DAO;
using OnlineShop.Models;
using OnlineShop.ViewModel;
using System.Security.Claims;

namespace OnlineShop.Controllers
{
    public class AddressController : Controller
    {
        private readonly IUserAddressDAO _userAddressDao;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressController(IUserAddressDAO userAddressDao, UserManager<ApplicationUser> userManager)
        {
            _userAddressDao = userAddressDao;
            _userManager = userManager;
        }

        // Hiển thị form thêm địa chỉ mới
        [HttpGet]
        public IActionResult AddAddress()
        {
            return View(new UserAddressViewModel());
        }

        // Xử lý thêm địa chỉ mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(UserAddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = "Validation failed: " + string.Join(", ", errors);
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Please log in to add an address.";
                return RedirectToAction("Login", "Auth");
            }

            var address = MapToUserAddress(model, userId);

            // Nếu địa chỉ mới được đặt làm mặc định, cập nhật các địa chỉ khác
            if (address.IsDefault)
            {
                await _userAddressDao.ClearDefaultAddressAsync(userId);
            }

            try
            {
                await _userAddressDao.AddAddressAsync(address);
                TempData["Success"] = "Address added successfully.";
                return RedirectToAction("CheckoutConfirmation", "Cart");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error adding address: {ex.Message}");
                return View(model);
            }
        }

        // Hiển thị danh sách địa chỉ của người dùng
        [HttpGet]
        public async Task<IActionResult> ManageAddresses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Please log in to manage addresses.";
                return RedirectToAction("Login", "Auth");
            }

            var user = await _userManager.FindByIdAsync(userId);
            var addresses = await _userAddressDao.GetAddressesByUserIdAsync(userId);
            var viewModels = addresses.Select(address =>
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

            return View(viewModels);
        }

        // Hiển thị form chỉnh sửa địa chỉ
        [HttpGet]
        public async Task<IActionResult> EditAddress(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Please log in to edit an address.";
                return RedirectToAction("Login", "Auth");
            }

            var address = await _userAddressDao.GetAddressByIdAsync(id, userId);
            if (address == null)
            {
                TempData["Error"] = "Address not found or does not belong to you.";
                return RedirectToAction("CheckoutConfirmation", "Cart");
            }

            var viewModel = await MapToViewModelAsync(address);
            return View(viewModel);
        }

        // Xử lý chỉnh sửa địa chỉ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(UserAddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Error"] = "Validation failed: " + string.Join(", ", errors);
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Please log in to edit an address.";
                return RedirectToAction("Login", "Auth");
            }

            var address = MapToUserAddress(model, userId);

            // Nếu địa chỉ được đặt làm mặc định, cập nhật các địa chỉ khác
            if (address.IsDefault)
            {
                await _userAddressDao.ClearDefaultAddressAsync(userId);
            }

            try
            {
                await _userAddressDao.UpdateAddressAsync(address);
                TempData["Success"] = "Address updated successfully.";
                return RedirectToAction("CheckoutConfirmation", "Cart");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating address: {ex.Message}");
                return View(model);
            }
        }

        // Xóa địa chỉ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Please log in to delete an address.";
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var address = await _userAddressDao.GetAddressByIdAsync(id, userId);
                if (address == null)
                {
                    TempData["Error"] = "Address not found or does not belong to you.";
                    return RedirectToAction("ManageAddresses");
                }

                // Nếu địa chỉ bị xóa là địa chỉ đang được chọn, xóa session SelectedAddressId
                int? selectedAddressId = HttpContext.Session.GetInt32("SelectedAddressId");
                if (selectedAddressId.HasValue && selectedAddressId == id)
                {
                    HttpContext.Session.Remove("SelectedAddressId");
                }

                await _userAddressDao.DeleteAddressAsync(id, userId);
                TempData["Success"] = "Address deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting address: {ex.Message}";
            }
            return RedirectToAction("CheckoutConfirmation", "Cart");
        }

        // Ánh xạ từ ViewModel sang Model
        private UserAddress MapToUserAddress(UserAddressViewModel model, string userId)
        {
            return new UserAddress
            {
                Id = model.Id,
                UserId = userId,
                Address = $"{model.Province}, {model.District}, {model.Ward}",
                AddressDetail = model.AddressDetail,
                IsDefault = model.IsDefault
            };
        }

        // Ánh xạ từ Model sang ViewModel
        private async Task<UserAddressViewModel> MapToViewModelAsync(UserAddress address)
        {
            var user = await _userManager.FindByIdAsync(address.UserId);
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
        }
    }
}
