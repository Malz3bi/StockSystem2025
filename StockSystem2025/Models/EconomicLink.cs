using System;
using System.Collections.Generic;

namespace StockSystem2025.Models;

public partial class EconomicLink
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Link { get; set; } = null!;

    public int TypeId { get; set; }

    public virtual EconLinksType Type { get; set; } = null!;
}
