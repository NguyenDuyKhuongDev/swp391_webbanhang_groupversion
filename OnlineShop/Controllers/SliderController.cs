using CloudinaryDotNet;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.DAO;
using OnlineShop.Models;
using OnlineShop.Services;

namespace OnlineShop.Controllers
{
    public class SliderController : Controller
    {
        private readonly ISliderDAO _sliderDAO;
        private readonly Cloudinary _cloudinaryService;

        public SliderController(ISliderDAO sliderDAO, Cloudinary cloudinaryService)
        {
            _sliderDAO = sliderDAO;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<IActionResult> Index()
        {
            var sliders = await _sliderDAO.GetAllSlidersAsync();
            return View(sliders);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Slider slider, IFormFile SliderImageFile)
        {
            if (SliderImageFile == null || SliderImageFile.Length == 0)
            {
                ModelState.AddModelError("SliderImageFile", "Vui lòng chọn một hình ảnh.");
                return View(slider);
            }

            const long MaxFileSize = 5 * 1024 * 1024;
            if (SliderImageFile.Length > MaxFileSize)
            {
                ModelState.AddModelError("SliderImageFile", "Kích thước file phải nhỏ hơn 5MB.");
                return View(slider);
            }

            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            string fileExtension = Path.GetExtension(SliderImageFile.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("SliderImageFile", "Chỉ chấp nhận file ảnh có định dạng .jpg, .jpeg, .png, .gif, .webp.");
                return View(slider);
            }

            var imageUrl = await CloudinaryHelper.UploadImageAsync(_cloudinaryService, SliderImageFile);
            if (string.IsNullOrEmpty(imageUrl))
            {
                ModelState.AddModelError("SliderImageFile", "Không thể tải lên hình ảnh. Vui lòng thử lại.");
                return View(slider);
            }

            slider.ImageUrl = imageUrl;


            await _sliderDAO.AddSliderAsync(slider);
            TempData["SuccessMessage"] = "Thêm slider thành công!";
            return RedirectToAction("Index", "Home");


            //await CloudinaryHelper.DeleteImageFromCloudinaryAsync(_cloudinaryService, imageUrl);
            //return View(slider);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var slider = await _sliderDAO.GetSliderByIdAsync(id);
            if (slider == null)
            {
                return NotFound();
            }
            return View(slider);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Slider slider, IFormFile? SliderImageFile)
        {
            if (id != slider.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string? newImageUrl = null;
                string oldImageUrl = slider.ImageUrl;

                if (SliderImageFile != null && SliderImageFile.Length > 0)
                {
                    const long MaxFileSize = 5 * 1024 * 1024;
                    if (SliderImageFile.Length > MaxFileSize)
                    {
                        ModelState.AddModelError("SliderImageFile", "Kích thước file phải nhỏ hơn 5MB.");
                        return View(slider);
                    }

                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    string fileExtension = Path.GetExtension(SliderImageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("SliderImageFile", "Chỉ chấp nhận file ảnh có định dạng .jpg, .jpeg, .png, .gif, .webp.");
                        return View(slider);
                    }

                    newImageUrl = await CloudinaryHelper.UploadImageAsync(_cloudinaryService, SliderImageFile);
                    if (string.IsNullOrEmpty(newImageUrl))
                    {
                        ModelState.AddModelError("SliderImageFile", "Không thể tải lên hình ảnh. Vui lòng thử lại.");
                        return View(slider);
                    }

                    slider.ImageUrl = newImageUrl;
                }

                await _sliderDAO.UpdateSliderAsync(slider);

                if (!string.IsNullOrEmpty(newImageUrl) && !string.IsNullOrEmpty(oldImageUrl))
                {
                    await CloudinaryHelper.DeleteImageFromCloudinaryAsync(_cloudinaryService, oldImageUrl);
                }

                TempData["SuccessMessage"] = "Cập nhật slider thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(slider);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var slider = await _sliderDAO.GetSliderByIdAsync(id);
            if (slider == null)
            {
                return NotFound();
            }
            return View(slider);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var slider = await _sliderDAO.GetSliderByIdAsync(id);
            if (slider == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(slider.ImageUrl))
            {
                await CloudinaryHelper.DeleteImageFromCloudinaryAsync(_cloudinaryService, slider.ImageUrl);
            }

            await _sliderDAO.DeleteSliderAsync(id);
            TempData["SuccessMessage"] = "Xóa slider thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
