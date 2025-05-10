using StockSystem2025.Models;

namespace StockSystem2025.Services
{
    public interface INewEconomicLinksService
    {
        Task<List<EconLinksType>> GetLinkTypesAsync();
        Task<List<EconomicLink>> GetLinksByTypeIdAsync(int typeId);
        Task AddLinkTypeAsync(string typeName);
        Task AddLinkAsync(string name, string link, int typeId);
        Task UpdateLinkAsync(int id, string name, string link, int typeId);
        Task DeleteLinkAsync(int id);
        Task DeleteLinkTypeAsync(int id);
        Task<EconomicLink> GetLinkByIdAsync(int id);
    }
}