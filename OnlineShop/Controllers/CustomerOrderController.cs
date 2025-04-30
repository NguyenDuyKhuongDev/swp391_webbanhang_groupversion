using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerOrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(OrderFilterModel filter)
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.OrderId))
                query = query.Where(o => o.OrderId.Contains(filter.OrderId));

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(o => o.Status == filter.Status);

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.CreatedDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(o => o.CreatedDate <= filter.ToDate.Value.AddDays(1));

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

            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(m => m.OrderId == id && m.UserId == user.Id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(string id, string cancellationReason)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            if (order.Status != "Pending" && order.Status != "Processing")
                return BadRequest("This order cannot be cancelled.");

            // Create refund history record
            var refundHistory = new RefundHistory
            {
                OrderId = order.OrderId,
                UserId = userId,
                RefundAmount = order.Amount,
                RefundStatus = "Completed",
                Reason = "Khách hàng hủy đơn hàng",
                RefundDate = DateTime.UtcNow
            };

            // Update order status
            order.Status = "Cancelled";

            // Save changes
            _context.RefundHistories.Add(refundHistory);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order cancelled successfully. Refund request has been submitted.";
            return RedirectToAction(nameof(Details), new { id = order.OrderId });
        }

        public async Task<IActionResult> Reorder(string id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var originalOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

                if (originalOrder == null)
                    return NotFound();

                // Create new order
                var newOrder = new Order
                {
                    UserId = userId,
                    OrderId = $"ORD{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                    CreatedDate = DateTime.Now,
                    Status = "Pending",
                    Amount = originalOrder.Amount,
                    DeliveryAddress = originalOrder.DeliveryAddress,
                    OrderInfo = originalOrder.OrderInfo,
                    OrderItems = new List<OrderDetail>()
                };

                // Copy order items
                foreach (var item in originalOrder.OrderItems)
                {
                    newOrder.OrderItems.Add(new OrderDetail
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        ProductName = item.ProductName,
                        OrderId = item.OrderId,
                        CategorySizeId = item.CategorySizeId,
                        ImageUrl = item.ImageUrl,
                    });
                }

                if (!newOrder.OrderItems.Any())
                {
                    TempData["ErrorMessage"] = "Cannot reorder as products are out of stock.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Order has been successfully recreated.";
                return RedirectToAction(nameof(Details), new { id = newOrder.OrderId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}