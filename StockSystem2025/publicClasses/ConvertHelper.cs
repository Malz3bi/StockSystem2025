namespace StockSystem2025.AppCode
{
    public static class ConvertHelper
    {
        public static string FormatDouble(double? value)
        {
            return value.HasValue ? $"{value:F2}" : "0";
        }
    }
}