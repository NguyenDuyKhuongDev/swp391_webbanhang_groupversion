using Microsoft.AspNetCore.Identity;
using OnlineShop.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Data
{
    public class TicketReply
    {
        public int Id { get; set; }

        public int SupportTicketId { get; set; }
        public SupportTicket SupportTicket { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}