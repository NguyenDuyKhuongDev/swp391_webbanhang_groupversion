using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineShop.DAO;
using OnlineShop.Models;
using OnlineShop.Services;
using System;
using System.Threading.Tasks;

namespace OnlineShop.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserManageDAO _userManageDAO;
        private readonly IAuthDAO _authDAO;
        private const int PAGE_SIZE = 3;

        public UserController(IUserManageDAO userManageDAO, UserManager<ApplicationUser> userManager, IAuthDAO authDAO)
        {
            _userManager = userManager;
            _userManageDAO = userManageDAO;
            _authDAO = authDAO;
        }

        public async Task<IActionResult> Customers(string searchTerm, string status,
            string dateRange, string sortOrder, int page = 1)
        {
            ViewBag.CurrentFilter = searchTerm;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentDateRange = dateRange;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParam = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.DateSortParam = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            // 2. Chuẩn bị dropdown cho Status
            ViewBag.StatusList = new List<SelectListItem> {
                new SelectListItem("Tất cả trạng thái",    "",         string.IsNullOrEmpty(status)),
                new SelectListItem("Đã xác thực",          "verified", status == "verified"),
                new SelectListItem("Chưa xác thực",        "unverified", status == "unverified"),
                new SelectListItem("Bị khóa",              "locked",   status == "locked"),
                new SelectListItem("Đang hoạt động",       "active",   status == "active"),
            };

            // 3. Chuẩn bị dropdown cho Date Range
            ViewBag.DateRangeList = new List<SelectListItem> {
                new SelectListItem("Tất cả thời gian", "",              string.IsNullOrEmpty(dateRange)),
                new SelectListItem("Hôm nay",         "today",         dateRange == "today"),
                new SelectListItem("7 ngày qua",      "7days",         dateRange == "7days"),
                new SelectListItem("30 ngày qua",     "30days",        dateRange == "30days"),
                new SelectListItem("90 ngày qua",     "90days",        dateRange == "90days"),
                new SelectListItem("Năm nay",         "thisyear",      dateRange == "thisyear"),
            };

            var result = await _userManageDAO.GetCustomersAsync(
                    page,
                    PAGE_SIZE,
                    searchTerm,
                    status,
                    dateRange,
                    sortOrder);

            var model = new PagingList<ApplicationUser>(
                result.users,
                result.totalCount,
                page,
                PAGE_SIZE);

            return View(model);
        }

        public async Task<IActionResult> Employees(string searchTerm, string status,
            string dateRange, string sortOrder, int page = 1)
        {
            ViewBag.CurrentFilter = searchTerm;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentDateRange = dateRange;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParam = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.DateSortParam = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            // 2. Chuẩn bị dropdown cho Status
            ViewBag.StatusList = new List<SelectListItem> {
                new SelectListItem("Tất cả trạng thái",    "",         string.IsNullOrEmpty(status)),
                new SelectListItem("Đã xác thực",          "verified", status == "verified"),
                new SelectListItem("Chưa xác thực",        "unverified", status == "unverified"),
                new SelectListItem("Bị khóa",              "locked",   status == "locked"),
                new SelectListItem("Đang hoạt động",       "active",   status == "active"),
            };

            // 3. Chuẩn bị dropdown cho Date Range
            ViewBag.DateRangeList = new List<SelectListItem> {
                new SelectListItem("Tất cả thời gian", "",              string.IsNullOrEmpty(dateRange)),
                new SelectListItem("Hôm nay",         "today",         dateRange == "today"),
                new SelectListItem("7 ngày qua",      "7days",         dateRange == "7days"),
                new SelectListItem("30 ngày qua",     "30days",        dateRange == "30days"),
                new SelectListItem("90 ngày qua",     "90days",        dateRange == "90days"),
                new SelectListItem("Năm nay",         "thisyear",      dateRange == "thisyear"),
            };

            var result = await _userManageDAO.GetEmployeesAsync(
                    page,
                    PAGE_SIZE,
                    searchTerm,
                    status,
                    dateRange,
                    sortOrder);

            var model = new PagingList<ApplicationUser>(
                result.users,
                result.totalCount,
                page,
                PAGE_SIZE);

            return View(model);
        }

        public async Task<IActionResult> LockAccount(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            bool isEmployee = await _userManager.IsInRoleAsync(user, "Employee");

            // Thực hiện khóa tài khoản
            var success = await _userManageDAO.LockUserAccountAsync(id);
            if (!success)
                TempData["ErrorMessage"] = "Không thể khóa tài khoản này. Vui lòng thử lại sau.";
            else
                TempData["SuccessMessage"] = $"Tài khoản {user.UserName} đã được khóa thành công.";

            if (isEmployee)
                return RedirectToAction(nameof(Employees));
            else
                return RedirectToAction(nameof(Customers));
        }

        public async Task<IActionResult> UnlockAccount(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            bool isEmployee = await _userManager.IsInRoleAsync(user, "Employee");

            // Thực hiện mở khóa tài khoản
            var success = await _userManageDAO.UnlockUserAccountAsync(id);
            if (!success)
                TempData["ErrorMessage"] = "Không thể mở khóa tài khoản này. Vui lòng thử lại sau.";
            else
                TempData["SuccessMessage"] = $"Tài khoản {user.UserName} đã được mở khóa thành công.";

            if (isEmployee)
                return RedirectToAction(nameof(Employees));
            else
                return RedirectToAction(nameof(Customers));
        }

        [HttpGet]
        public IActionResult RegisterEmployee()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterEmployee(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, errors) = await _userManageDAO.CreateEmployeeAsync(model);

                if (success)
                {
                    TempData["SuccessMessage"] = "Đã tạo tài khoản nhân viên thành công.";
                    return RedirectToAction("Employee");
                }

                if (errors != null)
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(error.Key, error.Value);
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportEmployees(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file để import.";
                return RedirectToAction("RegisterEmployee");
            }

            // Kiểm tra định dạng file
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (fileExtension != ".xlsx" && fileExtension != ".xls")
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file Excel (*.xlsx, *.xls).";
                return RedirectToAction("RegisterEmployee");
            }

            try
            {
                var (successCount, results) = await _userManageDAO.ImportEmployeesFromExcel(file);

                HttpContext.Session.SetObject("ImportResults", results);
                HttpContext.Session.SetInt32("SuccessCount", successCount);

                if (successCount > 0)
                {
                    TempData["SuccessMessage"] = $"Đã import thành công {successCount} nhân viên.";
                }

                return RedirectToAction("ImportResults");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi import: {ex.Message}";
                return RedirectToAction("RegisterEmployee");
            }
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            // Sử dụng IUserManageDAO thay vì ExcelImportService
            var fileBytes = _userManageDAO.GenerateExcelTemplate();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "EmployeeImportTemplate.xlsx");
        }

        [HttpGet]
        public IActionResult ImportResults()
        {
            // Lấy dữ liệu từ Session
            var results = HttpContext.Session.GetObject<List<EmployeeImportResult>>("ImportResults");
            ViewBag.SuccessCount = HttpContext.Session.GetInt32("SuccessCount");

            if (results == null)
            {
                return RedirectToAction("RegisterEmployee");
            }

            return View(results);
        }

        public async Task<IActionResult> SendVerificationEmail(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (user.EmailConfirmed)
            {
                TempData["InfoMessage"] = $"Email của người dùng {user.UserName} đã được xác thực trước đó.";
            }
            else
            {
                try
                {
                    // Generate email confirmation token
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // Create confirmation link
                    var confirmationLink = Url.Action("ConfirmEmail", "Auth",
                        new { userId = user.Id, token = token }, Request.Scheme);

                    // Send confirmation email
                    await _authDAO.SendConfirmationEmailAsync(user.Id, confirmationLink);

                    TempData["SuccessMessage"] = $"Đã gửi email xác thực đến {user.Email} thành công.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Không thể gửi email xác thực: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(Customers));
        }

        public async Task<IActionResult> SendPasswordResetLink(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (string.IsNullOrEmpty(user.Email))
            {
                TempData["ErrorMessage"] = "Không tìm thấy email người dùng!";
                return RedirectToAction(nameof(Employees));
            }

            try
            {
                // Generate token
                var token = await _authDAO.GeneratePasswordResetTokenAsync(user.Email);

                if (token != null)
                {
                    // Create reset link
                    var resetLink = Url.Action("ResetPassword", "Auth",
                        new { email = user.Email, token }, Request.Scheme);

                    // Send email
                    await _authDAO.InitiatePasswordResetAsync(user.Email, resetLink);

                    TempData["SuccessMessage"] = $"Link đặt lại mật khẩu đã được gửi đến {user.Email}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể tạo token đặt lại mật khẩu.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi gửi email đặt lại mật khẩu: {ex.Message}";
            }

            return RedirectToAction(nameof(Employees));
        }
    }
}