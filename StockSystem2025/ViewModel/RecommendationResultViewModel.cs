namespace StockSystem2025.Models
{
    public class RecommendationResultViewModel
    {
        public List<StockPrevDayViewModel> GeneralIndicators { get; set; } = new List<StockPrevDayViewModel>();
        public List<StockRecommendationViewModel> Recommendations { get; set; } = new List<StockRecommendationViewModel>();
        public CrriteriaViewModel Criteria { get; set; }
        public SectorStatsViewModel SectorStats { get; set; }
        public CompanyStatsViewModel CompanyStats { get; set; }
        public DateTime? SelectedDate { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public bool ShowError { get; set; }
        public int CurrentDayNo { get; set; }
        public string SpecialCompanyColor { get; set; }
        public List<FollowListViewModel> FollowLists { get; set; } = new List<FollowListViewModel>();
        public int ViewIndex { get; set; }
        public SupportTargetSettingsViewModel SupportTargetSettings { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
    }

    public class StockPrevDayViewModel
    {
        public string Sticker { get; set; }
        public string Sname { get; set; }
        public double? Sopen { get; set; }
        public double? Shigh { get; set; }
        public double? Slow { get; set; }
        public double? Sclose { get; set; }
        public double? Svol { get; set; }
        public double? ChangeValue { get; set; }
        public double? ChangeRate { get; set; }
        public double? IndicatorIn { get; set; }
        public double? IndicatorOut { get; set; }
        public bool IsIndicator { get; set; }
    }

    public class StockRecommendationViewModel
    {
        public string Sticker { get; set; }
        public string Sname { get; set; }
        public double? SOpen { get; set; } // New property for opening price
        public double? Shigh { get; set; } // New property for highest price
        public double? Slow { get; set; }  // New property for lowest price
        public double? Sclose { get; set; }
        public double? NextSclose { get; set; }
        public double? ExpectedOpen { get; set; }
        public double? ExpectedOpenValue { get; set; }
        public double? ExpectedOpenPercent { get; set; }
        public double? PrevSopen { get; set; }
        public double? OpeningGapValue { get; set; }
        public double? OpeningGapRate { get; set; }
        public double? PrevShigh { get; set; }
        public double? UpperLimitValue { get; set; }
        public double? UpperLimitRate { get; set; }
        public double? PrevSlow { get; set; }
        public double? LowerLimitValue { get; set; }
        public double? LowerLimitRate { get; set; }
        public double? PrevSclose { get; set; }
        public double? ChangeValue { get; set; }
        public double? ChangeRate { get; set; }
        public bool IsIndicator { get; set; }
        public bool IsSpecial { get; set; }
        public string FollowListNames { get; set; }
        public string FollowListColor { get; set; }
        public double? LastClose { get; set; }
        public double? MaxHigh { get; set; }
        public double? MaxHighPercentage { get; set; }
        public double? MinClose { get; set; }
        public double? MinLow { get; set; }
    }

    public class CrriteriaViewModel
    {
        public int Index { get; set; }
        public string Separator { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Notes { get; set; }
    }

    public class SectorStatsViewModel
    {
        public int UpCount { get; set; }
        public int DownCount { get; set; }
        public int NoChangeCount { get; set; }
    }

    public class CompanyStatsViewModel
    {
        public int UpCount { get; set; }
        public int DownCount { get; set; }
        public int NoChangeCount { get; set; }
    }

    public class FollowListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class SupportTargetSettingsViewModel
    {
        public double StopLossValue { get; set; }
        public double FirstSupportValue { get; set; }
        public double SecondSupportValue { get; set; }
        public double FirstTargetValue { get; set; }
        public double SecondTargetValue { get; set; }
        public double ThirdTargetValue { get; set; }
        public string StopLossColor { get; set; }
        public string FirstSupportColor { get; set; }
        public string SecondSupportColor { get; set; }
        public string FirstTargetColor { get; set; }
        public string SecondTargetColor { get; set; }
        public string ThirdTargetColor { get; set; }
    }
}