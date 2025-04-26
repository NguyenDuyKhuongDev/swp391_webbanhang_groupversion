using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class CategoryProduct
    {
        [Key]
        public int CategoryProductID { get; set; }

        [Required]
        [StringLength(255)]
        public string CategoryProductName { get; set; }

        // Navigation Property
        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<AdCategory> AdCategories{ get; set; }

        public CategoryProduct()
        {
            Products = new List<Product>();
        }
    }
}
