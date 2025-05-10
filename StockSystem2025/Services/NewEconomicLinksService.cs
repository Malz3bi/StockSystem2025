using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models;

namespace StockSystem2025.Services
{
    public class NewEconomicLinksService : INewEconomicLinksService
    {
        private readonly StockdbContext _context;

        public NewEconomicLinksService(StockdbContext context)
        {
            _context = context;
        }

        public async Task<List<EconLinksType>> GetLinkTypesAsync()
        {
            return await _context.EconLinksTypes.Include(t => t.EconomicLinks).ToListAsync();
        }

        public async Task<List<EconomicLink>> GetLinksByTypeIdAsync(int typeId)
        {
            return await _context.EconomicLinks.Where(l => l.TypeId == typeId).ToListAsync();
        }

        public async Task AddLinkTypeAsync(string typeName)
        {
            var type = new EconLinksType { TypeName = typeName };
            _context.EconLinksTypes.Add(type);
            await _context.SaveChangesAsync();
        }

        public async Task AddLinkAsync(string name, string link, int typeId)
        {
            var newLink = new EconomicLink { Name = name, Link = link, TypeId = typeId };
            _context.EconomicLinks.Add(newLink);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLinkAsync(int id, string name, string link, int typeId)
        {
            var existingLink = await _context.EconomicLinks.FindAsync(id);
            if (existingLink != null)
            {
                existingLink.Name = name;
                existingLink.Link = link;
                existingLink.TypeId = typeId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteLinkAsync(int id)
        {
            var link = await _context.EconomicLinks.FindAsync(id);
            if (link != null)
            {
                _context.EconomicLinks.Remove(link);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteLinkTypeAsync(int id)
        {
            var linkType = await _context.EconLinksTypes.Include(t => t.EconomicLinks).FirstOrDefaultAsync(t => t.Id == id);
            if (linkType != null)
            {
                _context.EconomicLinks.RemoveRange(linkType.EconomicLinks);
                _context.EconLinksTypes.Remove(linkType);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<EconomicLink> GetLinkByIdAsync(int id)
        {
            return await _context.EconomicLinks.FindAsync(id);
        }
    }
}