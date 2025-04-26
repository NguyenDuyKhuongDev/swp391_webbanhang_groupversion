using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public interface IUserProfileDAO
    {
        Task<UserProfileViewModel> GetUserProfileAsync(string userId);

        Task<bool> UpdateUserProfileAsync(UserProfileViewModel model, string userId);

        Task<(bool Success, IEnumerable<string> Errors)> ChangePasswordAsync(ChangePasswordModel model, string userId);

        Task<bool> CheckCurrentPasswordAsync(string userId, string currentPassword);
    }
}