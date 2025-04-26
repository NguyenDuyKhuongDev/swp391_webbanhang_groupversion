using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class ProductSize
    {
        public int ProductID { get; set; }

        public int CategorySizeID { get; set; }

        [Required]
        public int QuantityBySize { get; set; } // Số lượng theo kích thước
        // Navigation Properties
        public virtual Product Product { get; set; }
        public virtual CategorySize CategorySize { get; set; }
    }
}
