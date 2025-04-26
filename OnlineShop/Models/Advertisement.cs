using System;
using System.Collections.Generic;

namespace OnlineShop.Models;

public partial class Advertisement
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? AdHtmlContent { get; set; }

    public string? ImageUrl { get; set; }

    public string? LinkUrl { get; set; }

    public bool IsActive { get; set; } = false;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? AdTemplateId { get; set; }

    public bool? IsCustomHtml { get; set; }

    public virtual ICollection<AdCategory> AdCategories { get; set; } = new List<AdCategory>();

    public virtual ICollection<AdClickLog> AdClickLogs { get; set; } = new List<AdClickLog>();

    public virtual ICollection<AdPlacement> AdPlacements { get; set; } = new List<AdPlacement>();

    public virtual AdTemplate? AdTemplate { get; set; }
}
