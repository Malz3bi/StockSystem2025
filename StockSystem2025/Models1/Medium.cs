using System;
using System.Collections.Generic;

namespace StockSystem2025.Models1;

public partial class Medium
{
    public int Id { get; set; }

    public int DaysMedium { get; set; }

    public bool ForTable { get; set; }

    public bool ForChart { get; set; }

    public string UserId { get; set; } = null!;

    public virtual AspNetUser User { get; set; } = null!;
}
