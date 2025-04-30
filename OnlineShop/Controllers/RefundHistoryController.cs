using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize]
    public class RefundHistoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RefundHistoryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

       

        public IActionResult Index(RefundHistoryFilterModel filter)
        {
            var userId = _userManager.GetUserId(User);
            var query = _context.RefundHistories
                .Include(r => r.Order)
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.OrderId))
                query = query.Where(r => r.OrderId.Contains(filter.OrderId));

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(r => r.RefundStatus == filter.Status);

            if (filter.FromDate.HasValue)
                query = query.Where(r => r.RefundDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(r => r.RefundDate <= filter.ToDate.Value.AddDays(1));

            // Apply sorting
            query = filter.SortColumn?.ToLower() switch
            {
                "date" => filter.SortOrder == "asc"
                    ? query.OrderBy(r => r.RefundDate)
                    : query.OrderByDescending(r => r.RefundDate),
                "amount" => filter.SortOrder == "asc"
                    ? query.OrderBy(r => r.RefundAmount)
                    : query.OrderByDescending(r => r.RefundAmount),
                "status" => filter.SortOrder == "asc"
                    ? query.OrderBy(r => r.RefundStatus)
                    : query.OrderByDescending(r => r.RefundStatus),
                _ => query.OrderByDescending(r => r.RefundDate) // Default sorting
            };

            // Set total items for paging
            filter.TotalItems = query.Count();

            var refunds = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return View(new Tuple<List<RefundHistory>, RefundHistoryFilterModel>(refunds, filter));
        }

        public IActionResult Details(int id)
        {
            var refund = _context.RefundHistories
                .Include(r => r.Order)
                .Include(r => r.User)
                .FirstOrDefault(r => r.RefundId == id);

            if (refund == null)
            {
                return NotFound();
            }

            return View(refund);
        }
    }
}