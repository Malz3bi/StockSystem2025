using StockSystem2025.Models;

namespace StockSystem2025.SFLServices
{
    public interface SFLIStockService
    {
        Task<StockPrevDayView> SFLGetGeneralIndicatorAsync(DateTime date);
        Task<List<RecommendationsResultsView>> SFLGetRecommendationsAsync(int followListId, DateTime date, string sortColumn, string sortOrder);
        Task<DateTime> SFLGetLastDateAsync();
        Task<DateTime> SFLGetMinDateAsync();
        Task<DateTime> SFLGetMaxDateAsync();
        Task<bool> SFLDateExistsAsync(DateTime date);
        Task<DateTime> SFLGetNextRecommendationDateAsync(DateTime currentDate);
        Task<DateTime> SFLGetPreviousRecommendationDateAsync(DateTime currentDate);
    }
}