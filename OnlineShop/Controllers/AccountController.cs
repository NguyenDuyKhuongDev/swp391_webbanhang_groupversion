using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.DAO;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserProfileDAO _userProfileDAO;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            IUserProfileDAO userProfileDAO)
        {
            _userManager = userManager;
            _userProfileDAO = userProfileDAO;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var model = await _userProfileDAO.GetUserProfileAsync(userId);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        public async Task<IActionResult> UpdateProfile()
        {
            var userId = _userManager.GetUserId(User);
            var model = await _userProfileDAO.GetUserProfileAsync(userId);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            var success = await _userProfileDAO.UpdateUserProfileAsync(model, userId);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Cập nhật thông tin thất bại! Vui lòng thử lại.");
                return View(model);
            }

            TempData["StatusMessage"] = "Thông tin được cập nhật thành công!";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            // Kiểm tra mật khẩu hiện tại
            var isCurrentPasswordValid = await _userProfileDAO.CheckCurrentPasswordAsync(userId, model.Password);
            if (!isCurrentPasswordValid)
            {
                ModelState.AddModelError("Password", "Mật khẩu hiện tại không chính xác.");
                return View(model);
            }

            // Thực hiện đổi mật khẩu
            var (success, errors) = await _userProfileDAO.ChangePasswordAsync(model, userId);

            if (!success)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View(model);
            }

            TempData["StatusMessage"] = "Mật khẩu đã được thay đổi thành công!";
            return RedirectToAction(nameof(ChangePassword));
        }
    }
}