

namespace StockSystem2025.SFLServices
{
    public interface SFLISettingsService
    {
        Task<string> SFLGetSettingValueAsync(string name);
    }
}