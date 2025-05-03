using StockSystem2025.Models;

namespace StockSystem2025.ViewModel
{
    public class FormulasSettingViewModel
    {
        public string ContentTitle { get; set; }
        public List<CriteriaViewModel> Criteria { get; set; }
        public List<StockPrevDayView> GeneralIndicators { get; set; }
        public List<CompanyTable> Companies { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
    }

    public class CriteriaViewModel
    {
        public Criteria Criteria { get; set; }
        public int CompaniesCount { get; set; }
        public List<string> CompaniesSticer { get; set; }
    }

public class Formula1
    {
        public bool TypeAll { get; set; }
        public bool TypePositive { get; set; }
        public bool TypeNegative { get; set; }
        public bool TypeNoChange { get; set; }
        public double? GreaterThan { get; set; }
        public double? LessThan { get; set; }

        public Formula1()
        {
            TypeAll = true;
        }
    }
    public class Formula2
    {
        public double? GreaterThan { get; set; }
        public double? LessThan { get; set; }
    }
    public class Formula3
    {
        public bool TypeAll { get; set; }
        public bool TypeHigherGap { get; set; }
        public bool TypeLowerGap { get; set; }
        public double? GreaterThan { get; set; }
        public double? LessThan { get; set; }

        public Formula3()
        {
            TypeAll = true;
        }
    }
    public class Formula4
    {
        public bool TypeAll { get; set; }
        public bool TypeHigher { get; set; }
        public bool TypeLower { get; set; }
        public double? GreaterThan { get; set; }
        public double? LessThan { get; set; }

        public Formula4()
        {
            TypeAll = true;
        }






    }
    public class Formula5
    {
        public bool TypeAll { get; set; }
        public bool TypeHigherGap { get; set; }
        public bool TypeLowerGap { get; set; }


        public Formula5()
        {
            TypeAll = true;
        }
    }
    public class Formula6
    {
        public double? Between { get; set; }
        public double? And { get; set; }
    }
    public class Formula7
    {
        public double? Between { get; set; }
        public double? And { get; set; }
    }
    public class Formula8
    {
        public double? Between { get; set; }
        public double? And { get; set; }
    }
    public class Formula9
    {
        public bool TypeAll { get; set; }
        public bool TypePositive { get; set; }
        public bool TypeNegative { get; set; }
        public double? GreaterThan { get; set; }
        public double? LessThan { get; set; }

        public Formula9()
        {
            TypeAll = true;
        }
    }
    public class Formula10
    {
        public double? Between { get; set; }
        public double? And { get; set; }
    }
    public class Formula11
    {
        public bool MaximumAll { get; set; }
        public bool MaximumGreater { get; set; }
        public bool MaximumLess { get; set; }
        public double? MaximumBetween { get; set; }
        public double? MaximumAnd { get; set; }

        public bool MinimumAll { get; set; }
        public bool MinimumGreater { get; set; }
        public bool MinimumLess { get; set; }
        public double? MinimumBetween { get; set; }
        public double? MinimumAnd { get; set; }

        public Formula11()
        {
            MaximumAll = true;
            MinimumAll = true;
        }
    }
    public class Formula12
    {
        public bool TypeAll { get; set; }

        public bool TypeGreater { get; set; }
        public bool TypeLess { get; set; }
        public double? GreaterThan { get; set; }
        public double? LessThan { get; set; }

        public Formula12()
        {
            TypeAll = true;
        }
    }
    public class Formula13
    {
        public bool TypeAll { get; set; }
        public bool TypePositive { get; set; }
        public bool TypeNegative { get; set; }
        public int? Days { get; set; }
        public double? GreaterThan { get; set; }
        public double? LessThan { get; set; }

        public Formula13()
        {
            TypeAll = true;
        }
    }
    public class Formula14
    {
        public bool TypeAll { get; set; }
        public bool TypeGreater { get; set; }
        public bool TypeLess { get; set; }
        public double? GreaterThan { get; set; }
        public double? LessThan { get; set; }

        public Formula14()
        {
            TypeAll = true;
        }
    }
    public class Formula15
    {
        public double? Between { get; set; }
        public double? And { get; set; }
    }
    public class Formula16
    {
        public bool TypeAll { get; set; }
        public bool TypePositive { get; set; }
        public double? BetweenPositive { get; set; }
        public double? AndPositive { get; set; }
        public bool TypeNegative { get; set; }
        public double? BetweenNegative { get; set; }
        public double? AndNegative { get; set; }

        public Formula16()
        {
            TypeAll = true;
        }
    }
    public class Formula17
    {
        public bool TypeAll { get; set; }
        public bool TypeGreater { get; set; }
        public bool TypeLess { get; set; }
        public int? FromDays { get; set; }
        public int? ToDays { get; set; }
        public double? GreaterThan { get; set; }
        public double? LessThan { get; set; }

        public Formula17()
        {
            TypeAll = true;
        }
    }


    public class Formula17PenetrationObject
    {
        public double LatestHigh { get; set; }
        public RecommendationsResultsView StockItem { get; set; }
    }

}