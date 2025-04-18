using System;
using System.Collections.Generic;

namespace StockSystem2025.Models;

public partial class Medium
{
    public int Id { get; set; }

    public int DaysMedium { get; set; }

    public bool ForTable { get; set; }

    public bool ForChart { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
