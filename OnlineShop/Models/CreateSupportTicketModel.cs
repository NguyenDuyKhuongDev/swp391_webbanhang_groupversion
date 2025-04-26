using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models;

public class CreateSupportTicketModel
{
    [Required]
    [StringLength(100)]
    public string Subject { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;
}
