using StockSystem2025.Models;

namespace StockSystem2025.SFLServices
{
    public interface SFLIFollowListService
    {
        Task<List<FollowList>> SFLGetFollowListsAsync(string userId);
        Task SFLDeleteCompanyAsync(int followListId, string companyCode);
        Task SFLDeleteAllCompaniesAsync(int followListId);
    }
}