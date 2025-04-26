using System;
using System.Collections.Generic;

namespace OnlineShop.Models;

public partial class AdClickLog
{
    public int Id { get; set; }

    public int? AdId { get; set; }

    public int? BlogId { get; set; }

    public string? IpAddress { get; set; }

    public string? UserDevice { get; set; }

    public int? UserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ClickedAt { get; set; }

    public virtual Advertisement? Ad { get; set; }
}
