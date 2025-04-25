using StockSystem2025.Models;
using System.ComponentModel.DataAnnotations;

namespace StockSystem2025.ViewModel
{
    public class RecommendationResultViewModel
    {
        public int CriteriaId { get; set; }
        public string CriteriaName { get; set; }
        public string CriteriaType { get; set; }
        public string CriteriaNote { get; set; }
        public string CriteriaSeparater { get; set; }
        public int CriteriaIndex { get; set; }
        public DateTime SelectedDate { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public int IndicatorsUpCount { get; set; }
        public int IndicatorsDownCount { get; set; }
        public int IndicatorsNoChangeCount { get; set; }
        public int CompaniesUpCount { get; set; }
        public int CompaniesDownCount { get; set; }
        public int CompaniesNoChangeCount { get; set; }
        public List<GeneralIndicator> GeneralIndicators { get; set; }
        public List<FollowList> FollowLists { get; set; }
        public bool IsSecondView { get; set; }
        public double StopLossValue { get; set; }
        public double FirstSupportValue { get; set; }
        public double SecondSupportValue { get; set; }
        public double FirstTargetValue { get; set; }
        public double SecondTargetValue { get; set; }
        public double ThirdTargetValue { get; set; }
        public string? SpecialCompanyColor { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class GeneralIndicator
    {
        public string Sticker { get; set; }
        public string Sname { get; set; }
        public double Sopen { get; set; }
        public double Shigh { get; set; }
        public double Slow { get; set; }
        public double Sclose { get; set; }
        public double Svol { get; set; }
        public double ChangeValue { get; set; }
        public double ChangeRate { get; set; }
        public double IndicatorIn { get; set; }
        public double IndicatorOut { get; set; }
    }
}