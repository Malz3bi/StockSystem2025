using System;
using System.Collections.Generic;

namespace StockSystem2025.Models;

public partial class FollowList
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Color { get; set; }

    public double? StopLoss { get; set; }

    public double? FirstSupport { get; set; }

    public double? SecondSupport { get; set; }

    public double? FirstTarget { get; set; }

    public double? SecondTarget { get; set; }

    public double? ThirdTarget { get; set; }

    public int UserId { get; set; }

    public virtual ICollection<FollowListCompany> FollowListCompanies { get; set; } = new List<FollowListCompany>();

    public virtual User User { get; set; } = null!;
}
