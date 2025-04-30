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
                .Include(o => o.OrderItems)
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
                    ? query.OrderBy(o => o.CreatedDate)
                    : query.OrderByDescending(o => o.CreatedDate),
                "total" => filter.SortOrder == "asc"
                    ? query.OrderBy(o => o.Amount)
                    : query.OrderByDescending(o => o.Amount),
                "status" => filter.SortOrder == "asc"
                    ? query.OrderBy(o => o.Status)
                    : query.OrderByDescending(o => o.Status),
                _ => query.OrderByDescending(o => o.CreatedDate) // Default sorting
            };

            // Set total items for paging
            filter.TotalItems = await query.CountAsync();

            var orders = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return View(new Tuple<List<Order>, OrderFilterModel>(orders, filter));
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(string id, string status, string cancellationReason)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            order.Status = status;

            if (status == "Cancelled")
            {
                // Create refund history record
                var refundHistory = new RefundHistory
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    RefundAmount = order.Amount,
                    RefundStatus = "Completed",
                    Reason = "Nhân viên hủy đơn của khách",
                    RefundDate = DateTime.UtcNow
                };

                // Save changes
                _context.RefundHistories.Add(refundHistory);

                //if (string.IsNullOrWhiteSpace(cancellationReason))
                //    ModelState.AddModelError("", "Cancellation reason is required");
                //else
                //    order.OrderInfo = cancellationReason;
            }

            await _context.SaveChangesAsync();

            // Send email notification
            await _emailService.SendOrderStatusUpdateEmail(order);

            return RedirectToAction(nameof(Details), new { id = order.OrderId });
        }
    }
}