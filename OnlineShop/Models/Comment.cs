using System;
using System.Collections.Generic;

namespace OnlineShop.Models;

public partial class Comment
{
    public int Id { get; set; }

    public int? ParentId { get; set; }

    public string? Content { get; set; }

    public string? UserId { get; set; }

    public int? TargetId { get; set; }

    public string? TargetType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? Star { get; set; }
}
