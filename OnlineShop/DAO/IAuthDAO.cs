using Microsoft.AspNetCore.Identity;
using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public interface IAuthDAO
    {
        // Phương thức login
        Task<(bool Success, string ErrorField, string ErrorMessage, ApplicationUser User)> ValidateLoginAsync(string username, string password);

        // Phương thức kiểm tra và đăng ký người dùng
        Task<(bool Success, Dictionary<string, string> Errors)> RegisterUserAsync(RegisterModel model);

        // Phương thức xác nhận email
        Task<bool> ConfirmEmailAsync(string userId, string token);

        // Phương thức quên mật khẩu
        Task<bool> InitiatePasswordResetAsync(string email, string resetLink);

        // Phương thức đặt lại mật khẩu
        Task<(bool Success, Dictionary<string, string> Errors)> ResetPasswordAsync(string email, string token, string newPassword);

        // Phương thức đăng nhập
        Task SignInUserAsync(ApplicationUser user, bool rememberMe, HttpContext httpContext);

        // Phương thức đăng xuất
        Task SignOutAsync(HttpContext httpContext);

        // Phương thức lấy danh sách roles của user
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);

        // Phương thức tạo token đặt lại mật khẩu
        Task<string> GeneratePasswordResetTokenAsync(string email);

        // Phương thức tạo token xác nhận email
        Task<string> GenerateEmailConfirmationTokenAsync(string userId);

        // Phương thức gửi email xác nhận
        Task SendConfirmationEmailAsync(string userId, string confirmationLink);
    }
}