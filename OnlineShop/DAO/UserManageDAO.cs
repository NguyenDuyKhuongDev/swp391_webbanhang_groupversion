using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using OfficeOpenXml;

namespace OnlineShop.DAO
{
    public class UserManageDAO : IUserManageDAO
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserManageDAO(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // 1. Phương thức lấy danh sách Customer - giữ nguyên signature
        public async Task<(List<ApplicationUser> users, int totalCount)> GetCustomersAsync(
            int pageIndex = 1,
            int pageSize = 2,
            string searchTerm = null,
            string status = null,
            string dateRange = null,
            string sortOrder = null)
        {
            // Lấy userIds theo role
            var userIds = await GetUserIdsByRoleAsync("Customer");
            if (userIds.Count == 0)
                return (new List<ApplicationUser>(), 0);

            // Tạo truy vấn cơ bản
            var query = _context.Users
                .Where(u => userIds.Contains(u.Id))
                .AsQueryable();

            // Áp dụng bộ lọc và phân trang
            return await ApplyFiltersAndPaginationAsync(
                query, pageIndex, pageSize, searchTerm, status, dateRange, sortOrder);
        }

        // 2. Phương thức mới cho Employee
        public async Task<(List<ApplicationUser> users, int totalCount)> GetEmployeesAsync(
            int pageIndex = 1,
            int pageSize = 10,
            string searchTerm = null,
            string status = null,
            string dateRange = null,
            string sortOrder = null)
        {
            // Lấy userIds theo role
            var userIds = await GetUserIdsByRoleAsync("Employee");
            if (userIds.Count == 0)
                return (new List<ApplicationUser>(), 0);

            // Tạo truy vấn cơ bản
            var query = _context.Users
                .Where(u => userIds.Contains(u.Id))
                .AsQueryable();

            // Áp dụng bộ lọc và phân trang
            return await ApplyFiltersAndPaginationAsync(
                query, pageIndex, pageSize, searchTerm, status, dateRange, sortOrder);
        }

        // 3. Phương thức private để lấy userIds theo role
        private async Task<List<string>> GetUserIdsByRoleAsync(string roleName)
        {
            // Lấy ID của role
            var roleId = await _roleManager.Roles
                .Where(r => r.Name == roleName)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(roleId))
                return new List<string>();

            // Lấy danh sách userId thuộc role này
            return await _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId)
                .ToListAsync();
        }

        // 4. Phương thức private để áp dụng các bộ lọc và phân trang
        private async Task<(List<ApplicationUser> users, int totalCount)> ApplyFiltersAndPaginationAsync(
            IQueryable<ApplicationUser> query,
            int pageIndex,
            int pageSize,
            string searchTerm,
            string status,
            string dateRange,
            string sortOrder)
        {
            // Áp dụng tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(searchTerm)) ||
                    u.Email.ToLower().Contains(searchTerm) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm))
                );
            }

            // Áp dụng lọc theo trạng thái
            switch (status)
            {
                case "verified":
                    query = query.Where(u => u.EmailConfirmed);
                    break;
                case "unverified":
                    query = query.Where(u => !u.EmailConfirmed);
                    break;
                case "locked":
                    query = query.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);
                    break;
                case "active":
                    query = query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow);
                    break;
            }

            // Áp dụng lọc theo thời gian đăng ký
            if (!string.IsNullOrEmpty(dateRange))
            {
                var today = DateTime.Today;
                switch (dateRange)
                {
                    case "today":
                        query = query.Where(u => u.CreatedAt.Date == today);
                        break;
                    case "7days":
                        query = query.Where(u => u.CreatedAt.Date >= today.AddDays(-7));
                        break;
                    case "30days":
                        query = query.Where(u => u.CreatedAt.Date >= today.AddDays(-30));
                        break;
                    case "90days":
                        query = query.Where(u => u.CreatedAt.Date >= today.AddDays(-90));
                        break;
                    case "thisyear":
                        query = query.Where(u => u.CreatedAt.Year == today.Year);
                        break;
                }
            }

            // Đếm tổng số bản ghi thỏa mãn điều kiện lọc
            var totalCount = await query.CountAsync();

            // Áp dụng sắp xếp
            switch (sortOrder)
            {
                case "name_asc":
                    query = query.OrderBy(u => u.FullName);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(u => u.FullName);
                    break;
                case "date_asc":
                    query = query.OrderBy(u => u.CreatedAt);
                    break;
                case "date_desc":
                    query = query.OrderByDescending(u => u.CreatedAt);
                    break;
                default:
                    query = query.OrderByDescending(u => u.CreatedAt);
                    break;
            }

            // Áp dụng phân trang
            var users = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        // Các phương thức khác giữ nguyên
        public async Task<bool> LockUserAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Kiểm tra nếu người dùng có thuộc role Customer không
            if (!await IsCustomerOrEmployeeAsync(userId))
                return false;

            // CHÚ Ý: Bật LockoutEnabled trước khi thiết lập thời gian khóa
            await _userManager.SetLockoutEnabledAsync(user, true);

            // Sau đó mới thiết lập thời gian khóa
            var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

            // Thêm: Đăng xuất người dùng nếu đang đăng nhập
            if (user.UserName == "tonnahe171051")
            {
                await _signInManager.SignOutAsync();
            }

            return result.Succeeded;
        }

        public async Task<bool> UnlockUserAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            // Kiểm tra nếu người dùng có thuộc role Customer không
            if (!await IsCustomerOrEmployeeAsync(userId))
                return false;

            // Mở khóa tài khoản
            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            return result.Succeeded;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _context.Users
                .Include(u => u.Addresses)
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> IsCustomerOrEmployeeAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            return await _userManager.IsInRoleAsync(user, "Customer") ||
                   await _userManager.IsInRoleAsync(user, "Employee");
        }

        public async Task<(bool Success, Dictionary<string, string> Errors)> CreateEmployeeAsync(RegisterModel model)
        {
            var errors = new Dictionary<string, string>();

            // Kiểm tra username đã tồn tại chưa
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                errors.Add("Username", "tên người dùng đã tồn tại.");
                return (false, errors);
            }

            // Kiểm tra email đã đăng ký chưa
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
            {
                errors.Add("Email", "Email đã được đăng kí.");
                return (false, errors);
            }

            // Tạo user mới
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                user.CreatedAt = DateTime.UtcNow;
                user.EmailConfirmed = true;
                // Tạo role Customer nếu chưa tồn tại
                if (!await _roleManager.RoleExistsAsync(UserRoles.Employee))
                    await _roleManager.CreateAsync(new IdentityRole(UserRoles.Employee));

                // Gán role Customer cho user
                if (await _roleManager.RoleExistsAsync(UserRoles.Employee))
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.Employee);
                }

                return (true, null);
            }
            else
            {
                // Xử lý các lỗi khi tạo user
                foreach (var error in result.Errors)
                {
                    if (error.Description.Contains("password", StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add("Password", error.Description);
                    }
                    else if (error.Description.Contains("username", StringComparison.OrdinalIgnoreCase) ||
                             error.Description.Contains("user name", StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add("Username", error.Description);
                    }
                    else if (error.Description.Contains("email", StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add("Email", error.Description);
                    }
                    else
                    {
                        errors.Add(string.Empty, error.Description);
                    }
                }

                return (false, errors);
            }
        }

        public async Task<(int SuccessCount, List<EmployeeImportResult> Results)> ImportEmployeesFromExcel(IFormFile file)
        {
            var results = new List<EmployeeImportResult>();
            int successCount = 0;

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // Lấy sheet đầu tiên
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        // Bỏ qua dòng trống
                        if (string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text) &&
                            string.IsNullOrWhiteSpace(worksheet.Cells[row, 2].Text) &&
                            string.IsNullOrWhiteSpace(worksheet.Cells[row, 3].Text))
                            continue;

                        // Đọc dữ liệu từ file Excel
                        var model = new RegisterModel
                        {
                            Username = worksheet.Cells[row, 1].Text.Trim(),
                            Email = worksheet.Cells[row, 2].Text.Trim(),
                            Password = worksheet.Cells[row, 3].Text.Trim()
                        };

                        var importResult = new EmployeeImportResult
                        {
                            RowNumber = row,
                            Username = model.Username,
                            Email = model.Email,
                            Success = false
                        };

                        var (success, errors) = await CreateEmployeeAsync(model);

                        if (success)
                        {
                            successCount++;
                            importResult.Success = true;
                        }
                        else
                        {
                            importResult.Errors = errors;
                        }

                        results.Add(importResult);
                    }
                }
            }

            return (successCount, results);
        }

        // Tạo file template Excel
        public byte[] GenerateExcelTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Employees");

                // Thiết lập header
                worksheet.Cells[1, 1].Value = "Username";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Password";

                // Định dạng header
                using (var range = worksheet.Cells["A1:C1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Thêm validation và ghi chú
                worksheet.Cells["A2:A100"].AddComment("Tên đăng nhập không được trùng lặp", "System");
                worksheet.Cells["B2:B100"].AddComment("Email phải đúng định dạng và không trùng lặp", "System");
                worksheet.Cells["C2:C100"].AddComment("Mật khẩu phải có ít nhất 8 ký tự, chữ hoa, chữ thường, số và ký tự đặc biệt", "System");

                // Điều chỉnh độ rộng cột
                worksheet.Column(1).Width = 25;
                worksheet.Column(2).Width = 35;
                worksheet.Column(3).Width = 30;

                // Thêm vài dòng mẫu
                worksheet.Cells[2, 1].Value = "employee1";
                worksheet.Cells[2, 2].Value = "employee1@example.com";
                worksheet.Cells[2, 3].Value = "Password1!";

                worksheet.Cells[3, 1].Value = "employee2";
                worksheet.Cells[3, 2].Value = "employee2@example.com";
                worksheet.Cells[3, 3].Value = "Password2@";

                return package.GetAsByteArray();
            }
        }
    }
}