using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;
using OnlineShop.Models;
using OnlineShop.Models.OnlineShop.Models;
using System.Security.Claims;
using MailKit.Net.Smtp;

namespace OnlineShop.DAO
{
    public class AuthDAO : IAuthDAO
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly EmailSettings _emailSettings;

        public AuthDAO(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<EmailSettings> emailSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailSettings = emailSettings.Value;
        }

        public async Task<(bool Success, string ErrorField, string ErrorMessage, ApplicationUser User)> ValidateLoginAsync(string username, string password)
        {
            // Kiểm tra user tồn tại
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return (false, "Username", "Username does not exist.", null);
            }

            // Kiểm tra email đã xác nhận chưa
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return (false, "Username", "You must confirm your email before logging in.", null);
            }

            // Kiểm tra mật khẩu
            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                return (false, "Password", "Incorrect password.", null);
            }

            return (true, null, null, user);
        }

        public async Task<(bool Success, Dictionary<string, string> Errors)> RegisterUserAsync(RegisterModel model)
        {
            var errors = new Dictionary<string, string>();

            // Kiểm tra username đã tồn tại chưa
            var existingUser = await _userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                errors.Add("Username", "User Name is already taken.");
                return (false, errors);
            }

            // Kiểm tra email đã đăng ký chưa
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
            {
                errors.Add("Email", "Email is already registered.");
                return (false, errors);
            }

            // Tạo user mới
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Tạo role Customer nếu chưa tồn tại
                if (!await _roleManager.RoleExistsAsync(UserRoles.Customer))
                    await _roleManager.CreateAsync(new IdentityRole(UserRoles.Customer));

                // Gán role Customer cho user
                if (await _roleManager.RoleExistsAsync(UserRoles.Customer))
                {
                    await _userManager.AddToRoleAsync(user, UserRoles.Customer);
                }

                return (true, null);
            }
            else
            {
                // Xử lý các lỗi khi tạo user
                foreach (var error in result.Errors)
                {
                    if (error.Description.Contains("password", StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add("Password", error.Description);
                    }
                    else if (error.Description.Contains("username", StringComparison.OrdinalIgnoreCase) ||
                             error.Description.Contains("user name", StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add("Username", error.Description);
                    }
                    else if (error.Description.Contains("email", StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add("Email", error.Description);
                    }
                    else
                    {
                        errors.Add(string.Empty, error.Description);
                    }
                }

                return (false, errors);
            }
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }

        public async Task<bool> InitiatePasswordResetAsync(string email, string resetLink)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Vẫn trả về true để không tiết lộ thông tin email tồn tại
                return true;
            }

            // Gửi email đặt lại mật khẩu
            var subject = "Reset Password";
            var message = $@"
                <h2>Reset Your Password</h2>
                <p>Please click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you did not request a password reset, please ignore this email.</p>
            ";

            await SendEmailAsync(user.Email, subject, message);
            return true;
        }

        public async Task<string> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<(bool Success, Dictionary<string, string> Errors)> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var errors = new Dictionary<string, string>();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Vẫn trả về true để không tiết lộ thông tin email tồn tại
                return (true, null);
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                return (true, null);
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    if (error.Description.Contains("password", StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add("Password", error.Description);
                    }
                    else if (error.Description.Contains("token", StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(string.Empty, "Password reset link is invalid or has expired.");
                    }
                    else
                    {
                        errors.Add(string.Empty, error.Description);
                    }
                }

                return (false, errors);
            }
        }

        public async Task SignInUserAsync(ApplicationUser user, bool rememberMe, HttpContext httpContext)
        {
            var claims = await GetUserClaimsAsync(user);

            var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(3)
            };

            await httpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, authProperties);

            httpContext.Session.SetString("UserId", user.Id);
            httpContext.Session.SetString("UserName", user.UserName);
            httpContext.Session.SetString("UserEmail", user.Email);
        }

        public async Task SignOutAsync(HttpContext httpContext)
        {
            await _signInManager.SignOutAsync();
            httpContext.Session.Clear();
        }

        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            return await _userManager.GetRolesAsync(user);
        }

        public async Task SendConfirmationEmailAsync(string userId, string confirmationLink)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return;
            }

            var subject = "Confirm Your Email";
            var message = $@"
                <h2>Confirm Your Email</h2>
                <p>Thank you for registering with our website. Please click the link below to confirm your email address:</p>
                <p><a href='{confirmationLink}'>Confirm Email</a></p>
                <p>If you did not register for this account, please ignore this email.</p>
            ";

            await SendEmailAsync(user.Email, subject, message);
        }

        private async Task<List<Claim>> GetUserClaimsAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("UserEmail", user.Email)
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            return claims;
        }

        private async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (var client = new SmtpClient())
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("OnlineShop", _emailSettings.SenderEmail));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("html") { Text = htmlMessage };

                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, false);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }
    }
}