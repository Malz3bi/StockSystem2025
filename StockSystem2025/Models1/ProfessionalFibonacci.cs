using System;
using System.Collections.Generic;

namespace StockSystem2025.Models1;

public partial class ProfessionalFibonacci
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public double TopValue { get; set; }

    public double BottomValue { get; set; }

    public double FibonacciPercentageValue { get; set; }

    public int? OrderNo { get; set; }

    public bool ShowDescriptionColumn { get; set; }

    public string? CompanyCode { get; set; }

    public DateOnly? TopValueDate { get; set; }

    public DateOnly? BottomValueDate { get; set; }

    public virtual ICollection<ProfessionalFibonacciDatum> ProfessionalFibonacciData { get; set; } = new List<ProfessionalFibonacciDatum>();
}
