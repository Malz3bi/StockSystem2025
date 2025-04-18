using System;
using System.Collections.Generic;

namespace StockSystem2025.Models;

public partial class StockTable
{
    public int Id { get; set; }

    public int DayNo { get; set; }

    public string Sticker { get; set; } = null!;

    public string? Sname { get; set; }

    public string Sdate { get; set; } = null!;

    public double? Sopen { get; set; }

    public double? Shigh { get; set; }

    public double? Slow { get; set; }

    public double? Sclose { get; set; }

    public double? Svol { get; set; }

    public double? ExpectedOpen { get; set; }

    public DateTime? Createddate { get; set; }
}
