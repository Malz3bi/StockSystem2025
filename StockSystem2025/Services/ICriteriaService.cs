using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using StockSystem2025.Models;
using System.Threading.Tasks;

namespace StockSystem2025.Services
{
    public interface ICriteriaService
    {
        Task<Criteria?> GetCriteriaByIdAsync(int id);
        Task SaveCriteriaAsync(Criteria criteria);
        Task<string> GetDateByDayNumberAsync(int dayNo);
    }


        public class CriteriaService : ICriteriaService
        {
            private readonly StockdbContext _context;

            public CriteriaService(StockdbContext context)
            {
                _context = context;
            }

            public async Task<Criteria?> GetCriteriaByIdAsync(int id)
            {
                return await _context.Criterias
                    .Include(c => c.Formulas)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }

            public async Task SaveCriteriaAsync(Criteria criteria)
            {
                if (criteria.Id == 0)
                {
                    _context.Criterias.Add(criteria);
                }
                else
                {
                    var existing = await _context.Criterias
                        .Include(c => c.Formulas)
                        .FirstOrDefaultAsync(c => c.Id == criteria.Id);
                    if (existing != null)
                    {
                        _context.Formulas.RemoveRange(existing.Formulas);
                        existing.Formulas.Clear();
                        _context.Entry(existing).CurrentValues.SetValues(criteria);
                        existing.Formulas.AddRange(criteria.Formulas);
                    }
                }
                await _context.SaveChangesAsync();
            }

            public async Task<string> GetDateByDayNumberAsync(int dayNo)
            {
                var stockTable = await _context.StockTables
                    .FirstOrDefaultAsync(x => x.DayNo == dayNo);
                return stockTable?.Createddate?.ToString("dd-MM-yyyy م") ?? System.DateTime.Now.ToString("dd-MM-yyyy م");
            }
        }
    }
