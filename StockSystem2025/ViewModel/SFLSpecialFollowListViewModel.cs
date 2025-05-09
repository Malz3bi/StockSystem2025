using System;
using System.Collections.Generic;
using StockSystem2025.Models;

namespace StockSystem2025.SFLModels
{
    public class SFLSpecialFollowListViewModel
    {
        public List<FollowList> SFLFollowLists { get; set; } = new List<FollowList>();
        public int SFLFollowListId { get; set; }
        public DateTime SFLRecommendationDate { get; set; }
        public DateTime SFLMinDate { get; set; }
        public DateTime SFLMaxDate { get; set; }
        public StockPrevDayView SFLGeneralIndicator { get; set; } = new StockPrevDayView();
        public List<RecommendationsResultsView> SFLRecommendations { get; set; } = new List<RecommendationsResultsView>();
        public string SFLSpecialCompanyColor { get; set; } = string.Empty;
        public bool SFLShowSecondView { get; set; }
        public string SFLSortColumn { get; set; } = string.Empty;
        public string SFLSortOrder { get; set; } = "asc";
        public string SFLStopLossColor { get; set; } = string.Empty;
        public string SFLFirstSupportColor { get; set; } = string.Empty;
        public string SFLSecondSupportColor { get; set; } = string.Empty;
        public string SFLFirstTargetColor { get; set; } = string.Empty;
        public string SFLSecondTargetColor { get; set; } = string.Empty;
        public string SFLThirdTargetColor { get; set; } = string.Empty;
    }
}