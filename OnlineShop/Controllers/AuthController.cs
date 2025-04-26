using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.DAO;
using OnlineShop.Models;
using OnlineShop.Models.OnlineShop.Models;

namespace OnlineShop.Controllers
{
    public class AuthController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthDAO _authDAO;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IAuthDAO authDAO)
        {
            _userManager = userManager;
            _authDAO = authDAO;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate login
            var (success, errorField, errorMessage, user) = await _authDAO.ValidateLoginAsync(model.Username, model.Password);

            if (!success)
            {
                ModelState.AddModelError(errorField, errorMessage);
                return View(model);
            }

            // Sign in user
            await _authDAO.SignInUserAsync(user, model.RememberMe, HttpContext);

            // Check user roles and redirect
            var roles = await _authDAO.GetUserRolesAsync(user);
            if (roles.Contains("Employee"))
            {
                return RedirectToAction("ProductView", "Product");
            }

            // Default fallback
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Register user
            var (success, errors) = await _authDAO.RegisterUserAsync(model);

            if (!success)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }
                return View(model);
            }

            // Get user for email confirmation
            var user = await _userManager.FindByNameAsync(model.Username);

            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Create confirmation link
            var confirmationLink = Url.Action("ConfirmEmail", "Auth",
                new { userId = user.Id, token = token }, Request.Scheme);

            // Send confirmation email
            await _authDAO.SendConfirmationEmailAsync(user.Id, confirmationLink);

            return RedirectToAction(nameof(RegisterConfirmation));
        }

        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Error", "Home");
            }

            var result = await _authDAO.ConfirmEmailAsync(userId, token);

            if (result)
            {
                return View("ConfirmEmailSuccess");
            }
            else
            {
                return View("ConfirmEmailFailure");
            }
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Generate token
            var token = await _authDAO.GeneratePasswordResetTokenAsync(model.Email);
            if (token != null)
            {
                // Create reset link
                var resetLink = Url.Action("ResetPassword", "Auth",
                    new { email = model.Email, token }, Request.Scheme);

                // Send email
                await _authDAO.InitiatePasswordResetAsync(model.Email, resetLink);
            }

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Error", "Home");
            }

            var model = new ResetPasswordModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, errors) = await _authDAO.ResetPasswordAsync(model.Email, model.Token, model.Password);

            if (!success)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
                }
                return View(model);
            }

            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _authDAO.SignOutAsync(HttpContext);
            return RedirectToAction("Index", "Home");
        }
    }
}