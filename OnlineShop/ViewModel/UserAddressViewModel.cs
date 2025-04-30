using System.ComponentModel.DataAnnotations;

namespace OnlineShop.ViewModel
{
    public class UserAddressViewModel
    {
        public int Id { get; set; }
        public string? UserId { get; set; }

        [Required(ErrorMessage = "Province/City is required")]
        public string Province { get; set; }

        [Required(ErrorMessage = "District is required")]
        public string District { get; set; }

        [Required(ErrorMessage = "Ward/Commune is required")]
        public string Ward { get; set; }

        [Required(ErrorMessage = "Detailed address is required")]
        [Display(Name = "Detailed Address")]
        public string AddressDetail { get; set; }

        [Display(Name = "Set as default address")]
        public bool IsDefault { get; set; }

        public string FullAddress => $"{Province}, {District}, {Ward}";


        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}
