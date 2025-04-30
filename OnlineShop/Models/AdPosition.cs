using System;
using System.Collections.Generic;

namespace OnlineShop.Models;

public partial class AdPosition
{
    public int Id { get; set; }

    public string? Position{ get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsActive { get; set; } = false;
  

    public virtual ICollection<AdPlacement> AdPlacements { get; set; } = new List<AdPlacement>();
    public virtual ICollection<AdTemplatePosition> AdTemplatePositions { get; set; } = new List<AdTemplatePosition>();
}
