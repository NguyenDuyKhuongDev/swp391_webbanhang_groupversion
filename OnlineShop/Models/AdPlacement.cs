using System;
using System.Collections.Generic;

namespace OnlineShop.Models;

public partial class AdPlacement
{
    public int Id { get; set; }

    public int? BlogId { get; set; }

    public int? AdId { get; set; }

    public int? PositionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Advertisement? Ad { get; set; }

    public virtual Blog? Blog { get; set; }

    public virtual AdPosition? Position { get; set; }
}
