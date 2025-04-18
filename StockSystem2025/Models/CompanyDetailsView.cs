using System;
using System.Collections.Generic;

namespace StockSystem2025.Models;

public partial class CompanyDetailsView
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

    public DateOnly? Createddate { get; set; }

    public double? ChangeValue { get; set; }

    public double? ChangeRate { get; set; }

    public double? CIndicatorIn { get; set; }

    public double? CIndicatorOut { get; set; }

    public double? Axis { get; set; }

    public double? VolMedium { get; set; }

    public string? ParentIndicator { get; set; }

    public bool IsIndicator { get; set; }

    public double? PrevSclose { get; set; }
}
