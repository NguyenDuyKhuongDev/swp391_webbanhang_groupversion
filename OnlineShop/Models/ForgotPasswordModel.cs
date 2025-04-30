namespace OnlineShop.Models
{
    using System.ComponentModel.DataAnnotations;

    namespace OnlineShop.Models
    {
        public class ForgotPasswordModel
        {
            [Required(ErrorMessage = "Email không được để trống")]
            [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
            public string Email { get; set; }
        }
    }

}
