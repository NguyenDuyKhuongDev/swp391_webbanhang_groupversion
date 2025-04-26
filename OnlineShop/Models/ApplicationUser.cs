using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using OnlineShop.Data;

namespace OnlineShop.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        public bool? Gender { get; set; }

        public string? Avatar { get; set; }

        public virtual ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
        public virtual ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();
        public virtual ICollection<TicketReply> TicketReplies { get; set; } = new List<TicketReply>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
        public virtual ICollection<LikeOfBlog> LikeOfBlogs { get; set; } = new List<LikeOfBlog>();

    }
}
