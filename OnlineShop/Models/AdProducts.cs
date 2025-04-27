namespace OnlineShop.Models
{
    public class AdProducts
    {
        public int Id { get; set; }
        public int AdId { get; set; }
        public int ProductId { get; set; }
        public string? Link { get; set; }

        public virtual Product Product { get; set; } = null!;
        public virtual Advertisement Advertisement { get; set; } = null!;
    }
}
