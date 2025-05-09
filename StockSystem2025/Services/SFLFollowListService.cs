using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models;
using StockSystem2025.SFLServices;


namespace StockSystem2025.Services
{
    public class SFLFollowListService : SFLIFollowListService
    {
        private readonly StockdbContext _context;

        public SFLFollowListService(StockdbContext context)
        {
            _context = context;
        }

        public async Task<List<FollowList>> SFLGetFollowListsAsync(int userId)
        {
            return await _context.FollowLists
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        public async Task SFLDeleteCompanyAsync(int followListId, string companyCode)
        {
            var company = await _context.FollowListCompanies
                .FirstOrDefaultAsync(x => x.FollowListId == followListId && x.CompanyCode == companyCode);
            if (company != null)
            {
                _context.FollowListCompanies.Remove(company);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SFLDeleteAllCompaniesAsync(int followListId)
        {
            var companies = await _context.FollowListCompanies
                .Where(x => x.FollowListId == followListId)
                .ToListAsync();
            _context.FollowListCompanies.RemoveRange(companies);
            await _context.SaveChangesAsync();
        }
    }
}