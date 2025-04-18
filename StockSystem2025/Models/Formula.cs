using System;
using System.Collections.Generic;

namespace StockSystem2025.Models;

public partial class Formula
{
    public int Id { get; set; }

    public int FormulaType { get; set; }

    public int Day { get; set; }

    public string FormulaValues { get; set; } = null!;

    public int CriteriaId { get; set; }

    public virtual Criteria Criteria { get; set; } = null!;
}
