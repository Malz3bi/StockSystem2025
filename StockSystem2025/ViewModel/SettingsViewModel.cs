using System.ComponentModel.DataAnnotations;

namespace StockSystem2025.Models
{
    public class SettingsViewModel
    {
        // General Settings
        [Required(ErrorMessage = "حقل اسم البرنامج مطلوب")]
        [StringLength(50, ErrorMessage = "اسم البرنامج يجب ألا يتجاوز 50 حرف")]
        public string SiteName { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "يجب أن يكون عدد أيام RSI موجبًا")]
        public int RSIDays { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "يجب أن يكون عدد أيام Williams موجبًا")]
        public int Williams { get; set; }

        [Range(1, 15, ErrorMessage = "يجب أن يكون عدد أيام التداول بين 1 و 15")]
        public int StockDays { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "يجب أن يكون عدد أيام الرسم البياني موجبًا")]
        public int ChartDays { get; set; }

        // Candlestick Colors
        [StringLength(7, MinimumLength = 7, ErrorMessage = "يجب أن يكون اللون بصيغة HEX (مثل #FFFFFF)")]
        public string UpColor1 { get; set; } = "#FFFFFF"; // Default to white
        [StringLength(7, MinimumLength = 7, ErrorMessage = "يجب أن يكون اللون بصيغة HEX (مثل #FFFFFF)")]
        public string UpColor2 { get; set; } = "#00FF00"; // Default to green
        [StringLength(7, MinimumLength = 7, ErrorMessage = "يجب أن يكون اللون بصيغة HEX (مثل #FFFFFF)")]
        public string DownColor1 { get; set; } = "#FF0000"; // Default to red
        [StringLength(7, MinimumLength = 7, ErrorMessage = "يجب أن يكون اللون بصيغة HEX (مثل #FFFFFF)")]
        public string DownColor2 { get; set; } = "#000000"; // Default to black

        // Instant Update Settings
        [StringLength(50, ErrorMessage = "اسم الصفحة في ملف الإكسل يجب ألا يتجاوز 50 حرف")]
        public string ExcelSheetName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "يجب أن يكون رقم العمود موجبًا")]
        public int StickerColNo { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن يكون رقم العمود موجبًا")]
        public int SnameColNo { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن يكون رقم العمود موجبًا")]
        public int SopenColNo { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن يكون رقم العمود موجبًا")]
        public int SHighColNo { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن يكون رقم العمود موجبًا")]
        public int SLowColNo { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن يكون رقم العمود موجبًا")]
        public int SCloseColNo { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن يكون رقم العمود موجبًا")]
        public int SvolColNo { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن يكون رقم العمود موجبًا")]
        public int ExpectedOpenColNo { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "يجب أن يكون رقم العمود موجبًا")]
        public int SDateColNo { get; set; }

        [Range(0, 10, ErrorMessage = "يجب أن تكون قيمة التحديث التلقائي بين 0 و 10 دقائق")]
        public int InstantUpdateValue { get; set; }

        // Other Options
        public bool ShowCompaniesCount { get; set; }
        public bool ClearData { get; set; }
        public bool ResetColors { get; set; }
    }
}