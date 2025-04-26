using System;
using System.Collections.Generic;

namespace OnlineShop.Models;

public partial class BlogCategory
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
