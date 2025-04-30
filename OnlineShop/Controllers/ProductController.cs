using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.DAO;
using OnlineShop.Models;
using System.Net;

namespace OnlineShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductDAO _productDAO;
        private readonly ICategoryProductDAO _categoryProductDAO;
        private readonly IProductSizeDAO _productSizeDAO;
        private readonly ICategorySizeDAO _categorySizeDAO;
        private readonly Cloudinary _cloudinaryService;


        public ProductController(IProductDAO productDAO, ICategoryProductDAO categoryProductDAO, IProductSizeDAO productSizeDAO, ICategorySizeDAO categorySizeDAO, Cloudinary cloudinaryService)
        {
            _productDAO = productDAO;
            _categoryProductDAO = categoryProductDAO;
            _productSizeDAO = productSizeDAO;
            _categorySizeDAO = categorySizeDAO;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<IActionResult> ProductView(int page = 1, int pageSize = 5, string? searchName = null, bool? status = null, string? priceSort = null, int? categoryId = null)
        {
            try
            {
                var (products, totalItems) = await _productDAO.GetProductsAsync(page, pageSize, searchName, status, priceSort, categoryId);
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Truyền dữ liệu sang view
                ViewBag.Page = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.SearchName = searchName;
                ViewBag.Status = status?.ToString().ToLower();
                ViewBag.PriceSort = priceSort;
                ViewBag.CategoryId = categoryId;
                ViewBag.Categories = await _categoryProductDAO.GetAllCategoryProductsAsync();

                return View(products);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Không thể tải danh sách sản phẩm: " + ex.Message;
                return View(new List<Product>());
            }
        }

        public async Task<IActionResult> ProductAdd()
        {
            ViewBag.Categories = await _categoryProductDAO.GetAllCategoryProductsAsync();
            ViewBag.Sizes = await _categorySizeDAO.GetAllCategorySizesAsync();
            return View(new Product());
        }


        // Handle Add Product
        [HttpPost]
        public async Task<IActionResult> HandleAddProduct(Product product, IFormFile ProductImageFile, int[] SelectedSizes, [FromForm(Name = "SizeQuantities")] Dictionary<int, string> sizeQuantities)
        {
            try
            {
                ViewBag.Categories = await _categoryProductDAO.GetAllCategoryProductsAsync();
                ViewBag.Sizes = await _categorySizeDAO.GetAllCategorySizesAsync();

                //if (!ModelState.IsValid)
                //{
                //    return View("ProductAdd", product);
                //}

                // Validate: Kiểm tra ít nhất một kích thước được chọn
                //if (SelectedSizes == null || !SelectedSizes.Any())
                //{
                //    ModelState.AddModelError("SelectedSizes", "Vui lòng chọn ít nhất một kích thước.");
                //    return View("ProductAdd", product);
                //}

                // Validate: Kiểm tra trùng tên sản phẩm
                if (await _productDAO.CheckNameProductAsync(product.ProductName))
                {
                    ModelState.AddModelError("ProductName", "Tên sản phẩm đã tồn tại.");
                    return View("ProductAdd", product);
                }

                // Validate: Kiểm tra giá tiền không âm
                if (product.ProductPrice < 0)
                {
                    ModelState.AddModelError("ProductPrice", "Giá bán không được nhỏ hơn 0.");
                    return View("ProductAdd", product);
                }

                // Validate: Kiểm tra số lượng theo kích thước
                var sizeQuantityDict = new Dictionary<int, int>();
                foreach (var sizeId in SelectedSizes)
                {
                    if (!sizeQuantities.ContainsKey(sizeId) || string.IsNullOrEmpty(sizeQuantities[sizeId]))
                    {
                        ModelState.AddModelError($"SizeQuantities[{sizeId}]", "Vui lòng nhập số lượng cho kích thước được chọn.");
                        return View("ProductAdd", product);
                    }

                    if (!int.TryParse(sizeQuantities[sizeId], out int quantity) || quantity < 0)
                    {
                        ModelState.AddModelError($"SizeQuantities[{sizeId}]", "Số lượng phải lớn hơn hoặc bằng 0.");
                        return View("ProductAdd", product);
                    }

                    sizeQuantityDict[sizeId] = quantity;
                }

                // Validate: Kiểm tra file ảnh
                const long MaxFileSize = 5 * 1024 * 1024; // 5MB
                if (ProductImageFile.Length > MaxFileSize)
                {
                    ModelState.AddModelError("ProductImageFile", "Kích thước file phải nhỏ hơn 5MB.");
                    return View("ProductAdd", product);
                }

                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                string fileExtension = Path.GetExtension(ProductImageFile.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ProductImageFile", "Chỉ chấp nhận file ảnh có định dạng .jpg,.webp, .jpeg, .png, .gif.");
                    return View("ProductAdd", product);
                }

                // Upload ảnh lên Cloudinary
                var imageUrl = await UploadImageAsync(ProductImageFile);
                if (string.IsNullOrEmpty(imageUrl))
                {
                    ModelState.AddModelError("ProductImageFile", "Không thể tải lên hình ảnh. Vui lòng thử lại.");
                    return View("ProductAdd", product);
                }

                // Gán URL ảnh vào ProductImage
                product.ProductImage = imageUrl;

                // Set ProductCreatedDate
                product.ProductCreatedDate = DateTime.Now;

                // Tính ProductQuantity và ProductStatus
                product.ProductQuantity = sizeQuantityDict.Values.Sum();
                product.ProductStatus = product.ProductQuantity > 0;

                // Thêm sản phẩm vào cơ sở dữ liệu
                var success = await _productDAO.AddProductAsync(product);
                if (!success)
                {
                    await DeleteImageFromCloudinaryAsync(imageUrl);
                    TempData["ErrorMessage"] = "Không thể thêm sản phẩm.";
                    return View("ProductAdd", product);
                }

                // Thêm các kích thước và số lượng vào bảng ProductSize
                var productSizeSuccess = await _productSizeDAO.AddProductSizesAsync(product.ProductID, sizeQuantityDict);
                if (!productSizeSuccess)
                {
                    await DeleteImageFromCloudinaryAsync(imageUrl);
                    TempData["ErrorMessage"] = "Không thể thêm kích thước cho sản phẩm.";
                    return View("ProductAdd", product);
                }

                TempData["SuccessMessage"] = "Thêm mới sản phẩm thành công";
                return RedirectToAction("ProductView");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi thêm sản phẩm: " + ex.Message;
                ViewBag.Categories = await _categoryProductDAO.GetAllCategoryProductsAsync();
                ViewBag.Sizes = await _categorySizeDAO.GetAllCategorySizesAsync();
                return View("ProductAdd", product);
            }
        }

        public async Task<IActionResult> ProductDetail(int proId)
        {
            var product = await _productDAO.GetProductByIdAsync(proId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index");
            }

            ViewBag.Categories = await _categoryProductDAO.GetAllCategoryProductsAsync();
            ViewBag.Sizes = await _categorySizeDAO.GetAllCategorySizesAsync();
            return View(product);
        }

        // Handle Update Product
        [HttpPost]

        public async Task<IActionResult> HandleUpdateAndDeleteProduct(Product product, IFormFile? ProductImageFile, int[] SelectedSizes, [FromForm(Name = "SizeQuantities")] Dictionary<int, string> sizeQuantities, string actionProduct)
        {
            try
            {
                ViewBag.Categories = await _categoryProductDAO.GetAllCategoryProductsAsync();
                ViewBag.Sizes = await _categorySizeDAO.GetAllCategorySizesAsync();

                if (actionProduct == "0") // Update
                {
                    //if (!ModelState.IsValid)
                    //{
                    //    return View("ProductDetail", product);
                    //}

                    //// Validate: Kiểm tra ít nhất một kích thước được chọn
                    //if (SelectedSizes == null || !SelectedSizes.Any())
                    //{
                    //    ModelState.AddModelError("SelectedSizes", "Vui lòng chọn ít nhất một kích thước.");
                    //    return View("ProductDetail", product);
                    //}

                    // Validate: Kiểm tra trùng tên sản phẩm
                    if (await _productDAO.CheckNameProductAsync(product.ProductName, product.ProductID))
                    {
                        ModelState.AddModelError("ProductName", "Tên sản phẩm đã tồn tại.");
                        return View("ProductDetail", product);
                    }

                    // Validate: Kiểm tra giá tiền không âm
                    if (product.ProductPrice <= 0)
                    {
                        ModelState.AddModelError("ProductPrice", "Giá bán không được nhỏ hơn 0.");
                        return View("ProductDetail", product);
                    }

                    // Validate: Kiểm tra số lượng theo kích thước
                    var sizeQuantityDict = new Dictionary<int, int>();
                    foreach (var sizeId in SelectedSizes)
                    {
                        if (!sizeQuantities.ContainsKey(sizeId) || string.IsNullOrEmpty(sizeQuantities[sizeId]))
                        {
                            ModelState.AddModelError($"SizeQuantities[{sizeId}]", "Vui lòng nhập số lượng cho kích thước được chọn.");
                            return View("ProductDetail", product);
                        }

                        if (!int.TryParse(sizeQuantities[sizeId], out int quantity) || quantity < 0)
                        {
                            ModelState.AddModelError($"SizeQuantities[{sizeId}]", "Số lượng phải lớn hơn hoặc bằng 0.");
                            return View("ProductDetail", product);
                        }

                        sizeQuantityDict[sizeId] = quantity;
                    }

                    // Validate: Kiểm tra file ảnh nếu có upload
                    string? newImageUrl = null;
                    if (ProductImageFile != null && ProductImageFile.Length > 0)
                    {
                        const long MaxFileSize = 5 * 1024 * 1024; // 5MB
                        if (ProductImageFile.Length > MaxFileSize)
                        {
                            ModelState.AddModelError("ProductImageFile", "Kích thước file phải nhỏ hơn 5MB.");
                            return View("ProductDetail", product);
                        }

                        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        string fileExtension = Path.GetExtension(ProductImageFile.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ProductImageFile", "Chỉ chấp nhận file ảnh có định dạng .jpg, .jpeg, .png, .gif.");
                            return View("ProductDetail", product);
                        }

                        newImageUrl = await UploadImageAsync(ProductImageFile);
                        if (string.IsNullOrEmpty(newImageUrl))
                        {
                            ModelState.AddModelError("ProductImageFile", "Không thể tải lên hình ảnh. Vui lòng thử lại.");
                            return View("ProductDetail", product);
                        }
                    }

                    // Nếu có ảnh mới, cập nhật ProductImage và xóa ảnh cũ
                    var oldImageUrl = product.ProductImage;
                    if (!string.IsNullOrEmpty(newImageUrl))
                    {
                        product.ProductImage = newImageUrl;
                    }

                    // Cập nhật sản phẩm
                    var success = await _productDAO.UpdateProductAsync(product, sizeQuantityDict);
                    if (!success)
                    {
                        if (!string.IsNullOrEmpty(newImageUrl))
                        {
                            await DeleteImageFromCloudinaryAsync(newImageUrl);
                        }
                        TempData["ErrorMessage"] = "Không thể cập nhật sản phẩm.";
                        return View("ProductDetail", product);
                    }

                    // Nếu cập nhật thành công và có ảnh mới, xóa ảnh cũ trên Cloudinary
                    if (!string.IsNullOrEmpty(newImageUrl) && !string.IsNullOrEmpty(oldImageUrl))
                    {
                        await DeleteImageFromCloudinaryAsync(oldImageUrl);
                    }

                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("ProductView");
                }
                else // Toggle Status
                {
                    var success = await _productDAO.ToggleProductStatusAsync(product.ProductID);
                    if (!success)
                    {
                        TempData["ErrorMessage"] = "Không thể thay đổi trạng thái sản phẩm.";
                        return View("ProductDetail", product);
                    }

                    TempData["SuccessMessage"] = "Thay đổi trạng thái sản phẩm thành công!";
                    return RedirectToAction("ProductView");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xử lý sản phẩm: " + ex.Message;
                ViewBag.Categories = await _categoryProductDAO.GetAllCategoryProductsAsync();
                ViewBag.Sizes = await _categorySizeDAO.GetAllCategorySizesAsync();
                return View("ProductDetail", product);
            }
        }

        // Handle Image
        public async Task<string?> UploadImageAsync(IFormFile image)
        {
            long maxFileSize = 5 * 1024 * 1024;

            if (image == null || image.Length == 0)
            {
                return null;
            }

            if (image.Length > maxFileSize)
            {
                return null;
            }
            var id = await GenerateShortUniqueCode();
            try
            {
                using (var stream = image.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        //File = new FileDescription(image.FileName, stream),
                        //Folder = "/WMProject/Img_products", 
                        //PublicId = $"product_image_{id}",

                        File = new FileDescription(image.FileName, stream),
                        Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                        UploadPreset = "SWP391Project",
                        UseFilename = true,     // Dùng tên file gốc làm PublicId
                        UniqueFilename = true
                    };

                    var uploadResult = await _cloudinaryService.UploadAsync(uploadParams); // Use UploadAsync for file < 100MB
                    return uploadResult.SecureUrl.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GenerateShortUniqueCode()
        {
            Guid guid = Guid.NewGuid();
            byte[] bytes = guid.ToByteArray();
            byte[] shortBytes = new byte[6];
            Array.Copy(bytes, 0, shortBytes, 0, 6); // Get 6 byte (48-bit)
            return Convert.ToBase64String(shortBytes).Substring(0, 8); // 8 characters
        }

        public async Task<bool> DeleteImageFromCloudinaryAsync(string imageUrl)
        {
            string publicId = ExtractPublicIdFromUrl(imageUrl);

            if (string.IsNullOrEmpty(publicId))
            {
                return false;
            }

            try
            {
                var deletionParams = new DeletionParams(publicId);
                var deletionResult = await _cloudinaryService.DestroyAsync(deletionParams);

                if (deletionResult.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    Console.WriteLine($"Error deleting image: {deletionResult.Error?.Message}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
        public string ExtractPublicIdFromUrl(string url)
        {
            Uri uri = new Uri(url);
            string path = uri.AbsolutePath; // /image/upload/v1741920422/HelloApple_vocr5r.jpg

            // Tách phần public_id từ path (bỏ phần /image/upload/ và phiên bản)
            string[] pathParts = path.Split('/');
            string publicIdWithVersion = pathParts[4]; // v1741920422
            string publicId = pathParts[5].Split('.')[0]; // HelloApple_vocr5r

            return publicId;
        }



    }
}

