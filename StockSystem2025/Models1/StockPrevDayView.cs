using System;
using System.Collections.Generic;

namespace StockSystem2025.Models1;

public partial class StockPrevDayView
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

    public double? PrevSopen { get; set; }

    public double? PrevShigh { get; set; }

    public double? PrevSlow { get; set; }

    public double? PrevSclose { get; set; }

    public double? PrevSvol { get; set; }

    public double? ChangeValue { get; set; }

    public double? ChangeRate { get; set; }

    public double? IndicatorIn { get; set; }

    public double? IndicatorOut { get; set; }

    public bool IsIndicator { get; set; }

    public string? ParentIndicator { get; set; }

    public bool IsSpecial { get; set; }
}
