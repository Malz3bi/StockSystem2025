using StockSystem2025.Models;


namespace StockSystem2025.ViewModels
{
    public class MarketFollowListViewModel
    {
        public DateTime CriteriaStartDate { get; set; }
        public List<StockPrevDayView> GeneralIndicators { get; set; }
        public List<StockPrevDayView> Stocks { get; set; }
        public List<FollowList> FollowLists { get; set; }
        public Dictionary<string, FollowListCompanyInfo> FollowListCompanies { get; set; }
        public Dictionary<string, WeeklyValue> WeeklyValues { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public double StopLossValue { get; set; }
        public double FirstSupportValue { get; set; }
        public double SecondSupportValue { get; set; }
        public double FirstTargetValue { get; set; }
        public double SecondTargetValue { get; set; }
        public double ThirdTargetValue { get; set; }
        public string SpecialCompanyColor { get; set; }
        public int CurrentDayNo { get; set; }
    }

    public class FollowListCompanyInfo
    {
        public string Color { get; set; }
        public string Title { get; set; }
    }

    public class WeeklyValue
    {
        public double MaxHigh { get; set; }
    }
}