using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class LikeOfBlog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string? UserId { get; set; }

        public virtual Blog Blog { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } =null!;
    }
}
