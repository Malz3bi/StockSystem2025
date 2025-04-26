namespace StockSystem2025.ViewModel
{
    public class RecommendationViewModel
    {
        public string Sticker { get; set; }
        public string Sname { get; set; }
        public double Sclose { get; set; }
        public double PrevSclose { get; set; }
        public double NextSclose { get; set; }
        public double PrevSopen { get; set; }
        public double PrevShigh { get; set; }
        public double PrevSlow { get; set; }
        public double ExpectedOpen { get; set; }
        public double ExpectedOpenValue { get; set; }
        public double ExpectedOpenPercent { get; set; }
        public double OpeningGapValue { get; set; }
        public double OpeningGapRate { get; set; }
        public double UpperLimitValue { get; set; }
        public double UpperLimitRate { get; set; }
        public double LowerLimitValue { get; set; }
        public double LowerLimitRate { get; set; }
        public double ChangeValue { get; set; }
        public double ChangeRate { get; set; }
        public bool IsIndicator { get; set; }
        public bool IsSpecial { get; set; }
        public string Tooltip { get; set; } // لعرض قوائم المتابعة
        public string BackgroundColor { get; set; } // لون خلفية الصف
        // خصائص منسقة للعرض
        public string FormattedSclose { get; set; }
        public string FormattedExpectedOpen { get; set; }
        public string FormattedExpectedOpenValue { get; set; }
        public string FormattedExpectedOpenPercent { get; set; }
        public string FormattedPrevSopen { get; set; }
        public string FormattedOpeningGapValue { get; set; }
        public string FormattedOpeningGapRate { get; set; }
        public string FormattedPrevShigh { get; set; }
        public string FormattedUpperLimitValue { get; set; }
        public string FormattedUpperLimitRate { get; set; }
        public string FormattedPrevSlow { get; set; }
        public string FormattedLowerLimitValue { get; set; }
        public string FormattedLowerLimitRate { get; set; }
        public string FormattedPrevSclose { get; set; }
        public string FormattedChangeValue { get; set; }
        public string FormattedChangeRate { get; set; }
        // خصائص لـ SupportResistance
        public string FormattedStopLoss { get; set; }
        public string StopLossColor { get; set; }
        public string FormattedSecondSupport { get; set; }
        public string SecondSupportColor { get; set; }
        public string FormattedFirstSupport { get; set; }
        public string FirstSupportColor { get; set; }
        public string FormattedFirstTarget { get; set; }
        public string FirstTargetColor { get; set; }
        public string FormattedSecondTarget { get; set; }
        public string SecondTargetColor { get; set; }
        public string FormattedThirdTarget { get; set; }
        public string ThirdTargetColor { get; set; }
        public string FormattedLastClose { get; set; }
        public string FormattedMaxHigh { get; set; }
        public string FormattedMaxHighPercentage { get; set; }
    }

    public class GeneralIndicatorViewModel
    {
        public string Sticker { get; set; }
        public string Sname { get; set; }
        public double Sopen { get; set; }
        public double Shigh { get; set; }
        public double Slow { get; set; }
        public double Sclose { get; set; }
        public double ChangeValue { get; set; }
        public double ChangeRate { get; set; }
        public double Svol { get; set; }
        public double IndicatorIn { get; set; }
        public double IndicatorOut { get; set; }
        public int DayNo { get; set; }
        // خصائص منسقة
        public string FormattedSopen { get; set; }
        public string FormattedShigh { get; set; }
        public string FormattedSlow { get; set; }
        public string FormattedSclose { get; set; }
        public string FormattedChangeValue { get; set; }
        public string FormattedChangeRate { get; set; }
        public string FormattedIndicatorIn { get; set; }
        public string FormattedIndicatorOut { get; set; }
    }

    public class FollowListViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class RecommendationResultViewModel
    {
        public int criteriaId { get; set; }
        public DateTime? SelectedDate { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public string ViewMode { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
        public GeneralIndicatorViewModel GeneralIndicator { get; set; }
        public int IndicatorsUpCount { get; set; }
        public int IndicatorsDownCount { get; set; }
        public int IndicatorsNoChangeCount { get; set; }
        public int CompaniesUpCount { get; set; }
        public int CompaniesDownCount { get; set; }
        public int CompaniesNoChangeCount { get; set; }
        public int CriteriaIndex { get; set; }
        public string CriteriaSeparater { get; set; }
        public string CriteriaTitle { get; set; }
        public string CriteriaType { get; set; }
        public string CriteriaNotes { get; set; }
        public List<RecommendationViewModel> Recommendations { get; set; }
        public List<FollowListViewModel> FollowLists { get; set; }
        public string SpecialCompanyColor { get; set; }
        public double StopLossValue { get; set; }
        public double SecondSupportValue { get; set; }
        public double FirstSupportValue { get; set; }
        public double FirstTargetValue { get; set; }
        public double SecondTargetValue { get; set; }
        public double ThirdTargetValue { get; set; }
    }
}