using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using OnlineShop.Services;

namespace OnlineShop.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public EmployeeOrderController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(OrderFilterModel filter)
        {
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.User)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.OrderId))
                query = query.Where(o => o.OrderId.Contains(filter.OrderId));

            if (!string.IsNullOrEmpty(filter.CustomerSearch))
                query = query.Where(o => o.User.Email.Contains(filter.CustomerSearch) || 
                                        o.User.UserName.Contains(filter.CustomerSearch));

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(o => o.Status == filter.Status);

            // Apply sorting
            query = filter.SortColumn?.ToLower() switch
            {
                "date" => filter.SortOrder == "asc" 
                    ? query.OrderBy(o => o.OrderDate)
                    : query.OrderByDescending(o => o.OrderDate),
                "total" => filter.SortOrder == "asc"
                    ? query.OrderBy(o => o.TotalAmount)
                    : query.OrderByDescending(o => o.TotalAmount),
                "status" => filter.SortOrder == "asc"
                    ? query.OrderBy(o => o.Status)
                    : query.OrderByDescending(o => o.Status),
                _ => query.OrderByDescending(o => o.OrderDate) // Default sorting
            };

            // Set total items for paging
            filter.TotalItems = await query.CountAsync();

            var orders = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return View(new Tuple<List<Order>, OrderFilterModel>(orders, filter));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status, string cancellationReason)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            order.Status = status;
            
            if (status == "Cancelled")
            {
                if (string.IsNullOrWhiteSpace(cancellationReason))
                    ModelState.AddModelError("", "Cancellation reason is required");
                else
                    order.CancellationReason = cancellationReason;
            }

            await _context.SaveChangesAsync();
            
            // Send email notification
            await _emailService.SendOrderStatusUpdateEmail(order);

            return RedirectToAction(nameof(Details), new { id = order.Id });
        }
    }
}