using System;
using System.Collections.Generic;

namespace OnlineShop.Models;

public partial class TagBlog
{
    public int Id { get; set; }

    public int TagId { get; set; }

    public int BlogId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Blog Blog { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;
}
