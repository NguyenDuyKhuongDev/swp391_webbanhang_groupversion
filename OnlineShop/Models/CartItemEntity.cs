namespace OnlineShop.Models
{
    public class CartItemEntity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public int? CategorySizeId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }


        public ApplicationUser User { get; set; }
        public Product Product { get; set; }
        public CategorySize? CategorySize { get; set; }
    }
}
