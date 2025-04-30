using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        public string? Username { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [Required(ErrorMessage = "Email không được để trống.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải chứa ít nhất {2} ký tự.", MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).*$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất một chữ cái viết hoa, một chữ cái viết thường, một chữ số và một ký tự đặc biệt.")]
        public string? Password { get; set; }
    }
}