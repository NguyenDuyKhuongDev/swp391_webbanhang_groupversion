using CloudinaryDotNet;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineShop.DAO;
using OnlineShop.Data;
using OnlineShop.Data.Settings;
using OnlineShop.Models;
using OnlineShop.Services;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

// Add services to the container.

// For Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("ConnStr")));

// For Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// For Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Settings Cloudinary
builder.Services.AddSingleton(provider =>
{
    var config = builder.Configuration.GetSection("Cloudinary").Get<CloundinarySettings>();
    return new Cloudinary(new Account(config.CloudName, config.ApiKey, config.ApiSecret));
});

// For Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Adding Cookie Authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Auth/Login"; // Đường dẫn đến trang đăng nhập
    options.AccessDeniedPath = "/Auth/AccessDenied"; // Đường dẫn khi bị từ chối truy cập
    options.ExpireTimeSpan = TimeSpan.FromHours(3); // Thời gian hết hạn của cookie
    options.SlidingExpiration = true; // Gia hạn cookie khi người dùng hoạt động
});


builder.Services.AddScoped<IProductDAO, ProductDAO>();
builder.Services.AddScoped<ICategoryProductDAO, CategoryProductDAO>();
builder.Services.AddScoped<IProductSizeDAO, ProductSizeDAO>();
builder.Services.AddScoped<ICategorySizeDAO, CategorySizeDAO>();

builder.Services.AddScoped<ICartDAO, CartDAO>();
builder.Services.AddScoped<IVNPayDAO, VNPayDAO>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISliderDAO, SliderDAO>();
builder.Services.AddScoped<IUserProfileDAO, UserProfileDAO>();
builder.Services.AddScoped<IAuthDAO, AuthDAO>();

builder.Services.AddHttpContextAccessor();


builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
