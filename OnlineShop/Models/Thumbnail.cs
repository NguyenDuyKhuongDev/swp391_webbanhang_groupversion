using System;
using System.Collections.Generic;

namespace OnlineShop.Models;

public partial class Thumbnail
{
    public int Id { get; set; }

    public string? ThumnailUrl { get; set; }

    public string ThumbnailName { get; set; } = null!;

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
