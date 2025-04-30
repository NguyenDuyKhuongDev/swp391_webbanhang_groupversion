using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [Display(Name = "Tên đăng nhập")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }
}