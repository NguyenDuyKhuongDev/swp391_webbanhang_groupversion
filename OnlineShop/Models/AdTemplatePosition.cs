using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models
{
    public class AdTemplatePosition
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int AdTemplateId { get; set; }
        public int PositionId { get; set; }
     
        public virtual AdTemplate AdTemplate { get; set; } =null!;
        public virtual AdPosition AdPosition { get; set; } = null!;
    }
}
