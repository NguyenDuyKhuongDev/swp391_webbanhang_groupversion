using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Mật khẩu mới không được để trống.")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải chứa ít nhất {2} ký tự.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).*$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất một chữ cái viết hoa, một chữ cái viết thường, một chữ số và một ký tự đặc biệt.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu xác nhận.")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và mật khẩu xác nhận không khớp.")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmPassword { get; set; }
    }
}