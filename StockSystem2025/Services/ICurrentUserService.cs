using StockSystem2025.Models;
using System.Threading.Tasks;

namespace StockSystem2025.Services
{
    public interface ICurrentUserService
    {
        Task<User> GetCurrentUserAsync();
    }

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly StockdbContext _context;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, StockdbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                throw new UnauthorizedAccessException("User not logged in.");
            }

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            return user;
        }
    }


}