using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.DAO;
using OnlineShop.Models;

namespace OnlineShop.Controllers;

public class HomeController : Controller
{
    private readonly IProductDAO _productDAO;
    private readonly ICategoryProductDAO _categoryProductDAO;
    private readonly ISliderDAO _sliderDAO;

    public HomeController(IProductDAO productDAO, ICategoryProductDAO categoryProductDAO, ISliderDAO sliderDAO)
    {
        _productDAO = productDAO;
        _categoryProductDAO = categoryProductDAO;
        _sliderDAO = sliderDAO;
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
        var sliders = await _sliderDAO.GetAllSlidersAsync(onlyActive: true); // Chỉ lấy slider đang active

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
        return View(product);
    }

    public IActionResult Blog()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
}

public class ProductListViewModel
{
    public List<Product> Products { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public string? SearchName { get; set; }
    public string? PriceSort { get; set; }
    public int? CategoryId { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
}