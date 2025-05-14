namespace StockSystem2025.ViewModel
{
    public class HistoricalPeaksViewModel
    {
        public List<BreakoutCompany> BreakoutCompanies { get; set; } = new List<BreakoutCompany>();
    }

    public class BreakoutCompany
    {
        public string Sticker { get; set; }
        public string Name { get; set; }
        public double CurrentPrice { get; set; }
        public double ClosestPeakPrice { get; set; }
        public DateTime ClosestPeakDate { get; set; }
        public double BreakoutPercentage { get; set; }
    }

    public class HistoricalPeak
    {
        public DateTime Date { get; set; }
        public double HighPrice { get; set; }
    }

    public class HistoricalLowsViewModel
    {
        public List<BreakoutCompanylow> BreakoutCompanies { get; set; } = new List<BreakoutCompanylow>();
    }

    public class BreakoutCompanylow
    {
        public string Sticker { get; set; }
        public string Name { get; set; }
        public double CurrentPrice { get; set; }
        public double ClosestLowPrice { get; set; }
        public DateTime ClosestLowDate { get; set; }
        public double BreakoutPercentage { get; set; }
    }

    public class HistoricalLow
    {
        public DateTime Date { get; set; }
        public double LowPrice { get; set; }
    }
}