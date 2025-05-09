namespace StockSystem2025.AppCode
{
    public static class ConvertHelper
    {
        public static string FormatDouble(double? value)
        {
            return value.HasValue ? $"{value:F2}" : "0";
        }

        public static string CompareValues(double? current, double? previous)
        {
            if (!current.HasValue || !previous.HasValue) return FormatDouble(current);
            var color = current > previous ? "green" : current < previous ? "red" : "black";
            return $"<span style='color: {color}'>{FormatDouble(current)}</span>";
        }

        public static string ChangeValue(double? value)
        {
            if (!value.HasValue) return "0.00";
            var color = value > 0 ? "green" : value < 0 ? "red" : "black";
            return $"<span style='color: {color}'>{FormatDouble(value)}</span>";
        }

        public static string ChangeValuePercent(double? value)
        {
            if (!value.HasValue) return "0.00%";
            var color = value > 0 ? "green" : value < 0 ? "red" : "black";
            return $"<span style='color: {color}'>{FormatDouble(value)}%</span>";
        }

        public static string CompareValuesAndControlValue(double? compareValue, double? controlValue, double percentage, bool isPercentage)
        {
            if (!compareValue.HasValue || !controlValue.HasValue) return isPercentage ? "0.00%" : "0.00";
            var color = compareValue > controlValue ? "green" : compareValue < controlValue ? "red" : "black";
            var value = isPercentage ? $"{FormatDouble(percentage)}%" : FormatDouble(compareValue);
            return $"<span style='color: {color}'>{value}</span>";
        }

        public static string StopLoss(double? sClose, double? sLow, double stopLossValue)
        {
            if (!sClose.HasValue) return "0.00";
            var value = sClose.Value - (sClose.Value * (stopLossValue / 100));
            var formatted = FormatDouble(value);
            if (sClose <= value) return $"<div style='width:70px;background-color:red;color:white'>{formatted}</div>";
            if (sLow <= value) return $"<div style='width:70px;color:red'>{formatted}</div>";
            return $"<div style='width:70px'>{formatted}</div>";
        }

        public static string FirstSupport(double? sClose, double? sLow, double firstSupportValue)
        {
            if (!sClose.HasValue) return "0.00";
            var value = sClose.Value - (sClose.Value * (firstSupportValue / 100));
            var formatted = FormatDouble(value);
            if (sClose <= value) return $"<div style='width:70px;background-color:red;color:white'>{formatted}</div>";
            if (sLow <= value) return $"<div style='width:70px;color:red'>{formatted}</div>";
            return $"<div style='width:70px'>{formatted}</div>";
        }

        public static string SecondSupport(double? sClose, double? sLow, double secondSupportValue)
        {
            if (!sClose.HasValue) return "0.00";
            var value = sClose.Value - (sClose.Value * (secondSupportValue / 100));
            var formatted = FormatDouble(value);
            if (sClose <= value) return $"<div style='width:70px;background-color:red;color:white'>{formatted}</div>";
            if (sLow <= value) return $"<div style='width:70px;color:red'>{formatted}</div>";
            return $"<div style='width:70px'>{formatted}</div>";
        }

        public static string FirstTarget(double? sClose, double? sLow, double firstTargetValue)
        {
            if (!sClose.HasValue) return "0.00";
            var value = sClose.Value + (sClose.Value * (firstTargetValue / 100));
            var formatted = FormatDouble(value);
            if (sClose >= value) return $"<div style='width:70px;background-color:green;color:white'>{formatted}</div>";
            if (sLow >= value) return $"<div style='width:70px;color:green'>{formatted}</div>";
            return $"<div style='width:70px'>{formatted}</div>";
        }

        public static string SecondTarget(double? sClose, double? sLow, double secondTargetValue)
        {
            if (!sClose.HasValue) return "0.00";
            var value = sClose.Value + (sClose.Value * (secondTargetValue / 100));
            var formatted = FormatDouble(value);
            if (sClose >= value) return $"<div style='width:70px;background-color:green;color:white'>{formatted}</div>";
            if (sLow >= value) return $"<div style='width:70px;color:green'>{formatted}</div>";
            return $"<div style='width:70px'>{formatted}</div>";
        }

        public static string ThirdTarget(double? sClose, double? sLow, double thirdTargetValue)
        {
            if (!sClose.HasValue) return "0.00";
            var value = sClose.Value + (sClose.Value * (thirdTargetValue / 100));
            var formatted = FormatDouble(value);
            if (sClose >= value) return $"<div style='width:70px;background-color:green;color:white'>{formatted}</div>";
            if (sLow >= value) return $"<div style='width:70px;color:green'>{formatted}</div>";
            return $"<div style='width:70px'>{formatted}</div>";
        }
    }
}