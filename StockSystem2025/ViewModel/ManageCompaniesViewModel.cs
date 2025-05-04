namespace StockSystem2025.Models
{
    public class ManageCompaniesViewModel
    {
        public int IndicatorId { get; set; }
        public string IndicatorCode { get; set; } = string.Empty;
        public string IndicatorName { get; set; } = string.Empty;
        public List<CompanyTable> ExistingCompanies { get; set; } = new List<CompanyTable>();
        public List<CompanyTable> RemainingCompanies { get; set; } = new List<CompanyTable>();
        public List<string> SelectedExistingCompanies { get; set; } = new List<string>();
        public List<string> SelectedRemainingCompanies { get; set; } = new List<string>();
    }
}