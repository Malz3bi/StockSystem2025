using Microsoft.AspNetCore.Identity;
using StockSystem2025.Models;
using StockSystem2025.Models.AccountModels;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace StockSystem2025.Services
{
    public interface ICurrentUserService
    {
        Task<ApplicationUser?> GetCurrentUserAsync();
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return null;
            }
            return await _userManager.GetUserAsync(user);
        }

     
    }
}