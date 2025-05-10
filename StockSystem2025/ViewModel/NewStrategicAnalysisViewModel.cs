using StockSystem2025.Models;

namespace StockSystem2025.ViewModels
{
    public class NewStrategicAnalysisViewModel
    {
        public int SelectedTabIndex { get; set; }
        public List<DigitalAnalysis> DigitalAnalyses { get; set; }
        public List<ProfessionalFibonacci> FibonacciAnalyses { get; set; }
    }
}