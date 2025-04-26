using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop.Models;

public partial class AdTemplate
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? HtmlTemplate { get; set; }

    public string? PreviewImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
    public virtual ICollection<AdTemplatePosition> AdTemplatePositions { get; set; } = new List<AdTemplatePosition>();
    [NotMapped] // Không ánh xạ vào database , khi lấy dữ liệu từ vie xuống thì dùng cho asp-for
    public List<int>? SelectedPositionIds { get; set; } 
}
