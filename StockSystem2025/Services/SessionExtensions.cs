
namespace StockSystem2025.Extensions
{
    public static class SessionExtensions
    {
        private const string RecommendationStartDateKey = "RecommendationStartDate";

        public static void SetRecommendationStartDate(this ISession session, DateTime date)
        {
            session.SetString(RecommendationStartDateKey, date.ToString("o"));
        }

        public static DateTime? GetRecommendationStartDate(this ISession session)
        {
            var value = session.GetString(RecommendationStartDateKey);
            return value != null && DateTime.TryParse(value, out var date) ? date : null;
        }
    }
}