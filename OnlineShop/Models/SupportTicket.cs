using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class SupportTicket
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        [Required]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Open"; // Open, In Progress, Closed

        public List<TicketReply> Replies { get; set; } = new List<TicketReply>();
    }
}
