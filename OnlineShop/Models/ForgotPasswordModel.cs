namespace OnlineShop.Models
{
    using System.ComponentModel.DataAnnotations;

    namespace OnlineShop.Models
    {
        public class ForgotPasswordModel
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string Email { get; set; }
        }
    }

}
