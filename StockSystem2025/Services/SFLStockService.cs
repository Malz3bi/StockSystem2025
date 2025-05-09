using Microsoft.EntityFrameworkCore;

using StockSystem2025.Models;
using StockSystem2025.SFLServices;


namespace StockSystem2025.Services
{
    public class SFLStockService : SFLIStockService
    {
        private readonly StockdbContext _context;

        public SFLStockService(StockdbContext context)
        {
            _context = context;
        }

        public async Task<StockPrevDayView> SFLGetGeneralIndicatorAsync(DateTime date)
        {
            int dayNo = await GetDayNumberByDateAsync(date);
            var result = await _context.StockPrevDayViews
                .AsNoTracking()
                .Where(x => x.Sticker == "TASI" && x.DayNo == dayNo)
                .FirstOrDefaultAsync();

            return result ?? new StockPrevDayView
            {
                Sticker = "TASI",
                Sname = await GetCompanyNameAsync("TASI"),
                Sopen = 0,
                Shigh = 0,
                Slow = 0,
                Sclose = 0,
                Svol = 0
            };
        }

        public async Task<List<RecommendationsResultsView>> SFLGetRecommendationsAsync(int followListId, DateTime date, string sortColumn, string sortOrder)
        {
            int dayNo = await GetDayNumberByDateAsync(date);
            var followList = await _context.FollowLists
                .Include(x => x.FollowListCompanies)
                .FirstAsync(x => x.Id == followListId);

            var recommendations = new List<RecommendationsResultsView>();
            foreach (var item in followList.FollowListCompanies)
            {
                var result = await _context.RecommendationsResultsViews
                    .Where(x => x.Sticker == item.CompanyCode && x.DayNo == dayNo)
                    .FirstOrDefaultAsync();
                if (result != null)
                {
                    recommendations.Add(result);
                }
            }

            // Apply sorting
            recommendations = sortColumn switch
            {
                "Sticker" => sortOrder == "asc" ? recommendations.OrderBy(x => x.Sticker).ToList() : recommendations.OrderByDescending(x => x.Sticker).ToList(),
                "Sname" => sortOrder == "asc" ? recommendations.OrderBy(x => x.Sname).ToList() : recommendations.OrderByDescending(x => x.Sname).ToList(),
                "Sclose" => sortOrder == "asc" ? recommendations.OrderBy(x => x.Sclose).ToList() : recommendations.OrderByDescending(x => x.Sclose).ToList(),
                "ExpectedOpen" => sortOrder == "asc" ? recommendations.OrderBy(x => x.ExpectedOpen).ToList() : recommendations.OrderByDescending(x => x.ExpectedOpen).ToList(),
                "OpeningGapRate" => sortOrder == "asc" ? recommendations.OrderBy(x => x.OpeningGapRate).ToList() : recommendations.OrderByDescending(x => x.OpeningGapRate).ToList(),
                "UpperLimitRate" => sortOrder == "asc" ? recommendations.OrderBy(x => x.UpperLimitRate).ToList() : recommendations.OrderByDescending(x => x.UpperLimitRate).ToList(),
                "LowerLimitRate" => sortOrder == "asc" ? recommendations.OrderBy(x => x.LowerLimitRate).ToList() : recommendations.OrderByDescending(x => x.LowerLimitRate).ToList(),
                "ChangeRate" => sortOrder == "asc" ? recommendations.OrderBy(x => x.ChangeRate).ToList() : recommendations.OrderByDescending(x => x.ChangeRate).ToList(),
                _ => recommendations.OrderBy(x => x.Sticker).ToList() // Default sorting
            };

            return recommendations;
        }

        public async Task<DateTime> SFLGetLastDateAsync()
        {
            var maxDate = await _context.StockTables.MaxAsync(x => x.Createddate);
            return maxDate ?? DateTime.Today;
        }

        public async Task<DateTime> SFLGetMinDateAsync()
        {
            var minDate = await _context.StockTables.MinAsync(x => x.Createddate);
            return minDate ?? DateTime.Today;
        }

        public async Task<DateTime> SFLGetMaxDateAsync()
        {
            var maxDate = await _context.StockTables.MaxAsync(x => x.Createddate);
            return maxDate ?? DateTime.Today;
        }

        public async Task<bool> SFLDateExistsAsync(DateTime date)
        {
            return await _context.StockTables.AnyAsync(x => x.Createddate == date);
        }

        public async Task<DateTime> SFLGetNextRecommendationDateAsync(DateTime currentDate)
        {
            var nextDate = await _context.StockTables
                .Where(x => x.Createddate > currentDate)
                .OrderBy(x => x.Createddate)
                .Select(x => x.Createddate)
                .FirstOrDefaultAsync();
            return nextDate ?? currentDate;
        }

        public async Task<DateTime> SFLGetPreviousRecommendationDateAsync(DateTime currentDate)
        {
            var prevDate = await _context.StockTables
                .Where(x => x.Createddate < currentDate)
                .OrderByDescending(x => x.Createddate)
                .Select(x => x.Createddate)
                .FirstOrDefaultAsync();
            return prevDate ?? currentDate;
        }

        private async Task<int> GetDayNumberByDateAsync(DateTime date)
        {
            // Simplified placeholder; implement actual logic based on your database
            var stock = await _context.StockTables
                .Where(x => x.Createddate == date)
                .FirstOrDefaultAsync();
            return stock?.DayNo ?? 0;
        }

        private async Task<string> GetCompanyNameAsync(string companyCode)
        {
            var company = await _context.CompanyTables
                .Where(x => x.CompanyCode == companyCode)
                .Select(x => x.CompanyName)
                .FirstOrDefaultAsync();
            return company ?? companyCode;
        }
    }
}