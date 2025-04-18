using System;
using System.Collections.Generic;

namespace StockSystem2025.Models;

public partial class SectorIndicatorVeiw
{
    public int DayNo { get; set; }

    public string Sticker { get; set; } = null!;

    public double? IndicatorIn { get; set; }

    public double? IndicatorOut { get; set; }

    public string? ParentIndicator { get; set; }
}
