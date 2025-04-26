namespace OnlineShop.Models
{
    public class Slider
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } // Đường dẫn đến hình ảnh của slider
        public string Title { get; set; } // Tiêu đề của slider
        public string Description { get; set; } // Mô tả ngắn (nếu có)
        public string Link { get; set; } // Liên kết khi nhấn vào slider (nếu có)
        public bool IsActive { get; set; } // Trạng thái: active hay không
        public int DisplayOrder { get; set; } // Thứ tự hiển thị
    }
}
