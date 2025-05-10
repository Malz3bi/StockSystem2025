using StockSystem2025.Models;

namespace StockSystem2025.ViewModel
{
    public class NewEconomicLinksViewModel
    {
        public int? SelectedTypeId { get; set; }
        public string TypeName { get; set; }
        public string LinkName { get; set; }
        public string LinkUrl { get; set; }
        public int? EditLinkId { get; set; }
        public List<EconLinksType> LinkTypes { get; set; }
        public List<(EconLinksType Type, List<EconomicLink> Links)> GroupedLinks { get; set; }
    }
}