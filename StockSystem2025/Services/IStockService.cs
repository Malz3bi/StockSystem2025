using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models;
using StockSystem2025.Services;
using StockSystem2025.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockSystem2025.Services
{
    public interface IStockService
    {
        Task<List<StockPrevDayView>> GetGeneralIndicators(int dayNo);
        Task<List<StockPrevDayView>> GetStockPrevDayByDayNo(int dayNo);
        Task<DateTime> GetMinDate();
        Task<DateTime> GetMaxDate();
        Task<string> GetCompanyName(string sticker);
        Task<List<Setting>> GetSettings();
        Task<double?> GetMaxHigh(string sticker, int currentDayNo);
        Task<List<FollowList>> GetFollowLists(string userId);
        Task<Dictionary<string, FollowListCompanyInfo>> GetFollowListCompanies(string userId);
        Task AddToFollowList(int followListId, string companyCode);
        Task<int> GetDayNumberByDate(DateTime date);
        Task<DateTime> GetLastDate();
        Task<bool> CheckIfDateExists(DateTime date);
        Task<DateTime> GetRecommendationNextDate(DateTime currentDate);
        Task<DateTime> GetRecommendationPreviousDate(DateTime currentDate);
    }
}   



    public class StockService : IStockService
    {
        private readonly StockdbContext _context;

        public StockService(StockdbContext context)
        {
            _context = context;
        }

        public async Task<List<StockPrevDayView>> GetGeneralIndicators(int dayNo)
        {
            return await _context.StockPrevDayViews
                .AsNoTracking()
                .Where(x => x.Sticker == "TASI" && x.DayNo == dayNo)
                .ToListAsync();
        }

        public async Task<List<StockPrevDayView>> GetStockPrevDayByDayNo(int dayNo)
        {
            return await _context.StockPrevDayViews
                .AsNoTracking()
                .Where(x => x.DayNo == dayNo)
                .ToListAsync();
        }

        public async Task<DateTime> GetMinDate()
        {
            return await _context.StockTables.MinAsync(x => x.Createddate) ?? DateTime.Now;
        }

        public async Task<DateTime> GetMaxDate()
        {
            return await _context.StockTables.MaxAsync(x => x.Createddate) ?? DateTime.Now;
        }

        public async Task<string> GetCompanyName(string sticker)
        {
            var company = await _context.CompanyTables.FirstOrDefaultAsync(c => c.CompanyCode == sticker);
            return company?.CompanyName ?? sticker;
        }

        public async Task<List<Setting>> GetSettings()
        {
            return await _context.Settings.ToListAsync();
        }

        public async Task<double?> GetMaxHigh(string sticker, int currentDayNo)
        {
            return await _context.StockTables
                .Where(x => x.Sticker == sticker && x.DayNo < currentDayNo)
                .MaxAsync(x => x.Shigh);
        }

        public async Task<List<FollowList>> GetFollowLists(string userId)
        {
            return await _context.FollowLists
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

    public async Task<Dictionary<string, FollowListCompanyInfo>> GetFollowListCompanies(string userId)
    {
        var companies = await _context.FollowListCompanies
            .Where(fc => fc.FollowList.UserId == userId)
            .Select(fc => new
            {
                fc.CompanyCode,
                fc.FollowList.Name,
                fc.FollowList.Color
            })
            .ToListAsync();

        var result = new Dictionary<string, FollowListCompanyInfo>();
        foreach (var group in companies.GroupBy(c => c.CompanyCode))
        {
            var title = string.Join(", ", group.Select(g => g.Name));
            var color = group.First().Color;
            if (!string.IsNullOrEmpty(color) && color.Length > 3)
            {
                color = "#" + color.Substring(3) + "75";
            }
            result[group.Key] = new FollowListCompanyInfo
            {
                Title = title,
                Color = color
            };
        }
        return result;
    }
    public async Task AddToFollowList(int followListId, string companyCode)
        {
            var existing = await _context.FollowListCompanies
                .AnyAsync(x => x.FollowListId == followListId && x.CompanyCode == companyCode);
            if (!existing)
            {
                _context.FollowListCompanies.Add(new FollowListCompany
                {
                    FollowListId = followListId,
                    CompanyCode = companyCode
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetDayNumberByDate(DateTime date)
        {
            var stock = await _context.StockTables
                .Where(s => s.Createddate == date)
                .Select(s => s.DayNo)
                .FirstOrDefaultAsync();
            return stock;
        }

        public async Task<DateTime> GetLastDate()
        {
            return await _context.StockTables
                .OrderByDescending(s => s.Createddate)
                .Select(s => s.Createddate)
                .FirstOrDefaultAsync() ?? DateTime.Now;
        }

        public async Task<bool> CheckIfDateExists(DateTime date)
        {
            return await _context.StockTables
                .AnyAsync(s => s.Createddate == date);
        }

        public async Task<DateTime> GetRecommendationNextDate(DateTime currentDate)
        {
            return await _context.StockTables
                .Where(s => s.Createddate > currentDate)
                .OrderBy(s => s.Createddate)
                .Select(s => s.Createddate)
                .FirstOrDefaultAsync() ?? currentDate;
        }

        public async Task<DateTime> GetRecommendationPreviousDate(DateTime currentDate)
        {
            return await _context.StockTables
                .Where(s => s.Createddate < currentDate)
                .OrderByDescending(s => s.Createddate)
                .Select(s => s.Createddate)
                .FirstOrDefaultAsync() ?? currentDate;
        }
    }
