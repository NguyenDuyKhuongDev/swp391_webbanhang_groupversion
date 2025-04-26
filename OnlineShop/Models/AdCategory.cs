using System;
using System.Collections.Generic;

namespace OnlineShop.Models;

public partial class AdCategory
{
    public int Id { get; set; }

    public int? AdId { get; set; }

    public int? CategoryId { get; set; }

    public virtual Advertisement? Ad { get; set; }
    public virtual CategoryProduct? Category { get; set; }
}
