using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models;
using StockSystem2025.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockSystem2025.Services
{
    public class RecommendationsStockService
    {
        private readonly StockdbContext _db;
        private readonly ICriteriaService _criteriaService;
        private readonly ICurrentUserService _currentUserService;

        public RecommendationsStockService(StockdbContext db, ICriteriaService criteriaService, ICurrentUserService currentUserService)
        {
            _db = db;
            _criteriaService = criteriaService;
            _currentUserService = currentUserService;
        }

        public async Task<RecommendationResultViewModel> GetRecommendationResultAsync(int? criteriaId, DateTime? selectedDate, int viewIndex, string sortColumn, string sortOrder, List<string>? CompaniesSticer)
        {
            //var user = await _currentUserService.GetCurrentUserAsync();
            var userId = "1";
           
            selectedDate = (selectedDate ?? await GetLastDateAsync());
          var model = new RecommendationResultViewModel
            {
                SelectedDate = selectedDate ?? await GetLastDateAsync(),
                CurrentDayNo = await GetDayNumberByDateAsync(selectedDate),
                ViewIndex = viewIndex,
                SortColumn = sortColumn ?? "Sticker",
                SortOrder = sortOrder ?? "ASC"
            };

            // Settings
            var settings = await _db.Settings
                .Where(s => new[] { "StopLossValue", "FirstSupportValue", "SecondSupportValue",
                                    "FirstTargetValue", "SecondTargetValue", "ThirdTargetValue",
                                    "StopLossColor", "FirstSupportColor", "SecondSupportColor",
                                    "FirstTargetColor", "SecondTargetColor", "ThirdTargetColor" }
                                    .Contains(s.Name))
                .ToDictionaryAsync(s => s.Name, s => s.Value);

            model.SupportTargetSettings = new SupportTargetSettingsViewModel
            {
                StopLossValue = double.TryParse(settings.GetValueOrDefault("StopLossValue"), out var slv) ? slv : 0,
                FirstSupportValue = double.TryParse(settings.GetValueOrDefault("FirstSupportValue"), out var fsv) ? fsv : 0,
                SecondSupportValue = double.TryParse(settings.GetValueOrDefault("SecondSupportValue"), out var ssv) ? ssv : 0,
                FirstTargetValue = double.TryParse(settings.GetValueOrDefault("FirstTargetValue"), out var ftv) ? ftv : 0,
                SecondTargetValue = double.TryParse(settings.GetValueOrDefault("SecondTargetValue"), out var stv) ? stv : 0,
                ThirdTargetValue = double.TryParse(settings.GetValueOrDefault("ThirdTargetValue"), out var ttv) ? ttv : 0,
                StopLossColor = settings.GetValueOrDefault("StopLossColor") ?? "#FF0000",
                FirstSupportColor = settings.GetValueOrDefault("FirstSupportColor") ?? "#FFA500",
                SecondSupportColor = settings.GetValueOrDefault("SecondSupportColor") ?? "#FFFF00",
                FirstTargetColor = settings.GetValueOrDefault("FirstTargetColor") ?? "#00FF00",
                SecondTargetColor = settings.GetValueOrDefault("SecondTargetColor") ?? "#0000FF",
                ThirdTargetColor = settings.GetValueOrDefault("ThirdTargetColor") ?? "#800080"
            };

            // General Indicators
            int dayNo = model.CurrentDayNo  ;
            var generalIndicators = dayNo != 0
                ? await _db.StockPrevDayViews.AsNoTracking()
                    .Where(x => x.Sticker.Equals("TASI") && x.DayNo == dayNo)
                    .Select(x => new StockPrevDayViewModel
                    {
                        Sticker = x.Sticker,
                        Sname = x.Sname,
                        Sopen = x.Sopen,
                        Shigh = x.Shigh,
                        Slow = x.Slow,
                        Sclose = x.Sclose,
                        Svol = x.Svol,
                        ChangeValue = x.ChangeValue,
                        ChangeRate = x.ChangeRate,
                        IndicatorIn = x.IndicatorIn,
                        IndicatorOut = x.IndicatorOut,
                        IsIndicator = x.IsIndicator
                    }).ToListAsync()
                : new List<StockPrevDayViewModel>
                {
                    new StockPrevDayViewModel
                    {
                        Sticker = "TASI",
                        Sname = await GetCompanyNameAsync("TASI"),
                        Sopen = 0,
                        Shigh = 0,
                        Slow = 0,
                        Sclose = 0,
                        Svol = 0
                    }
                };
            model.GeneralIndicators = generalIndicators;

            // Stats
            model.SectorStats = new SectorStatsViewModel
            {
                UpCount = await _db.StockPrevDayViews.AsNoTracking()
                    .Where(x => x.IsIndicator == true && x.DayNo == dayNo && x.ChangeRate > 0)
                    .CountAsync(),
                DownCount = await _db.StockPrevDayViews.AsNoTracking()
                    .Where(x => x.IsIndicator == true && x.DayNo == dayNo && x.ChangeRate < 0)
                    .CountAsync(),
                NoChangeCount = await _db.StockPrevDayViews.AsNoTracking()
                    .Where(x => x.IsIndicator == true && x.DayNo == dayNo && x.ChangeRate == 0)
                    .CountAsync()
            };

            model.CompanyStats = new CompanyStatsViewModel
            {
                UpCount = await _db.StockPrevDayViews.AsNoTracking()
                    .Where(x => x.IsIndicator == false && x.DayNo == dayNo && x.ChangeRate > 0)
                    .CountAsync(),
                DownCount = await _db.StockPrevDayViews.AsNoTracking()
                    .Where(x => x.IsIndicator == false && x.DayNo == dayNo && x.ChangeRate < 0)
                    .CountAsync(),
                NoChangeCount = await _db.StockPrevDayViews.AsNoTracking()
                    .Where(x => x.IsIndicator == false && x.DayNo == dayNo && x.ChangeRate == 0)
                    .CountAsync()
            };

            // Criteria
            if (criteriaId.HasValue)
            {
                var criteria = await _criteriaService.GetCriteriaByIdAsync(criteriaId.Value);
                if (criteria != null)
                {
                    model.Criteria = new CrriteriaViewModel
                    {
                        Index = criteria.Id,
                        Separator = criteria.Separater,
                        Name = criteria.Name,
                        Notes = criteria.Note,
                        Type = criteria.Type
                    };
                }
            }

            // Date Range
            model.MinDate = await _db.StockTables.MinAsync(x => x.Createddate);
            model.MaxDate = await _db.StockTables.MaxAsync(x => x.Createddate);

            // Special Company Color
            model.SpecialCompanyColor = (await _db.Settings.FirstOrDefaultAsync(x => x.Name.Equals("SpecialCompanyColor")))?.Value ?? "#FFFFE0";

            // Follow Lists
            model.FollowLists = await _db.FollowLists
                .Where(f => f.UserId == userId)
                .Select(f => new FollowListViewModel
                {
                    Id = f.Id,
                    Name = f.Name,
                    Color = f.Color
                })
                .ToListAsync();

            // Recommendations
            List<StockPrevDayView> criteriaResult = criteriaId.HasValue
                ? await GetCriteriaRecommendationResultAsync(criteriaId.Value, selectedDate, CompaniesSticer)
                : new List<StockPrevDayView>();

            var stickers = criteriaResult.Select(x => x.Sticker).Distinct().ToList();
            var historicalData = await _db.StockTables
                .Where(x => stickers.Contains(x.Sticker) && x.DayNo <= model.CurrentDayNo)
                .GroupBy(x => x.Sticker)
                .Select(g => new
                {
                    Sticker = g.Key,
                    MinClose = g.Min(x => x.Sclose),
                    MinLow = g.Min(x => x.Slow),
                    MaxHigh = g.Max(x => x.Shigh),
                    Latest = g.OrderByDescending(x => x.DayNo).FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.Sticker, x => x);

            var recommendations = new List<StockRecommendationViewModel>();
            foreach (var item in criteriaResult)
            {
                var rec = await _db.RecommendationsResultsViews
                    .FirstOrDefaultAsync(x => x.Sticker.Equals(item.Sticker) && x.DayNo == item.DayNo);

                if (rec != null)
                {
                    var followLists = await _db.FollowListCompanies
                        .Where(x => x.CompanyCode == rec.Sticker)
                        .Select(x => x.FollowList)
                        .ToListAsync();

                    var histData = historicalData.GetValueOrDefault(rec.Sticker);
                    var recommendation = new StockRecommendationViewModel
                    {
                        Sticker = rec.Sticker,
                        Sname = rec.Sname,
                        SOpen = rec.PrevSopen, // Use PrevSopen as opening price
                        Shigh = rec.PrevShigh,
                        Slow = rec.PrevSlow,
                        Sclose = rec.Sclose,
                        NextSclose = rec.NextSclose,
                        ExpectedOpen = rec.ExpectedOpen,
                        ExpectedOpenValue = rec.ExpectedOpenValue,
                        ExpectedOpenPercent = rec.ExpectedOpenPercent,
                        PrevSopen = rec.PrevSopen,
                        OpeningGapValue = rec.OpeningGapValue,
                        OpeningGapRate = rec.OpeningGapRate,
                        PrevShigh = rec.PrevShigh,
                        UpperLimitValue = rec.UpperLimitValue,
                        UpperLimitRate = rec.UpperLimitRate,
                        PrevSlow = rec.PrevSlow,
                        LowerLimitValue = rec.LowerLimitValue,
                        LowerLimitRate = rec.LowerLimitRate,
                        PrevSclose = rec.PrevSclose,
                        ChangeValue = rec.ChangeValue,
                        ChangeRate = rec.ChangeRate,
                        IsIndicator = rec.IsIndicator,
                        IsSpecial = rec.IsSpecial,
                        FollowListNames = string.Join(", ", followLists.Select(f => f.Name)),
                        FollowListColor = followLists.Any() ? followLists.First().Color?.Substring(3) + "75" : null,
                        LastClose = histData?.Latest?.Sclose,
                        MaxHigh = histData?.MaxHigh,
                        MaxHighPercentage = histData.MaxHigh.HasValue && histData.Latest?.Sclose.HasValue == true
                            ? ((histData.MaxHigh.Value - histData.Latest.Sclose.Value) / histData.Latest.Sclose.Value) * 100
                            : (double?)null,
                        MinClose = histData?.MinClose,
                        MinLow = histData?.MinLow
                    };

                    if (viewIndex == 1)
                    {
                        // Additional logic for viewIndex == 1 if needed
                    }

                    recommendations.Add(recommendation);
                }
            }

            // Sorting
            recommendations = SortRecommendations(recommendations, model.SortColumn, model.SortOrder);
            model.Recommendations = recommendations;
            model.CompaniesSticer = CompaniesSticer;

            return model;
        }

        public async Task<bool> AddToFollowListAsync(int followListId, string companyCode)
        {
            var existing = await _db.FollowListCompanies
                .AnyAsync(x => x.FollowListId == followListId && x.CompanyCode == companyCode);

            if (!existing)
            {
                _db.FollowListCompanies.Add(new FollowListCompany
                {
                    FollowListId = followListId,
                    CompanyCode = companyCode
                });
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<DateTime?> GetNextDateAsync(DateTime? currentDate)
        {
            if (currentDate == null) return null;
            var result = await _db.StockTables
                .FirstOrDefaultAsync(x => x.Createddate == currentDate);
            if (result == null) return null;
            return await _db.StockTables
                .Where(x => x.Sticker.Equals(result.Sticker) && x.DayNo == result.DayNo - 1)
                .Select(x => x.Createddate)
                .FirstOrDefaultAsync();
        }

        public async Task<DateTime?> GetPreviousDateAsync(DateTime? currentDate)
        {
            if (currentDate == null) return null;
            var result = await _db.StockTables
                .FirstOrDefaultAsync(x => x.Createddate == currentDate);
            if (result == null) return null;
            return await _db.StockTables
                .Where(x => x.Sticker.Equals(result.Sticker) && x.DayNo == result.DayNo + 1)
                .Select(x => x.Createddate)
                .FirstOrDefaultAsync();
        }

        private async Task<DateTime> GetLastDateAsync()
        {
            return await _db.StockTables
                .OrderByDescending(x => x.Createddate)
                .Select(x => x.Createddate)
                .FirstOrDefaultAsync() ?? DateTime.Now;
        }

        private async Task<int> GetDayNumberByDateAsync(DateTime? date)
        {
            if (date == null) return 0;
            return await _db.StockTables
                .Where(x => x.Createddate == date)
                .Select(x => x.DayNo)
                .FirstOrDefaultAsync();
        }

        private async Task<string> GetCompanyNameAsync(string companyCode)
        {
            return await _db.CompanyTables
                .Where(c => c.CompanyCode == companyCode)
                .Select(c => c.CompanyName)
                .FirstOrDefaultAsync() ?? companyCode;
        }

        private async Task<List<StockPrevDayView>> GetCriteriaRecommendationResultAsync(int criteriaId, DateTime? selectedDate, List<string>? CompaniesSticer)
        {
            var criteria = await _criteriaService.GetCriteriaByIdAsync(criteriaId);
            if (criteria == null) return new List<StockPrevDayView>();

            var dayNo = await GetDayNumberByDateAsync(selectedDate);
            if (dayNo == 0) return new List<StockPrevDayView>();

            var query = new List<StockPrevDayView>().AsQueryable();
            if (CompaniesSticer!=null)
            {
                query = _db.StockPrevDayViews.Where(c => CompaniesSticer.Contains(c.Sticker)|| c.IsIndicator).AsNoTracking()
                .Where(s => s.DayNo == dayNo);
            }
            else
            {
                query = _db.StockPrevDayViews.Where(c => c.IsIndicator).AsNoTracking()
              .Where(s => s.DayNo == dayNo);
            }


              

            return await query.ToListAsync();
        }

        public async Task<bool> CheckIfDateExistsAsync(DateTime date)
        {
            return await _db.StockTables
                .AnyAsync(x => x.Createddate == date);
        }

        public async Task<double?> GetMinCloseAsync(string sticker, int currentDayNo)
        {
            return await _db.StockTables
                .Where(x => x.Sticker == sticker && x.DayNo <= currentDayNo)
                .MinAsync(x => (double?)x.Sclose) ?? 0;
        }

        public async Task<double?> GetMinLowAsync(string sticker, int currentDayNo)
        {
            return await _db.StockTables
                .Where(x => x.Sticker == sticker && x.DayNo <= currentDayNo)
                .MinAsync(x => (double?)x.Slow) ?? 0;
        }

        public async Task<double?> GetMaxHighAsync(string sticker, int currentDayNo)
        {
            return await _db.StockTables
                .Where(x => x.Sticker == sticker && x.DayNo <= currentDayNo)
                .MaxAsync(x => (double?)x.Shigh) ?? 0;
        }

        private List<StockRecommendationViewModel> SortRecommendations(List<StockRecommendationViewModel> recommendations, string sortColumn, string sortOrder)
        {
            var ordered = sortOrder == "DESC"
                ? recommendations.AsQueryable().OrderByDescending(x => GetProperty(x, sortColumn))
                : recommendations.AsQueryable().OrderBy(x => GetProperty(x, sortColumn));
            return ordered.ToList();
        }

        private object GetProperty(StockRecommendationViewModel model, string property)
        {
            return property switch
            {
                "Sticker" => model.Sticker,
                "Sname" => model.Sname,
                "ExpectedOpenValue" => model.ExpectedOpenValue ?? 0,
                "ExpectedOpenPercent" => model.ExpectedOpenPercent ?? 0,
                "OpeningGapValue" => model.OpeningGapValue ?? 0,
                "OpeningGapRate" => model.OpeningGapRate ?? 0,
                "UpperLimitValue" => model.UpperLimitValue ?? 0,
                "UpperLimitRate" => model.UpperLimitRate ?? 0,
                "LowerLimitValue" => model.LowerLimitValue ?? 0,
                "LowerLimitRate" => model.LowerLimitRate ?? 0,
                "ChangeValue" => model.ChangeValue ?? 0,
                "ChangeRate" => model.ChangeRate ?? 0,
                "SOpen" => model.SOpen ?? 0,
                "Shigh" => model.Shigh ?? 0,
                "Slow" => model.Slow ?? 0,
                "Sclose" => model.Sclose ?? 0,
                "SclosePercent" => model.MaxHighPercentage ?? 0,
                _ => model.Sticker
            };
        }
    }
}