using OnlineShop.Data;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(255)]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm.")]
        [StringLength(1000)]
        public string ProductDescription { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá bán.")]
        public double ProductPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng sản phẩm.")]
        public int ProductQuantity { get; set; }

        public DateTime ProductCreatedDate { get; set; }

        public bool ProductStatus { get; set; }

        [Required(ErrorMessage = "Vui lòng thêm hình ảnh sản phẩm.")]
        [StringLength(1000)]
        public string ProductImage { get; set; } // Store image URL or file path



        // Foreign Keys
        [Required(ErrorMessage = "Vui lòng chọn thể loại sản phẩm.")]
        public int CategoryProductId { get; set; }

        // Navigation Properties
        public virtual CategoryProduct? CategoryProduct { get; set; }
        public virtual ICollection<ProductSize> ProductSizes { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<AdProducts> AdProducts { get; set; }
        public Product()
        {
            ProductSizes = new List<ProductSize>();
            ProductCreatedDate = DateTime.Now;
        }
    }
}
