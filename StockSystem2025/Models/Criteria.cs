using System;
using System.Collections.Generic;

namespace StockSystem2025.Models;

public partial class Criteria
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Type { get; set; }

    public string? Note { get; set; }

    public string? Separater { get; set; }

    public int? OrderNo { get; set; }

    public string? Description { get; set; }

    public string? Color { get; set; }

    public string? ImageUrl { get; set; }

    /// <summary>
    /// 0  = false, 1 = true, 2 = all
    /// </summary>
    public int IsIndicator { get; set; }

    public bool? IsGeneral { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<Formula> Formulas { get; set; } = new List<Formula>();

    public virtual User User { get; set; } = null!;
}
