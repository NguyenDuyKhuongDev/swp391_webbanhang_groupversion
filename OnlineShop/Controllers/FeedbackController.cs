using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.DAO;
using OnlineShop.Models;

namespace OnlineShop.Controllers
{
    [Authorize(Roles = UserRoles.Employee)]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackDAO _commentDAO;

        public FeedbackController(IFeedbackDAO commentDAO)
        {
            _commentDAO = commentDAO;
        }

        public async Task<IActionResult> ManageComments()
        {
            var comments = await _commentDAO.GetCommentsByProductIdAsync(0); // 0 để lấy tất cả bình luận
            return View(comments);
        }

        public async Task<IActionResult> EditComment(int id)
        {
            var comment = await _commentDAO.GetCommentByIdAsync(id);
            if (comment == null) return NotFound();
            return View(comment);
        }

        [HttpPost]
        public async Task<IActionResult> EditComment(Feedback model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ!";
                return View(model);
            }

            var comment = await _commentDAO.GetCommentByIdAsync(model.CommentID);
            if (comment == null) return NotFound();

            comment.CommentText = model.CommentText;
            await _commentDAO.UpdateCommentAsync(comment);
            TempData["Success"] = "Cập nhật bình luận thành công!";
            return RedirectToAction("ManageComments");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _commentDAO.GetCommentByIdAsync(id);
            if (comment == null)
            {
                TempData["Error"] = "Không tìm thấy bình luận!";
                return RedirectToAction("ManageComments");
            }

            await _commentDAO.DeleteCommentAsync(id);
            TempData["Success"] = "Xóa bình luận thành công!";
            return RedirectToAction("ManageComments");
        }
    }
}
