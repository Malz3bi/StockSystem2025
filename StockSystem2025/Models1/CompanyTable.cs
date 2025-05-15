using System;
using System.Collections.Generic;

namespace StockSystem2025.Models1;

public partial class CompanyTable
{
    public int CompanyId { get; set; }

    public string CompanyCode { get; set; } = null!;

    public string? CompanyName { get; set; }

    public bool? Follow { get; set; }

    public bool IsSpecial { get; set; }

    public bool IsIndicator { get; set; }

    public string? ParentIndicator { get; set; }

    public virtual ICollection<FollowListCompany> FollowListCompanies { get; set; } = new List<FollowListCompany>();

    public virtual ICollection<CompanyTable> InverseParentIndicatorNavigation { get; set; } = new List<CompanyTable>();

    public virtual CompanyTable? ParentIndicatorNavigation { get; set; }
}
