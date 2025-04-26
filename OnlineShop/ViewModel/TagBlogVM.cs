namespace OnlineShop.ViewModel
{
    public class TagBlogVM
    {
        public int Id { get; set; }

        public int TagId { get; set; }

        public int BlogId { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}
