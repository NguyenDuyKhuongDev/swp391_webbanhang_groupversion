using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class UserProfileViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Display(Name = "Phone Number")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Gender")]
        public bool? Gender { get; set; }
        public int? AddressId { get; set; }

        [Required(ErrorMessage = "Province/City is required")]
        [Display(Name = "Province/City")]
        public string? Province { get; set; }

        [Required(ErrorMessage = "District is required")]
        [Display(Name = "District")]
        public string? District { get; set; }

        [Required(ErrorMessage = "Ward/Commune is required")]
        [Display(Name = "Ward/Commune")]
        public string? Ward { get; set; }

        [Required(ErrorMessage = "Detailed address is required")]
        [Display(Name = "Detailed Address")]
        public string? AddressDetail { get; set; }

        public string? Avatar { get; set; }

    }
}
