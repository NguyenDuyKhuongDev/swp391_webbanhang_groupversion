using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.DAO
{
    public class UserProfileDAO : IUserProfileDAO
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserProfileDAO(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<UserProfileViewModel> GetUserProfileAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var address = await _context.UserAddresses
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.IsDefault)
                .FirstOrDefaultAsync();

            var model = new UserProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName ?? string.Empty,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Gender = user.Gender,
                Avatar = user.Avatar
            };

            if (address != null)
            {
                model.AddressId = address.Id;
                model.AddressDetail = address.AddressDetail;

                var addressParts = address.Address.Split(", ");
                model.Province = addressParts.Length > 0 ? addressParts[0] : string.Empty;
                model.District = addressParts.Length > 1 ? addressParts[1] : string.Empty;
                model.Ward = addressParts.Length > 2 ? addressParts[2] : string.Empty;
            }

            return model;
        }

        public async Task<bool> UpdateUserProfileAsync(UserProfileViewModel model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Cập nhật thông tin cơ bản
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            if (string.IsNullOrEmpty(model.Avatar))
            {
                user.Avatar = null;
            }
            if (model.Avatar != null)
            {
                user.Avatar = model.Avatar;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return false;
            }

            // Cập nhật hoặc tạo địa chỉ
            if (!string.IsNullOrEmpty(model.Province) &&
                !string.IsNullOrEmpty(model.District) &&
                !string.IsNullOrEmpty(model.Ward) &&
                !string.IsNullOrEmpty(model.AddressDetail))
            {
                string fullAddress = $"{model.Province}, {model.District}, {model.Ward}";

                UserAddress address;
                if (model.AddressId.HasValue)
                {
                    // Cập nhật địa chỉ hiện tại
                    address = await _context.UserAddresses.FindAsync(model.AddressId);
                    if (address != null && address.UserId == user.Id)
                    {
                        address.Address = fullAddress;
                        address.AddressDetail = model.AddressDetail;
                        address.IsDefault = true;
                    }
                    else
                    {
                        // Tạo mới nếu không tìm thấy
                        address = new UserAddress
                        {
                            UserId = user.Id,
                            Address = fullAddress,
                            AddressDetail = model.AddressDetail,
                            IsDefault = true
                        };
                        _context.UserAddresses.Add(address);
                    }
                }
                else
                {
                    // Tạo địa chỉ mới
                    address = new UserAddress
                    {
                        UserId = user.Id,
                        Address = fullAddress,
                        AddressDetail = model.AddressDetail,
                        IsDefault = true
                    };
                    _context.UserAddresses.Add(address);
                }

                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> CheckCurrentPasswordAsync(string userId, string currentPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            return await _userManager.CheckPasswordAsync(user, currentPassword);
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> ChangePasswordAsync(ChangePasswordModel model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return (false, new[] { "User not found" });
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                return (false, changePasswordResult.Errors.Select(e => e.Description));
            }

            await _userManager.UpdateSecurityStampAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            return (true, Array.Empty<string>());
        }
    }
}