using OnlineShop.Models;

namespace OnlineShop.ViewModel
{
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
}
