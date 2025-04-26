using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class CategorySize
    {
        [Key]
        public int CategorySizeID { get; set; }

        [Required]
        [StringLength(100)]
        public string CategorySizeName { get; set; }

        // Navigation Property
        public virtual ICollection<ProductSize> ProductSizes { get; set; }

        public CategorySize()
        {
            ProductSizes = new List<ProductSize>();
        }
    }
}
