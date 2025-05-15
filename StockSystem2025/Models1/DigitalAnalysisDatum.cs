using System;
using System.Collections.Generic;

namespace StockSystem2025.Models1;

public partial class DigitalAnalysisDatum
{
    public int Id { get; set; }

    public int DigitalAnalysisId { get; set; }

    public string Name { get; set; } = null!;

    public double Value { get; set; }

    public string? Color { get; set; }

    public bool Visible { get; set; }

    public virtual DigitalAnalysis DigitalAnalysis { get; set; } = null!;
}
