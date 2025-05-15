using System;
using System.Collections.Generic;

namespace StockSystem2025.Models1;

public partial class GeneralDetailedStockView
{
    public int DayNo { get; set; }

    public string Sticker { get; set; } = null!;

    public string? Sname { get; set; }

    public string Sdate { get; set; } = null!;

    public double? Sopen { get; set; }

    public double? Shigh { get; set; }

    public double? Slow { get; set; }

    public double? Sclose { get; set; }

    public double? Svol { get; set; }

    public DateTime? Createddate { get; set; }

    public double? ChangeValue { get; set; }

    public double? ChangeRate { get; set; }

    public double? Axis { get; set; }

    public double? VolMedium { get; set; }

    public double? CIndicatorIn { get; set; }

    public double? CIndicatorOut { get; set; }

    public double? SIndicatorIn { get; set; }

    public double? SIndicatorOut { get; set; }

    public double? GIndicatorIn { get; set; }

    public double? GIndicatorOut { get; set; }

    public double? PrevSclose { get; set; }
}
