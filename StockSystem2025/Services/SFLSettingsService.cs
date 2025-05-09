using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models;
using StockSystem2025.SFLServices;

namespace StockSystem2025.Services
{
    public class SFLSettingsService : SFLISettingsService
    {
        private readonly StockdbContext _context;

        public SFLSettingsService(StockdbContext context)
        {
            _context = context;
        }

        public async Task<string> SFLGetSettingValueAsync(string name)
        {
            var setting = await _context.Settings
                .Where(x => x.Name == name)
                .Select(x => x.Value)
                .FirstOrDefaultAsync();
            return setting ?? string.Empty;
        }
    }
}