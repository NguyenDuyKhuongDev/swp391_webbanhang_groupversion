using System;
using System.Collections.Generic;

namespace OnlineShop.Models;

public partial class Blog
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? Slug { get; set; }

    public string? Summary { get; set; }

    public string? Content { get; set; }

    public string? AuthorId { get; set; }

    public DateTime? PublishedDate { get; set; }

    public DateTime? LastUpdated { get; set; }

    public bool IsPublished { get; set; }

    public int? CategoryId { get; set; }

    public int? ViewCount { get; set; }

    public int? LikeCount { get; set; }

    public int? CommentCount { get; set; }

    public int ThumbnailId { get; set; }

    public virtual ICollection<AdPlacement> AdPlacements { get; set; } = new List<AdPlacement>();

    public virtual BlogCategory? Category { get; set; }

    public virtual ICollection<TagBlog> TagBlogs { get; set; } = new List<TagBlog>();

    public virtual Thumbnail Thumbnail { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<LikeOfBlog> LikeOfBlogs { get; set; } = new List<LikeOfBlog>();
}
