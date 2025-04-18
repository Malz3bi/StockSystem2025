using System;
using System.Collections.Generic;

namespace StockSystem2025.Models;

public partial class FollowListCompany
{
    public int Id { get; set; }

    public int? FollowListId { get; set; }

    public string? CompanyCode { get; set; }

    public virtual CompanyTable? CompanyCodeNavigation { get; set; }

    public virtual FollowList? FollowList { get; set; }
}
