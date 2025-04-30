namespace OnlineShop.ViewModel
{
    public class CartItemViewModel
    {
        public string Id { get; set; }
        public int ProductId { get; set; }
        public int? CategorySizeId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
