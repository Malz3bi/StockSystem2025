using System;

namespace StockSystem2025.AppCode
{
    public static class ControlHelper
    {
        public static string GetChangeValueControl(double? value)
        {
            if (!value.HasValue) return "0";
            string color = value > 0 ? "green" : value < 0 ? "red" : "black";
            return $"<span style=\"color: {color}\">{value:F2}</span>";
        }

        public static string GetChangeValueControlPercent(double? value)
        {
            if (!value.HasValue) return "0%";
            string color = value > 0 ? "green" : value < 0 ? "red" : "black";
            return $"<span style=\"color: {color}\">{value:F2}%</span>";
        }

        public static string GetControlWithCompareValues(double? value1, double? value2)
        {
            if (!value1.HasValue || !value2.HasValue) return ConvertHelper.FormatDouble(value1);
            string color = value1 > value2 ? "green" : value1 < value2 ? "red" : "black";
            return $"<span style=\"color: {color}\">{ConvertHelper.FormatDouble(value1)}</span>";
        }

        public static string GetControlWithCompareValuesAndControlValue(double? value1, double? value2, double? controlValue, bool isPercent)
        {
            if (!value1.HasValue || !value2.HasValue || !controlValue.HasValue) return isPercent ? "0%" : "0";
            string color = controlValue > 0 ? "green" : controlValue < 0 ? "red" : "black";
            string formattedValue = isPercent ? $"{controlValue:F2}%" : $"{controlValue:F2}";
            return $"<span style=\"color: {color}\">{formattedValue}</span>";
        }

        public static string GetExpectedOpenControlWithCompareValuesAndControlValue(double? expectedOpen, double? Sclose, double? controlValue, bool isPercent, double? compareValue)
        {
            if (!expectedOpen.HasValue || !Sclose.HasValue || !controlValue.HasValue) return isPercent ? "0%" : "0";
            string color = controlValue > 0 ? "green" : controlValue < 0 ? "red" : "black";
            string formattedValue = isPercent ? $"{controlValue:F2}%" : $"{controlValue:F2}";
            return $"<span style=\"color: {color}\">{formattedValue}</span>";
        }

        public static string GetStopLossChangeValueColor(double stopLossValue, double minClose)
        {
            string color = stopLossValue > minClose ? "red" : "green";
            return $"<span style=\"color: {color}\">{ConvertHelper.FormatDouble(stopLossValue)}</span>";
        }

        public static string GetSupportChangeValueColor(double supportValue, double minLow)
        {
            string color = supportValue > minLow ? "red" : "green";
            return $"<span style=\"color: {color}\">{ConvertHelper.FormatDouble(supportValue)}</span>";
        }

        public static string GetTargetChangeValueColor(double targetValue, double maxHigh)
        {
            string color = targetValue < maxHigh ? "green" : "red";
            return $"<span style=\"color: {color}\">{ConvertHelper.FormatDouble(targetValue)}</span>";
        }
    }
}