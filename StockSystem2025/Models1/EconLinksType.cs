using System;
using System.Collections.Generic;

namespace StockSystem2025.Models1;

public partial class EconLinksType
{
    public int Id { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<EconomicLink> EconomicLinks { get; set; } = new List<EconomicLink>();
}
