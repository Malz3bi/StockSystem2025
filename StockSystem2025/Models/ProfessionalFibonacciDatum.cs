using System;
using System.Collections.Generic;

namespace StockSystem2025.Models;

public partial class ProfessionalFibonacciDatum
{
    public int Id { get; set; }

    public int ProfessionalFibonacciId { get; set; }

    public string? Name { get; set; }

    public double Value { get; set; }

    public string? Color { get; set; }

    public bool Visible { get; set; }

    public virtual ProfessionalFibonacci ProfessionalFibonacci { get; set; } = null!;
}
