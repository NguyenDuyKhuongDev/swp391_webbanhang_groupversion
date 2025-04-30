using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class UserProfileViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [Display(Name = "Họ và tên")]
        public string? FullName { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Display(Name = "Phone")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Giới tính")]
        public bool? Gender { get; set; }
        public int? AddressId { get; set; }

        [Required(ErrorMessage = "Tỉnh/Thành phố là bắt buộc")]
        [Display(Name = "Tỉnh/Thành phố")]
        public string? Province { get; set; }

        [Required(ErrorMessage = "Quận/Huyện là bắt buộc")]
        [Display(Name = "Quận/Huyện")]
        public string? District { get; set; }

        [Required(ErrorMessage = "Phường/Xã là bắt buộc")]
        [Display(Name = "Phường/Xã")]
        public string? Ward { get; set; }

        [Required(ErrorMessage = "Địa chỉ chi tiết là bắt buộc")]
        [Display(Name = "Địa chỉ chi tiết")]
        public string? AddressDetail { get; set; }

        public string? Avatar { get; set; }
    }
}
