using Microsoft.AspNetCore.Identity;

namespace StockSystem2025.Models.AccountModels;

    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public bool IsActive { get; set; }
        public IList<string>? Roles { get; set; }

    }

