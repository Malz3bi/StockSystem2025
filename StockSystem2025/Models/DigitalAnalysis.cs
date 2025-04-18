using System;
using System.Collections.Generic;

namespace StockSystem2025.Models;

public partial class DigitalAnalysis
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public double TopValue { get; set; }

    public double Bottom { get; set; }

    public int WavesCount { get; set; }

    public string? WavesVisibility { get; set; }

    public int? OrderNo { get; set; }

    public bool ShowDescriptionColumn { get; set; }

    public string? CompanyCode { get; set; }

    public DateOnly? TopValueDate { get; set; }

    public DateOnly? BottomValueDate { get; set; }

    public virtual ICollection<DigitalAnalysisDatum> DigitalAnalysisData { get; set; } = new List<DigitalAnalysisDatum>();
}
