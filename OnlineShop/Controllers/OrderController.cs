using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyOrders(OrderFilterModel filter)
        {
            var user = await _userManager.GetUserAsync(User);

            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == user.Id)
                .AsQueryable();

            // Apply filters
            if (filter.FromDate.HasValue)
                query = query.Where(o => o.CreatedDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(o => o.CreatedDate <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(o => o.Status == filter.Status);

            // Set total items for paging
            filter.TotalItems = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return View(new Tuple<List<Order>, OrderFilterModel>(orders, filter));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(m => m.OrderId == id.ToString());

            if (order == null)
                return NotFound();

            // Check if user has permission to view this order
            if (!isAdmin && order.UserId != user.Id)
                return Forbid();

            return View(order);
        }
    }
}