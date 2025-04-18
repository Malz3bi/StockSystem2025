using System;
using System.Collections.Generic;

namespace StockSystem2025.Models;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string EmailCode { get; set; } = null!;

    public bool EmailConfirmed { get; set; }

    public int Bundle { get; set; }

    public int Role { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Criteria> Criteria { get; set; } = new List<Criteria>();

    public virtual ICollection<FollowList> FollowLists { get; set; } = new List<FollowList>();

    public virtual ICollection<Medium> Media { get; set; } = new List<Medium>();
}
