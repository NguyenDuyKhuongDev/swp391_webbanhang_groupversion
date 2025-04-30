using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.DAO;
using OnlineShop.Models;
using OnlineShop.ViewModel;

namespace OnlineShop.Controllers;

public class HomeController : Controller
{
    private readonly IProductDAO _productDAO;
    private readonly ICategoryProductDAO _categoryProductDAO;
    private readonly ISliderDAO _sliderDAO;
    private readonly IFeedbackDAO _commentDAO;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        IProductDAO productDAO,
        ICategoryProductDAO categoryProductDAO,
        ISliderDAO sliderDAO,
        IFeedbackDAO commentDAO,
        UserManager<ApplicationUser> userManager)
    {
        _productDAO = productDAO;
        _categoryProductDAO = categoryProductDAO;
        _sliderDAO = sliderDAO;
        _commentDAO = commentDAO;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 8, string? searchName = null, string? priceSort = null, int? categoryId = null)
    {
        var (products, totalItems) = await _productDAO.GetProductsAsync(
            page: page,
            pageSize: pageSize,
            searchName: searchName,
            status: true,
            priceSort: priceSort,
            categoryId: categoryId
        );

        var categories = await _categoryProductDAO.GetAllCategoryProductsAsync();
        var sliders = await _sliderDAO.GetAllSlidersAsync(onlyActive: true);

        ViewBag.Categories = categories;
        ViewBag.Sliders = sliders;

        var viewModel = new ProductListViewModel
        {
            Products = products,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            SearchName = searchName,
            PriceSort = priceSort,
            CategoryId = categoryId
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productDAO.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // Lấy danh sách bình luận của sản phẩm
        var comments = await _commentDAO.GetCommentsByProductIdAsync(id);
        ViewBag.Comments = comments;

        // Kiểm tra xem khách hàng đã mua sản phẩm hay chưa
        var user = await _userManager.GetUserAsync(User);
        bool hasPurchased = false;
        if (user != null)
        {
            hasPurchased = await _commentDAO.HasPurchasedProductAsync(user.Id, id);
        }
        ViewBag.HasPurchased = hasPurchased;

        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment(int productId, string customerId, string commentText)
    {
        if (string.IsNullOrEmpty(commentText))
        {
            TempData["Error"] = "Bình luận không được để trống!";
            return RedirectToAction("Details", new { id = productId });
        }

        var user = await _userManager.FindByIdAsync(customerId);
        if (user == null)
        {
            TempData["Error"] = "Không tìm thấy khách hàng!";
            return RedirectToAction("Details", new { id = productId });
        }

        var comment = new Feedback
        {
            ProductID = productId,
            CustomerID = customerId,
            CustomerName = user.FullName ?? user.UserName,
            CommentText = commentText,
            CommentDate = DateTime.Now
        };

        await _commentDAO.AddCommentAsync(comment);
        TempData["Success"] = "Bình luận đã được gửi thành công!";
        return RedirectToAction("Details", new { id = productId });
    }

    public IActionResult Blog()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> EditComment([FromBody] EditCommentViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var comment = await _commentDAO.GetCommentByIdAsync(model.CommentId);

        if (comment == null || comment.CustomerID != userId)
        {
            return Json(new { success = false, message = "Bình luận không tồn tại hoặc bạn không có quyền chỉnh sửa!" });
        }

        if (string.IsNullOrWhiteSpace(model.CommentText))
        {
            return Json(new { success = false, message = "Bình luận không được để trống!" });
        }

        comment.CommentText = model.CommentText;
        comment.CommentDate = DateTime.Now;
        await _commentDAO.UpdateCommentAsync(comment);

        return Json(new { success = true });
    }

    // Model để nhận dữ liệu từ AJAX
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> DeleteComment(int id, int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var comment = await _commentDAO.GetCommentByIdAsync(id);

        if (comment == null || comment.CustomerID != userId)
        {
            TempData["Error"] = "Bình luận không tồn tại hoặc bạn không có quyền xóa!";
            return RedirectToAction("Details", new { id = productId });
        }

        await _commentDAO.DeleteCommentAsync(id);
        TempData["Success"] = "Xóa bình luận thành công!";
        return RedirectToAction("Details", new { id = productId });
    }
}
