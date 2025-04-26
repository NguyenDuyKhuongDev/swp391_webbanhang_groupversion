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
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == userId)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.OrderId))
                query = query.Where(o => o.OrderId.Contains(filter.OrderId));

            // Apply filters
            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(o => o.Status == filter.Status);

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.OrderDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(o => o.OrderDate <= filter.ToDate.Value.AddDays(1));

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

            var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id, string cancellationReason)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        
            if (order == null)
                return NotFound();
        
            if (order.Status != "Pending" && order.Status != "Processing")
                return BadRequest("This order cannot be cancelled.");
        
            order.Status = "Cancelled";
            order.CancellationReason = cancellationReason;
            await _context.SaveChangesAsync();
        
            TempData["SuccessMessage"] = "Order cancelled successfully.";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        public async Task<IActionResult> Reorder(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var originalOrder = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

                if (originalOrder == null)
                    return NotFound();

                // Create new order
                var newOrder = new Order
                {
                    UserId = userId,
                    OrderId = $"ORD{DateTime.Now.ToString("yyyyMMddHHmmss")}",
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    TotalAmount = originalOrder.TotalAmount,
                    OrderDetails = new List<OrderDetail>()
                };

                // Copy order details
                foreach (var item in originalOrder.OrderDetails)
                {
                    newOrder.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        ProductName = item.ProductName,
                        OrderId = item.OrderId,
                    });
                }

                if (!newOrder.OrderDetails.Any())
                {
                    TempData["ErrorMessage"] = "Cannot reorder as products are out of stock.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Orders.Add(newOrder);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Order has been successfully recreated.";
                return RedirectToAction(nameof(Details), new { id = newOrder.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}