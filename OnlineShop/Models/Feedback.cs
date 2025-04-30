using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Feedback
    {
        [Key]
        public int CommentID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public string CustomerID { get; set; }

        [Required]
        [StringLength(256)]
        public string CustomerName { get; set; }

        [Required]
        public string CommentText { get; set; }

        [Required]
        public DateTime CommentDate { get; set; } = DateTime.Now;

        // Navigation properties
        public Product? Product { get; set; }
        public ApplicationUser? Customer { get; set; }
    }
}
