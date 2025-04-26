using System;
using System.Collections.Generic;

namespace OnlineShop.Models;

public partial class Tag
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Slug { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual ICollection<TagBlog> TagBlogs { get; set; } = new List<TagBlog>();
}
