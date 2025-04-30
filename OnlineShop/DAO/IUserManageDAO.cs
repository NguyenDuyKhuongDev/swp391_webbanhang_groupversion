using OnlineShop.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineShop.DAO
{
    public interface IUserManageDAO
    {
        Task<(List<ApplicationUser> users, int totalCount)> GetCustomersAsync(
            int pageIndex = 1,
            int pageSize = 10,
            string searchTerm = null,
            string status = null,
            string dateRange = null,
            string sortOrder = null);

        Task<(List<ApplicationUser> users, int totalCount)> GetEmployeesAsync(
            int pageIndex = 1,
            int pageSize = 10,
            string searchTerm = null,
            string status = null,
            string dateRange = null,
            string sortOrder = null);

        Task<bool> LockUserAccountAsync(string userId);
        Task<bool> UnlockUserAccountAsync(string userId);
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<bool> IsCustomerOrEmployeeAsync(string userId);
        Task<(bool Success, Dictionary<string, string> Errors)> CreateEmployeeAsync(RegisterModel model);

        Task<(int SuccessCount, List<EmployeeImportResult> Results)> ImportEmployeesFromExcel(IFormFile file);

        byte[] GenerateExcelTemplate();

    }
}