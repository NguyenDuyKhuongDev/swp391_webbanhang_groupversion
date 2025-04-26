﻿using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
