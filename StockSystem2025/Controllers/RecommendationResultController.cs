using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models;
using StockSystem2025.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static StockSystem2025.Controllers.FormulasSettingController;

namespace StockSystem2025.Controllers
{


    // Controller
    public class RecommendationResultController : Controller
    {
        private readonly StockdbContext _context;
        private readonly ILogger<RecommendationResultController> _logger;

        public RecommendationResultController(ILogger<RecommendationResultController> logger, StockdbContext context)
        {
            _logger = logger;
            _context = context;
        }




        public async Task<IActionResult> Index(int? id, DateTime? date, string viewMode = "ProfitLoss", string sortColumn = "Sticker", string sortOrder = "ASC")
        {
            if (!id.HasValue) return NotFound();
            var userId = 1; // Replace with actual user ID
            var model = await GetRecommendationResultAsync(id.Value, date, viewMode, sortColumn, sortOrder, userId);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeDate(int criteriaId, DateTime selectedDate, string viewMode, string sortColumn, string sortOrder)
        {
            var userId = 1; // Replace with actual user ID
            var model = await GetRecommendationResultAsync(criteriaId, selectedDate, viewMode, sortColumn, sortOrder, userId);
            if (await GetDayNumberByDateAsync(selectedDate) == 1)
            {
                ModelState.AddModelError("", "يرجى إدخال تاريخ صحيح");
            }
            return View("Index", model);
        }
        [HttpGet]
        public async Task<IActionResult> GetRecommendations(int? id, DateTime? date, string viewMode, string sortColumn, string sortOrder)
        {
            if (!id.HasValue) return Json(new { success = false, message = "Invalid criteria ID" });
            var userId = 1; // Replace with actual user ID
            var model = await GetRecommendationResultAsync(id.Value, date, viewMode, sortColumn, sortOrder, userId);
            return Json(new { success = true, data = model });
        }
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> GetNextDate(DateTime? selectedDate)
        {
            if (!selectedDate.HasValue)
            {
                return Json(new { success = false, message = "Selected date is required" });
            }

            // Get the DayNo for the selected date
            var currentDayNo = await _context.StockTables
                .Where(s => s.Sdate == selectedDate.Value.ToString("yyyy-MM-dd"))
                .Select(s => s.DayNo)
                .FirstOrDefaultAsync();

            // Find the next date where DayNo is greater than currentDayNo
            var nextDate = await _context.StockTables
                .Where(x => x.DayNo > currentDayNo)
                .OrderBy(x => x.DayNo) // Ensure we get the earliest next date
                .Select(x => x.Sdate)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(nextDate))
            {
                return Json(new { success = false, message = "No next date available" });
            }

            // Validate that Sdate is in the correct format
            if (!DateTime.TryParse(nextDate, out var parsedNextDate))
            {
                return Json(new { success = false, message = "Invalid date format in Sdate" });
            }

            return Json(new { success = true, nextDate = parsedNextDate.ToString("yyyy-MM-dd") });
        }
        [HttpPost]
        public async Task<IActionResult> GetPreviousDate(DateTime? selectedDate)
        {
            if (!selectedDate.HasValue)
            {
                return Json(new { success = false, message = "Selected date is required" });
            }

            // Get the DayNo for the selected date
            var currentDayNo = await _context.StockTables
                .Where(s => s.Sdate == selectedDate.Value.ToString("yyyy-MM-dd"))
                .Select(s => s.DayNo)
                .FirstOrDefaultAsync();

            // Find the previous date where DayNo is less than currentDayNo
            var prevDate = await _context.StockTables
                .Where(x => x.DayNo < currentDayNo)
                .OrderByDescending(x => x.DayNo) // Ensure we get the latest previous date
                .Select(x => x.Sdate)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(prevDate))
            {
                return Json(new { success = false, message = "No previous date available" });
            }

            // Validate that Sdate is in the correct format
            if (!DateTime.TryParse(prevDate, out var parsedPrevDate))
            {
                return Json(new { success = false, message = "Invalid date format in Sdate" });
            }

            return Json(new { success = true, prevDate = parsedPrevDate.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        public async Task<IActionResult> AddToFollowList(int criteriaId, int followListId, string companyCode, DateTime? selectedDate, string viewMode, string sortColumn, string sortOrder)
        {
            var exists = await _context.FollowListCompanies
                .AnyAsync(x => x.FollowListId == followListId && x.CompanyCode == companyCode);
            if (!exists)
            {
                _context.FollowListCompanies.Add(new FollowListCompany
                {
                    FollowListId = followListId,
                    CompanyCode = companyCode
                });
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Company already in follow list" });
        }

        private async Task<RecommendationResultViewModel> GetRecommendationResultAsync(int criteriaId, DateTime? date, string viewMode, string sortColumn, string sortOrder, int userId)
        {
            var model = new RecommendationResultViewModel
            {
                criteriaId = criteriaId,
                SelectedDate = date ?? DateTime.Today,
                ViewMode = viewMode,
                SortColumn = sortColumn,
                SortOrder = sortOrder,
              
                SpecialCompanyColor = "#FFFF0075", // Example color
                StopLossValue = 5.0, // Example values
                SecondSupportValue = 3.0,
                FirstSupportValue = 1.5,
                FirstTargetValue = 1.5,
                SecondTargetValue = 3.0,
                ThirdTargetValue = 5.0
            };

            model.MinDate = _context.StockTables
                .AsEnumerable()
                .Min(x => DateTime.Parse(x.Sdate));
            
            model.MaxDate =  _context.StockTables.AsEnumerable()
                .Max(x => DateTime.Parse(x.Sdate));





            // General Indicator (Aggregated across all stocks)
            var currentStocks = await _context.StockTables
                .Where(x => x.Sdate == model.MaxDate.Value.ToString("yyyy/MM/dd"))
                .ToListAsync();

            if (currentStocks.Any())
            {
                // Aggregate current day's data
                var avgSclose = currentStocks.Average(x => x.Sclose ?? 0);
                var avgSopen = currentStocks.Average(x => x.Sopen ?? 0);
                var avgShigh = currentStocks.Average(x => x.Shigh ?? 0);
                var avgSlow = currentStocks.Average(x => x.Slow ?? 0);
                var sumSvol = currentStocks.Sum(x => x.Svol ?? 0);

                // Get previous day's data (aggregate across all stocks)
                var GprevDayNo = currentStocks.Min(x => x.DayNo) + 1;
                var prevStocks = await _context.StockTables
                    .Where(x => x.DayNo == GprevDayNo)
                    .ToListAsync();

                double changeValue = 0;
                double changeRate = 0;
                if (prevStocks.Any())
                {
                    var prevAvgSclose = prevStocks.Average(x => x.Sclose ?? 0);
                    changeValue = avgSclose - prevAvgSclose;
                    changeRate = prevAvgSclose != 0 ? (changeValue / prevAvgSclose) * 100 : 0;
                }

                model.GeneralIndicator = new GeneralIndicatorViewModel
                {
                    Sticker = "TASI", // Placeholder for market index identifier
                    Sname = " المؤشر العام", // Placeholder name
                    Sopen = avgSopen,
                    Shigh = avgShigh,
                    Slow = avgSlow,
                    Sclose = avgSclose,
                    ChangeValue = changeValue,
                    ChangeRate = changeRate,
                    Svol = sumSvol,
                    IndicatorIn = 0, // TODO: Replace with actual logic
                    IndicatorOut = 0, // TODO: Replace with actual logic
                    DayNo = currentStocks.First().DayNo,
                    FormattedSopen = ConvertHelper.FormatDouble(avgSopen),
                    FormattedShigh = ConvertHelper.FormatDouble(avgShigh),
                    FormattedSlow = ConvertHelper.FormatDouble(avgSlow),
                    FormattedSclose = ConvertHelper.FormatDouble(avgSclose),
                    FormattedChangeValue = ConvertHelper.GetChangeValueControl(changeValue),
                    FormattedChangeRate = ConvertHelper.GetChangeValueControlPercent(changeRate),
                    FormattedIndicatorIn = ConvertHelper.FormatDouble(0), // TODO: Replace
                    FormattedIndicatorOut = ConvertHelper.FormatDouble(0) // TODO: Replace
                };
            }
            else
            {
                // Handle case where no data is available for the selected date
                model.GeneralIndicator = new GeneralIndicatorViewModel
                {
                    Sticker = "MARKET_INDEX",
                    Sname = "General Market Index",
                    Sopen = 0,
                    Shigh = 0,
                    Slow = 0,
                    Sclose = 0,
                    ChangeValue = 0,
                    ChangeRate = 0,
                    Svol = 0,
                    IndicatorIn = 0,
                    IndicatorOut = 0,
                    DayNo = 0,
                    FormattedSopen = ConvertHelper.FormatDouble(0),
                    FormattedShigh = ConvertHelper.FormatDouble(0),
                    FormattedSlow = ConvertHelper.FormatDouble(0),
                    FormattedSclose = ConvertHelper.FormatDouble(0),
                    FormattedChangeValue = ConvertHelper.GetChangeValueControl(0),
                    FormattedChangeRate = ConvertHelper.GetChangeValueControlPercent(0),
                    FormattedIndicatorIn = ConvertHelper.FormatDouble(0),
                    FormattedIndicatorOut = ConvertHelper.FormatDouble(0)
                };

            }



                // Stats (Indicators and Companies)
                var stockData = await _context.StockTables
                .Where(x => x.Sdate == model.MaxDate.Value.ToString("yyyy/MM/dd"))
                .ToListAsync();
            var prevDayNo = await _context.StockTables
                .Where(x => x.Sdate == model.MaxDate.Value.ToString("yyyy/MM/dd"))
                .Select(x => x.DayNo)
                .FirstOrDefaultAsync();
            var prevStockData = await _context.StockTables
                .Where(x => x.DayNo == prevDayNo + 1)
                .ToListAsync();

            model.IndicatorsUpCount = stockData
                .Count(x => x.Sclose.HasValue && prevStockData
                    .FirstOrDefault(p => p.Sticker == x.Sticker)?.Sclose is double prevSclose && prevSclose != 0
                    && ((x.Sclose.Value - prevSclose) / prevSclose * 100) > 0);
            model.IndicatorsDownCount = stockData
                .Count(x => x.Sclose.HasValue && prevStockData
                    .FirstOrDefault(p => p.Sticker == x.Sticker)?.Sclose is double prevSclose && prevSclose != 0
                    && ((x.Sclose.Value - prevSclose) / prevSclose * 100) < 0);


            model.IndicatorsNoChangeCount = stockData
                .Count(x => x.Sclose.HasValue && prevStockData
                    .FirstOrDefault(p => p.Sticker == x.Sticker)?.Sclose is double prevSclose
                    && ((x.Sclose.Value - prevSclose) / prevSclose * 100) == 0);


            model.CompaniesUpCount = model.IndicatorsUpCount; // Adjust if companies differ from indicators
            model.CompaniesDownCount = model.IndicatorsDownCount;
            model.CompaniesNoChangeCount = model.IndicatorsNoChangeCount;

            // Criteria
            var criteria = await _context.Criterias
                .Where(x => x.Id == criteriaId)
                .FirstOrDefaultAsync();
            if (criteria != null)
            {
                model.CriteriaIndex = criteria.OrderNo ?? 0;
                model.CriteriaSeparater = criteria.Separater ?? "";
                model.CriteriaTitle = criteria.Name ?? "";
                model.CriteriaType = criteria.Type ?? "";
                model.CriteriaNotes = criteria.Note ?? ""; // Changed from Notes to Note
            }

            // Follow Lists
            model.FollowLists = await _context.FollowLists
                .Where(x => x.UserId == userId)
                .Select(x => new FollowListViewModel
                {
                    Id = x.Id,
                    Name = x.Name ?? "0",
                    Color = x.Color ?? "#ff000000"
                })
                .ToListAsync();

            // Recommendations
            var recommendations = await _context.StockTables
                .Where(x => x.Sdate == model.MaxDate.Value.ToString("yyyy/MM/dd"))
                .ToListAsync();
            var recommendationViewModels = new List<RecommendationViewModel>();

            // Pre-fetch settings for SupportResistance
            var settings = await _context.Settings.ToListAsync();

            foreach (var row in recommendations)
            {
                var prevRow = await _context.StockTables
                    .Where(x => x.Sticker == row.Sticker && x.DayNo > row.DayNo)
                    .OrderBy(x => x.DayNo)
                    .FirstOrDefaultAsync();
                double changeValue = row.Sclose.HasValue && prevRow!=null&& prevRow.Sclose.HasValue
                    ? row.Sclose.Value - prevRow.Sclose.Value
                    : 0;
                double changeRate = row.Sclose.HasValue && prevRow != null && prevRow.Sclose.HasValue && prevRow.Sclose.Value != 0
                    ? (changeValue / prevRow.Sclose.Value) * 100
                    : 0;

                var followLists = await _context.FollowListCompanies
                    .Where(fc => fc.CompanyCode == row.Sticker)
                    .Join(_context.FollowLists,
                        fc => fc.FollowListId,
                        fl => fl.Id,
                        (fc, fl) => fl)
                    .ToListAsync();
                var tooltip = string.Join(", ", followLists.Select(f => f.Name));
                var backgroundColor = followLists.Any() ? $"#{followLists.First().Color?.Substring(3)}75" : "";

                var viewModel = new RecommendationViewModel
                {
                    Sticker = row.Sticker,
                    Sname = row.Sname ?? "0",
                    Sclose = row.Sclose ?? 0,
                    PrevSclose = prevRow?.Sclose ?? 0,
                    NextSclose = 0, // Placeholder: Replace with actual logic
                    PrevSopen = prevRow?.Sopen ?? 0,
                    PrevShigh = prevRow?.Shigh ?? 0,
                    PrevSlow = prevRow?.Slow ?? 0,
                    ExpectedOpen = row.ExpectedOpen ?? 0,
                    ExpectedOpenValue = 0, // Placeholder: Replace with actual logic
                    ExpectedOpenPercent = 0, // Placeholder
                    OpeningGapValue = 0, // Placeholder
                    OpeningGapRate = 0, // Placeholder
                    UpperLimitValue = 0, // Placeholder
                    UpperLimitRate = 0, // Placeholder
                    LowerLimitValue = 0, // Placeholder
                    LowerLimitRate = 0, // Placeholder
                    ChangeValue = changeValue,
                    ChangeRate = changeRate,
                    IsIndicator = false, // Placeholder: Replace with actual logic
                    IsSpecial = false, // Placeholder
                    Tooltip = tooltip,
                    BackgroundColor = backgroundColor,
                    // Formatted fields for ProfitLoss
                    FormattedSclose = ConvertHelper.GetControlWithCompareValues(row.Sclose ?? 0, prevRow?.Sclose ?? 0),
                    FormattedExpectedOpen = ConvertHelper.GetControlWithCompareValues(row.ExpectedOpen ?? 0, row.Sclose ?? 0),
                    FormattedExpectedOpenValue = ConvertHelper.FormatDouble(0), // Placeholder
                    FormattedExpectedOpenPercent = ConvertHelper.FormatDouble(0), // Placeholder
                    FormattedPrevSopen = ConvertHelper.GetControlWithCompareValues(prevRow?.Sopen ?? 0, row.Sclose ?? 0),
                    FormattedOpeningGapValue = ConvertHelper.FormatDouble(0), // Placeholder
                    FormattedOpeningGapRate = ConvertHelper.FormatDouble(0), // Placeholder
                    FormattedPrevShigh = ConvertHelper.GetControlWithCompareValues(prevRow?.Shigh ?? 0, row.Sclose ?? 0),
                    FormattedUpperLimitValue = ConvertHelper.FormatDouble(0), // Placeholder
                    FormattedUpperLimitRate = ConvertHelper.FormatDouble(0), // Placeholder
                    FormattedPrevSlow = ConvertHelper.GetControlWithCompareValues(prevRow?.Slow ?? 0, row.Sclose ?? 0),
                    FormattedLowerLimitValue = ConvertHelper.FormatDouble(0), // Placeholder
                    FormattedLowerLimitRate = ConvertHelper.FormatDouble(0), // Placeholder
                    FormattedPrevSclose = ConvertHelper.GetControlWithCompareValues(prevRow?.Sclose ?? 0, row.Sclose ?? 0),
                    FormattedChangeValue = ConvertHelper.GetControlWithCompareValuesAndControlValue(prevRow?.Sclose ?? 0, row.Sclose ?? 0, changeValue, false),
                    FormattedChangeRate = ConvertHelper.GetControlWithCompareValuesAndControlValue(prevRow?.Sclose ?? 0, row.Sclose ?? 0, changeRate, true)
                };

                if (viewMode == "SupportResistance")
                {
                    var stopLossValue = row.Sclose.HasValue ? row.Sclose.Value - (row.Sclose.Value * (model.StopLossValue / 100)) : 0;
                    var secondSupportValue = row.Sclose.HasValue ? row.Sclose.Value - (row.Sclose.Value * (model.SecondSupportValue / 100)) : 0;
                    var firstSupportValue = row.Sclose.HasValue ? row.Sclose.Value - (row.Sclose.Value * (model.FirstSupportValue / 100)) : 0;
                    var firstTargetValue = row.Sclose.HasValue ? row.Sclose.Value + (row.Sclose.Value * (model.FirstTargetValue / 100)) : 0;
                    var secondTargetValue = row.Sclose.HasValue ? row.Sclose.Value + (row.Sclose.Value * (model.SecondTargetValue / 100)) : 0;
                    var thirdTargetValue = row.Sclose.HasValue ? row.Sclose.Value + (row.Sclose.Value * (model.ThirdTargetValue / 100)) : 0;

                    var minClose = await _context.StockTables
                        .Where(x => x.Sticker == row.Sticker && x.DayNo == model.GeneralIndicator.DayNo)
                        .Select(x => x.Sclose)
                        .FirstOrDefaultAsync() ?? 0;
                    var minLow = await _context.StockTables
                        .Where(x => x.Sticker == row.Sticker && x.DayNo == model.GeneralIndicator.DayNo)
                        .Select(x => x.Slow)
                        .FirstOrDefaultAsync() ?? 0;
                    var maxHigh = await _context.StockTables
                        .Where(x => x.Sticker == row.Sticker && x.DayNo < model.GeneralIndicator.DayNo)
                        .Select(x => x.Shigh)
                        .DefaultIfEmpty(0)
                        .MaxAsync();
                    var lastClose = await _context.StockTables
                        .Where(x => x.Sticker == row.Sticker && x.DayNo == 1)
                        .Select(x => x.Sclose)
                        .FirstOrDefaultAsync() ?? 0;
                    var maxHighPercentage = maxHigh != 0 && row.Sclose.HasValue ? ((maxHigh - row.Sclose.Value) / row.Sclose.Value) * 100 : 0;
                    viewModel.FormattedStopLoss = ConvertHelper.GetStopLossChangeValueColor(stopLossValue, minClose);
                    viewModel.StopLossColor = settings?.Where(x => x.Name == "StopLossColor").Select(x=> x.Value).FirstOrDefault() ?? "";
                    viewModel.FormattedSecondSupport = ConvertHelper.GetSupportChangeValueColor(secondSupportValue, minLow);
                    viewModel.SecondSupportColor = settings?.Where(x => x.Name == "SecondSupportColor").Select(x => x.Value).FirstOrDefault() ?? "";
                    viewModel.FormattedFirstSupport = ConvertHelper.GetSupportChangeValueColor(firstSupportValue, minLow);
                    viewModel.FirstSupportColor = settings?.Where(x => x.Name == "FirstSupportColor").Select(x => x.Value).FirstOrDefault() ?? "";
                    viewModel.FormattedFirstTarget = ConvertHelper.GetTargetChangeValueColor(firstTargetValue, maxHigh);
                    viewModel.FirstTargetColor = settings?.Where(x => x.Name == "FirstTargetColor").Select(x => x.Value).FirstOrDefault() ?? "";
                    viewModel.FormattedSecondTarget = ConvertHelper.GetTargetChangeValueColor(secondTargetValue, maxHigh);
                    viewModel.SecondTargetColor = settings?.Where(x => x.Name == "SecondTargetColor").Select(x => x.Value).FirstOrDefault() ?? "";
                    viewModel.FormattedThirdTarget = ConvertHelper.GetTargetChangeValueColor(thirdTargetValue, maxHigh);
                    viewModel.ThirdTargetColor = settings?.Where(x => x.Name == "ThirdTargetColor").Select(x => x.Value).FirstOrDefault() ?? "";
                    viewModel.FormattedLastClose = ConvertHelper.GetControlWithCompareValues(Math.Round(lastClose, 2), row.Sclose ?? 0);
                    viewModel.FormattedMaxHigh = ConvertHelper.GetControlWithCompareValues(maxHigh, row.Sclose ?? 0);
                    viewModel.FormattedMaxHighPercentage = ConvertHelper.GetControlWithCompareValuesAndControlValue(maxHigh, row.Sclose ?? 0, Math.Round((decimal)maxHighPercentage, 2), true);
                }

                recommendationViewModels.Add(viewModel);
            }

            // Apply sorting
            recommendationViewModels = sortColumn switch
            {
                "Sticker" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.Sticker).ToList() : recommendationViewModels.OrderByDescending(x => x.Sticker).ToList(),
                "Sname" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.Sname).ToList() : recommendationViewModels.OrderByDescending(x => x.Sname).ToList(),
                "ExpectedOpenValue" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.ExpectedOpenValue).ToList() : recommendationViewModels.OrderByDescending(x => x.ExpectedOpenValue).ToList(),
                "ExpectedOpenPercent" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.ExpectedOpenPercent).ToList() : recommendationViewModels.OrderByDescending(x => x.ExpectedOpenPercent).ToList(),
                "OpeningGapValue" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.OpeningGapValue).ToList() : recommendationViewModels.OrderByDescending(x => x.OpeningGapValue).ToList(),
                "OpeningGapRate" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.OpeningGapRate).ToList() : recommendationViewModels.OrderByDescending(x => x.OpeningGapRate).ToList(),
                "UpperLimitValue" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.UpperLimitValue).ToList() : recommendationViewModels.OrderByDescending(x => x.UpperLimitValue).ToList(),
                "UpperLimitRate" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.UpperLimitRate).ToList() : recommendationViewModels.OrderByDescending(x => x.UpperLimitRate).ToList(),
                "LowerLimitValue" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.LowerLimitValue).ToList() : recommendationViewModels.OrderByDescending(x => x.LowerLimitValue).ToList(),
                "LowerLimitRate" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.LowerLimitRate).ToList() : recommendationViewModels.OrderByDescending(x => x.LowerLimitRate).ToList(),
                "ChangeValue" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.ChangeValue).ToList() : recommendationViewModels.OrderByDescending(x => x.ChangeValue).ToList(),
                "ChangeRate" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.ChangeRate).ToList() : recommendationViewModels.OrderByDescending(x => x.ChangeRate).ToList(),
                "Sclose" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.Sclose).ToList() : recommendationViewModels.OrderByDescending(x => x.Sclose).ToList(),
                "SclosePercent" => sortOrder == "ASC" ? recommendationViewModels.OrderBy(x => x.ChangeRate).ToList() : recommendationViewModels.OrderByDescending(x => x.ChangeRate).ToList(),
                _ => recommendationViewModels
            };

            model.Recommendations = recommendationViewModels;
            return model;
        }
        private async Task<List<StockPrevDayView>> GetCriteriaRecommendationResultAsync(int criteriaId, DateTime? startDate)
        {
            int startDayNo = await GetDayNumberByDateAsync(startDate);
            var criteria = await _context.Criterias
                .Include(x => x.Formulas)
                .FirstOrDefaultAsync(x => x.Id == criteriaId);
            if (criteria == null) return new List<StockPrevDayView>();

            var stockResult = await _context.StockPrevDayViews
                .AsNoTracking()
                .Where(x => x.DayNo == startDayNo && x.ParentIndicator != null &&
                            (criteria.IsIndicator == 0 ? !x.IsIndicator :
                             criteria.IsIndicator == 1 ? x.IsIndicator : true))
                .ToListAsync();

            foreach (var group in criteria.Formulas.OrderByDescending(x => x.Day).GroupBy(x => x.Day))
            {
                int formulaDayNo = startDayNo + group.First().Day - 1;
                var res = await _context.StockPrevDayViews
                    .AsNoTracking()
                    .Where(x => x.DayNo == formulaDayNo && x.ParentIndicator != null &&
                                (criteria.IsIndicator == 0 ? !x.IsIndicator :
                                 criteria.IsIndicator == 1 ? x.IsIndicator : true))
                    .ToListAsync();
                stockResult = res.Where(x => stockResult.Select(y => y.Sticker).Contains(x.Sticker)).ToList();

                foreach (var formula in group)
                {
                    var values = formula.FormulaValues.Split(';');
                    stockResult = ApplyFormula(stockResult, formula.FormulaType, values, formulaDayNo);
                }
            }

            var groupedResults = stockResult.GroupBy(x => x.ParentIndicator).ToList();
            var sortedList = new List<StockPrevDayView>();
            foreach (var group in groupedResults)
            {
                var parentIndicator = await _context.StockPrevDayViews
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Sticker == group.First().ParentIndicator && x.DayNo == startDayNo && x.ParentIndicator != null);
                if (parentIndicator != null)
                    sortedList.Add(parentIndicator);

                if (group.Key == "TASI" && groupedResults.Count > 1)
                {
                    var temp = group.ToList();
                    temp.RemoveAll(x => stockResult.Contains(x));
                    sortedList.AddRange(temp);
                }
                else
                {
                    sortedList.AddRange(group);
                }
            }

            return sortedList;
        }

        private List<StockPrevDayView> ApplyFormula(List<StockPrevDayView> stockResult, int formulaType, string[] values, int formulaDayNo)
        {
            // Common formula structure
            var formula = new
            {
                TypeAll = bool.Parse(values[0]),
                TypePositive = bool.Parse(values[1]),
                TypeNegative = bool.Parse(values[2]),
                TypeNoChange = bool.Parse(values[3]),
                GreaterThan = ConvertHelper.ToDoubleZ(values[4]),
                LessThan = ConvertHelper.ToDoubleZ(values[5])
            };

            // Formula 1: Percentage change in closing price
            if (formulaType == 1)
            {
                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => x.PrevSclose != 0 && ((x.Sclose - x.PrevSclose) / x.PrevSclose * 100) > 0).ToList();
                        var negative = stockResult.Where(x => x.PrevSclose != 0 && ((x.Sclose - x.PrevSclose) / x.PrevSclose * 100) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => ((x.Sclose - x.PrevSclose) / x.PrevSclose * 100) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -((x.Sclose - x.PrevSclose) / x.PrevSclose * 100) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => ((x.Sclose - x.PrevSclose) / x.PrevSclose * 100) < formula.LessThan).ToList();
                            negative = negative.Where(x => -((x.Sclose - x.PrevSclose) / x.PrevSclose * 100) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => x.PrevSclose != 0 && ((x.Sclose - x.PrevSclose) / x.PrevSclose * 100) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => ((x.Sclose - x.PrevSclose) / x.PrevSclose * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => ((x.Sclose - x.PrevSclose) / x.PrevSclose * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => x.PrevSclose != 0 && ((x.Sclose - x.PrevSclose) / x.PrevSclose * 100) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -((x.Sclose - x.PrevSclose) / x.PrevSclose * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -((x.Sclose - x.PrevSclose) / x.PrevSclose * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => x.PrevSclose != 0 && ((x.Sclose - x.PrevSclose) / x.PrevSclose * 100) == 0).ToList();
                    }
                }
            }
            // Formula 2: Volume change percentage
            else if (formulaType == 2)
            {
                var prevDayVolumes = stockResult.ToDictionary(
                    x => x.Sticker,
                    x => _context.StockPrevDayViews
                        .AsNoTracking()
                        .Where(p => p.Sticker == x.Sticker && p.DayNo == formulaDayNo - 1)
                        .Select(p => p.Svol)
                        .FirstOrDefault()
                );

                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => prevDayVolumes[x.Sticker] != 0 && ((x.Svol - prevDayVolumes[x.Sticker]) / prevDayVolumes[x.Sticker] * 100) > 0).ToList();
                        var negative = stockResult.Where(x => prevDayVolumes[x.Sticker] != 0 && ((x.Svol - prevDayVolumes[x.Sticker]) / prevDayVolumes[x.Sticker] * 100) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => ((x.Svol - prevDayVolumes[x.Sticker]) / prevDayVolumes[x.Sticker] * 100) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -((x.Svol - prevDayVolumes[x.Sticker]) / prevDayVolumes[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => ((x.Svol - prevDayVolumes[x.Sticker]) / prevDayVolumes[x.Sticker] * 100) < formula.LessThan).ToList();
                            negative = negative.Where(x => -((x.Svol - prevDayVolumes[x.Sticker]) / prevDayVolumes[x.Sticker] * 100) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => prevDayVolumes[x.Sticker] != 0 && ((x.Svol - prevDayVolumes[x.Sticker]) / prevDayVolumes[x.Sticker] * 100) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => ((x.Svol - prevDayVolumes[x.Sticker]) / prevDayVolumes[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => ((x.Svol - prevDayVolumes[x.Sticker]) / prevDayVolumes[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => prevDayVolumes[x.Sticker] != 0 && ((x.Svol - prevDayVolumes[x.Sticker]) / prevDayVolumes[x.Sticker] * 100) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -((x.Svol - prevDayVolumes[x.Sticker]) / prevDayVolumes[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -((x.Svol - prevDayVolumes[x.Sticker]) / prevDayVolumes[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => prevDayVolumes[x.Sticker] != 0 && ((x.Svol - prevDayVolumes[x.Sticker]) / prevDayVolumes[x.Sticker] * 100) == 0).ToList();
                    }
                }
            }
            // Formula 3: Price range percentage (High - Low) / Close
            else if (formulaType == 3)
            {
                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => x.Sclose != 0 && ((x.Shigh - x.Slow) / x.Sclose * 100) > 0).ToList();
                        var negative = stockResult.Where(x => x.Sclose != 0 && ((x.Shigh - x.Slow) / x.Sclose * 100) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => ((x.Shigh - x.Slow) / x.Sclose * 100) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -((x.Shigh - x.Slow) / x.Sclose * 100) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => ((x.Shigh - x.Slow) / x.Sclose * 100) < formula.LessThan).ToList();
                            negative = negative.Where(x => -((x.Shigh - x.Slow) / x.Sclose * 100) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => x.Sclose != 0 && ((x.Shigh - x.Slow) / x.Sclose * 100) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => ((x.Shigh - x.Slow) / x.Sclose * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => ((x.Shigh - x.Slow) / x.Sclose * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => x.Sclose != 0 && ((x.Shigh - x.Slow) / x.Sclose * 100) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -((x.Shigh - x.Slow) / x.Sclose * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -((x.Shigh - x.Slow) / x.Sclose * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => x.Sclose != 0 && ((x.Shigh - x.Slow) / x.Sclose * 100) == 0).ToList();
                    }
                }
            }
            // Formula 4: Opening gap percentage (Open - PrevClose) / PrevClose
            else if (formulaType == 4)
            {
                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => x.PrevSclose != 0 && ((x.Sopen - x.PrevSclose) / x.PrevSclose * 100) > 0).ToList();
                        var negative = stockResult.Where(x => x.PrevSclose != 0 && ((x.Sopen - x.PrevSclose) / x.PrevSclose * 100) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => ((x.Sopen - x.PrevSclose) / x.PrevSclose * 100) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -((x.Sopen - x.PrevSclose) / x.PrevSclose * 100) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => ((x.Sopen - x.PrevSclose) / x.PrevSclose * 100) < formula.LessThan).ToList();
                            negative = negative.Where(x => -((x.Sopen - x.PrevSclose) / x.PrevSclose * 100) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => x.PrevSclose != 0 && ((x.Sopen - x.PrevSclose) / x.PrevSclose * 100) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => ((x.Sopen - x.PrevSclose) / x.PrevSclose * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => ((x.Sopen - x.PrevSclose) / x.PrevSclose * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => x.PrevSclose != 0 && ((x.Sopen - x.PrevSclose) / x.PrevSclose * 100) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -((x.Sopen - x.PrevSclose) / x.PrevSclose * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -((x.Sopen - x.PrevSclose) / x.PrevSclose * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => x.PrevSclose != 0 && ((x.Sopen - x.PrevSclose) / x.PrevSclose * 100) == 0).ToList();
                    }
                }
            }
            // Formula 5: High price change percentage
            else if (formulaType == 5)
            {
                var prevDayHighs = stockResult.ToDictionary(
                    x => x.Sticker,
                    x => _context.StockPrevDayViews
                        .AsNoTracking()
                        .Where(p => p.Sticker == x.Sticker && p.DayNo == formulaDayNo - 1)
                        .Select(p => p.Shigh)
                        .FirstOrDefault()
                );

                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => prevDayHighs[x.Sticker] != 0 && ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) > 0).ToList();
                        var negative = stockResult.Where(x => prevDayHighs[x.Sticker] != 0 && ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) < formula.LessThan).ToList();
                            negative = negative.Where(x => -((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => prevDayHighs[x.Sticker] != 0 && ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => prevDayHighs[x.Sticker] != 0 && ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => prevDayHighs[x.Sticker] != 0 && ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) == 0).ToList();
                    }
                }
            }
            // Formula 6: Low price change percentage
            else if (formulaType == 6)
            {
                var prevDayLows = stockResult.ToDictionary(
                    x => x.Sticker,
                    x => _context.StockPrevDayViews
                        .AsNoTracking()
                        .Where(p => p.Sticker == x.Sticker && p.DayNo == formulaDayNo - 1)
                        .Select(p => p.Slow)
                        .FirstOrDefault()
                );

                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => prevDayLows[x.Sticker] != 0 && ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) > 0).ToList();
                        var negative = stockResult.Where(x => prevDayLows[x.Sticker] != 0 && ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) < formula.LessThan).ToList();
                            negative = negative.Where(x => -((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => prevDayLows[x.Sticker] != 0 && ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => prevDayLows[x.Sticker] != 0 && ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => prevDayLows[x.Sticker] != 0 && ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) == 0).ToList();
                    }
                }
            }
            // Formula 7: Close to High percentage
            else if (formulaType == 7)
            {
                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => x.Shigh != 0 && ((x.Sclose - x.Shigh) / x.Shigh * 100) > 0).ToList();
                        var negative = stockResult.Where(x => x.Shigh != 0 && ((x.Sclose - x.Shigh) / x.Shigh * 100) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => ((x.Sclose - x.Shigh) / x.Shigh * 100) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -((x.Sclose - x.Shigh) / x.Shigh * 100) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => ((x.Sclose - x.Shigh) / x.Shigh * 100) < formula.LessThan).ToList();
                            negative = negative.Where(x => -((x.Sclose - x.Shigh) / x.Shigh * 100) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => x.Shigh != 0 && ((x.Sclose - x.Shigh) / x.Shigh * 100) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => ((x.Sclose - x.Shigh) / x.Shigh * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => ((x.Sclose - x.Shigh) / x.Shigh * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => x.Shigh != 0 && ((x.Sclose - x.Shigh) / x.Shigh * 100) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -((x.Sclose - x.Shigh) / x.Shigh * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -((x.Sclose - x.Shigh) / x.Shigh * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => x.Shigh != 0 && ((x.Sclose - x.Shigh) / x.Shigh * 100) == 0).ToList();
                    }
                }
            }
            // Formula 8: Close to Low percentage
            else if (formulaType == 8)
            {
                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => x.Slow != 0 && ((x.Sclose - x.Slow) / x.Slow * 100) > 0).ToList();
                        var negative = stockResult.Where(x => x.Slow != 0 && ((x.Sclose - x.Slow) / x.Slow * 100) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => ((x.Sclose - x.Slow) / x.Slow * 100) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -((x.Sclose - x.Slow) / x.Slow * 100) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => ((x.Sclose - x.Slow) / x.Slow * 100) < formula.LessThan).ToList();
                            negative = negative.Where(x => -((x.Sclose - x.Slow) / x.Slow * 100) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => x.Slow != 0 && ((x.Sclose - x.Slow) / x.Slow * 100) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => ((x.Sclose - x.Slow) / x.Slow * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => ((x.Sclose - x.Slow) / x.Slow * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => x.Slow != 0 && ((x.Sclose - x.Slow) / x.Slow * 100) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -((x.Sclose - x.Slow) / x.Slow * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -((x.Sclose - x.Slow) / x.Slow * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => x.Slow != 0 && ((x.Sclose - x.Slow) / x.Slow * 100) == 0).ToList();
                    }
                }
            }
            // Formula 9: Absolute closing price change
            else if (formulaType == 9)
            {
                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => (x.Sclose - x.PrevSclose) > 0).ToList();
                        var negative = stockResult.Where(x => (x.Sclose - x.PrevSclose) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => (x.Sclose - x.PrevSclose) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -(x.Sclose - x.PrevSclose) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => (x.Sclose - x.PrevSclose) < formula.LessThan).ToList();
                            negative = negative.Where(x => -(x.Sclose - x.PrevSclose) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => (x.Sclose - x.PrevSclose) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => (x.Sclose - x.PrevSclose) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => (x.Sclose - x.PrevSclose) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => (x.Sclose - x.PrevSclose) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -(x.Sclose - x.PrevSclose) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -(x.Sclose - x.PrevSclose) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => (x.Sclose - x.PrevSclose) == 0).ToList();
                    }
                }
            }
            // Formula 10: Absolute volume change
            else if (formulaType == 10)
            {
                var prevDayVolumes = stockResult.ToDictionary(
                    x => x.Sticker,
                    x => _context.StockPrevDayViews
                        .AsNoTracking()
                        .Where(p => p.Sticker == x.Sticker && p.DayNo == formulaDayNo - 1)
                        .Select(p => p.Svol)
                        .FirstOrDefault()
                );

                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => (x.Svol - prevDayVolumes[x.Sticker]) > 0).ToList();
                        var negative = stockResult.Where(x => (x.Svol - prevDayVolumes[x.Sticker]) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => (x.Svol - prevDayVolumes[x.Sticker]) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -(x.Svol - prevDayVolumes[x.Sticker]) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => (x.Svol - prevDayVolumes[x.Sticker]) < formula.LessThan).ToList();
                            negative = negative.Where(x => -(x.Svol - prevDayVolumes[x.Sticker]) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => (x.Svol - prevDayVolumes[x.Sticker]) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => (x.Svol - prevDayVolumes[x.Sticker]) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => (x.Svol - prevDayVolumes[x.Sticker]) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => (x.Svol - prevDayVolumes[x.Sticker]) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -(x.Svol - prevDayVolumes[x.Sticker]) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -(x.Svol - prevDayVolumes[x.Sticker]) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => (x.Svol - prevDayVolumes[x.Sticker]) == 0).ToList();
                    }
                }
            }
            // Formula 11: Open to Close percentage
            else if (formulaType == 11)
            {
                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => x.Sopen != 0 && ((x.Sclose - x.Sopen) / x.Sopen * 100) > 0).ToList();
                        var negative = stockResult.Where(x => x.Sopen != 0 && ((x.Sclose - x.Sopen) / x.Sopen * 100) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => ((x.Sclose - x.Sopen) / x.Sopen * 100) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -((x.Sclose - x.Sopen) / x.Sopen * 100) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => ((x.Sclose - x.Sopen) / x.Sopen * 100) < formula.LessThan).ToList();
                            negative = negative.Where(x => -((x.Sclose - x.Sopen) / x.Sopen * 100) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => x.Sopen != 0 && ((x.Sclose - x.Sopen) / x.Sopen * 100) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => ((x.Sclose - x.Sopen) / x.Sopen * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => ((x.Sclose - x.Sopen) / x.Sopen * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => x.Sopen != 0 && ((x.Sclose - x.Sopen) / x.Sopen * 100) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -((x.Sclose - x.Sopen) / x.Sopen * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -((x.Sclose - x.Sopen) / x.Sopen * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => x.Sopen != 0 && ((x.Sclose - x.Sopen) / x.Sopen * 100) == 0).ToList();
                    }
                }
            }
            // Formula 12: Absolute open to close change
            else if (formulaType == 12)
            {
                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => (x.Sclose - x.Sopen) > 0).ToList();
                        var negative = stockResult.Where(x => (x.Sclose - x.Sopen) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => (x.Sclose - x.Sopen) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -(x.Sclose - x.Sopen) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => (x.Sclose - x.Sopen) < formula.LessThan).ToList();
                            negative = negative.Where(x => -(x.Sclose - x.Sopen) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => (x.Sclose - x.Sopen) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => (x.Sclose - x.Sopen) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => (x.Sclose - x.Sopen) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => (x.Sclose - x.Sopen) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -(x.Sclose - x.Sopen) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -(x.Sclose - x.Sopen) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => (x.Sclose - x.Sopen) == 0).ToList();
                    }
                }
            }
            // Formula 13: High to previous high percentage
            else if (formulaType == 13)
            {
                var prevDayHighs = stockResult.ToDictionary(
                    x => x.Sticker,
                    x => _context.StockPrevDayViews
                        .AsNoTracking()
                        .Where(p => p.Sticker == x.Sticker && p.DayNo == formulaDayNo - 1)
                        .Select(p => p.Shigh)
                        .FirstOrDefault()
                );

                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => prevDayHighs[x.Sticker] != 0 && ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) > 0).ToList();
                        var negative = stockResult.Where(x => prevDayHighs[x.Sticker] != 0 && ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) < formula.LessThan).ToList();
                            negative = negative.Where(x => -((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => prevDayHighs[x.Sticker] != 0 && ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => prevDayHighs[x.Sticker] != 0 && ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => prevDayHighs[x.Sticker] != 0 && ((x.Shigh - prevDayHighs[x.Sticker]) / prevDayHighs[x.Sticker] * 100) == 0).ToList();
                    }
                }
            }
            // Formula 14: Low to previous low percentage
            else if (formulaType == 14)
            {
                var prevDayLows = stockResult.ToDictionary(
                    x => x.Sticker,
                    x => _context.StockPrevDayViews
                        .AsNoTracking()
                        .Where(p => p.Sticker == x.Sticker && p.DayNo == formulaDayNo - 1)
                        .Select(p => p.Slow)
                        .FirstOrDefault()
                );

                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => prevDayLows[x.Sticker] != 0 && ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) > 0).ToList();
                        var negative = stockResult.Where(x => prevDayLows[x.Sticker] != 0 && ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) < formula.LessThan).ToList();
                            negative = negative.Where(x => -((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => prevDayLows[x.Sticker] != 0 && ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => prevDayLows[x.Sticker] != 0 && ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => prevDayLows[x.Sticker] != 0 && ((x.Slow - prevDayLows[x.Sticker]) / prevDayLows[x.Sticker] * 100) == 0).ToList();
                    }
                }
            }
            // Formula 15: Absolute high to low change
            else if (formulaType == 15)
            {
                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => (x.Shigh - x.Slow) > 0).ToList();
                        var negative = stockResult.Where(x => (x.Shigh - x.Slow) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => (x.Shigh - x.Slow) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -(x.Shigh - x.Slow) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => (x.Shigh - x.Slow) < formula.LessThan).ToList();
                            negative = negative.Where(x => -(x.Shigh - x.Slow) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => (x.Shigh - x.Slow) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => (x.Shigh - x.Slow) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => (x.Shigh - x.Slow) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => (x.Shigh - x.Slow) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -(x.Shigh - x.Slow) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -(x.Shigh - x.Slow) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => (x.Shigh - x.Slow) == 0).ToList();
                    }
                }
            }
            // Formula 16: Close to previous open percentage
            else if (formulaType == 16)
            {
                var prevDayOpens = stockResult.ToDictionary(
                    x => x.Sticker,
                    x => _context.StockPrevDayViews
                        .AsNoTracking()
                        .Where(p => p.Sticker == x.Sticker && p.DayNo == formulaDayNo - 1)
                        .Select(p => p.Sopen)
                        .FirstOrDefault()
                );

                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => prevDayOpens[x.Sticker] != 0 && ((x.Sclose - prevDayOpens[x.Sticker]) / prevDayOpens[x.Sticker] * 100) > 0).ToList();
                        var negative = stockResult.Where(x => prevDayOpens[x.Sticker] != 0 && ((x.Sclose - prevDayOpens[x.Sticker]) / prevDayOpens[x.Sticker] * 100) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => ((x.Sclose - prevDayOpens[x.Sticker]) / prevDayOpens[x.Sticker] * 100) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -((x.Sclose - prevDayOpens[x.Sticker]) / prevDayOpens[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => ((x.Sclose - prevDayOpens[x.Sticker]) / prevDayOpens[x.Sticker] * 100) < formula.LessThan).ToList();
                            negative = negative.Where(x => -((x.Sclose - prevDayOpens[x.Sticker]) / prevDayOpens[x.Sticker] * 100) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => prevDayOpens[x.Sticker] != 0 && ((x.Sclose - prevDayOpens[x.Sticker]) / prevDayOpens[x.Sticker] * 100) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => ((x.Sclose - prevDayOpens[x.Sticker]) / prevDayOpens[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => ((x.Sclose - prevDayOpens[x.Sticker]) / prevDayOpens[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => prevDayOpens[x.Sticker] != 0 && ((x.Sclose - prevDayOpens[x.Sticker]) / prevDayOpens[x.Sticker] * 100) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -((x.Sclose - prevDayOpens[x.Sticker]) / prevDayOpens[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -((x.Sclose - prevDayOpens[x.Sticker]) / prevDayOpens[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => prevDayOpens[x.Sticker] != 0 && ((x.Sclose - prevDayOpens[x.Sticker]) / prevDayOpens[x.Sticker] * 100) == 0).ToList();
                    }
                }
            }
            // Formula 17: Trading range change percentage ((High - Low) - Prev(High - Low)) / Prev(High - Low)
            else if (formulaType == 17)
            {
                var prevDayRanges = stockResult.ToDictionary(
                    x => x.Sticker,
                    x =>
                    {
                        var prevDay = _context.StockPrevDayViews
                            .AsNoTracking()
                            .Where(p => p.Sticker == x.Sticker && p.DayNo == formulaDayNo - 1)
                            .Select(p => new { p.Shigh, p.Slow })
                            .FirstOrDefault();
                        return prevDay != null ? prevDay.Shigh - prevDay.Slow : 0;
                    }
                );

                if (formula.TypeAll)
                {
                    if (formula.GreaterThan != 0 || formula.LessThan != 0)
                    {
                        var positive = stockResult.Where(x => prevDayRanges[x.Sticker] != 0 && (((x.Shigh - x.Slow) - prevDayRanges[x.Sticker]) / prevDayRanges[x.Sticker] * 100) > 0).ToList();
                        var negative = stockResult.Where(x => prevDayRanges[x.Sticker] != 0 && (((x.Shigh - x.Slow) - prevDayRanges[x.Sticker]) / prevDayRanges[x.Sticker] * 100) < 0).ToList();

                        if (formula.GreaterThan != 0)
                        {
                            positive = positive.Where(x => (((x.Shigh - x.Slow) - prevDayRanges[x.Sticker]) / prevDayRanges[x.Sticker] * 100) > formula.GreaterThan).ToList();
                            negative = negative.Where(x => -(((x.Shigh - x.Slow) - prevDayRanges[x.Sticker]) / prevDayRanges[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        }
                        if (formula.LessThan != 0)
                        {
                            positive = positive.Where(x => (((x.Shigh - x.Slow) - prevDayRanges[x.Sticker]) / prevDayRanges[x.Sticker] * 100) < formula.LessThan).ToList();
                            negative = negative.Where(x => -(((x.Shigh - x.Slow) - prevDayRanges[x.Sticker]) / prevDayRanges[x.Sticker] * 100) < formula.LessThan).ToList();
                        }
                        positive.AddRange(negative);
                        return positive;
                    }
                }
                else
                {
                    if (formula.TypePositive)
                    {
                        stockResult = stockResult.Where(x => prevDayRanges[x.Sticker] != 0 && (((x.Shigh - x.Slow) - prevDayRanges[x.Sticker]) / prevDayRanges[x.Sticker] * 100) > 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => (((x.Shigh - x.Slow) - prevDayRanges[x.Sticker]) / prevDayRanges[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => (((x.Shigh - x.Slow) - prevDayRanges[x.Sticker]) / prevDayRanges[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNegative)
                    {
                        stockResult = stockResult.Where(x => prevDayRanges[x.Sticker] != 0 && (((x.Shigh - x.Slow) - prevDayRanges[x.Sticker]) / prevDayRanges[x.Sticker] * 100) < 0).ToList();
                        if (formula.GreaterThan != 0)
                            stockResult = stockResult.Where(x => -(((x.Shigh - x.Slow) - prevDayRanges[x.Sticker]) / prevDayRanges[x.Sticker] * 100) > formula.GreaterThan).ToList();
                        if (formula.LessThan != 0)
                            stockResult = stockResult.Where(x => -(((x.Shigh - x.Slow) - prevDayRanges[x.Sticker]) / prevDayRanges[x.Sticker] * 100) < formula.LessThan).ToList();
                    }
                    else if (formula.TypeNoChange)
                    {
                        stockResult = stockResult.Where(x => prevDayRanges[x.Sticker] != 0 && (((x.Shigh - x.Slow) - prevDayRanges[x.Sticker]) / prevDayRanges[x.Sticker] * 100) == 0).ToList();
                    }
                }
            }

            return stockResult;
        }



        private List<RecommendationsResultsView> SortRecommendations(List<RecommendationsResultsView> recommendations, string sortColumn, string sortOrder)
        {
            var query = recommendations.AsQueryable();
            bool isDescending = sortOrder == "DESC";

            switch (sortColumn)
            {
                case "Sticker":
                    query = isDescending ? query.OrderByDescending(x => x.Sticker) : query.OrderBy(x => x.Sticker);
                    break;
                case "Sname":
                    query = isDescending ? query.OrderByDescending(x => x.Sname) : query.OrderBy(x => x.Sname);
                    break;
                case "ExpectedOpenValue":
                    query = isDescending ? query.OrderByDescending(x => x.ExpectedOpenValue) : query.OrderBy(x => x.ExpectedOpenValue);
                    break;
                case "ExpectedOpenPercent":
                    query = isDescending ? query.OrderByDescending(x => x.ExpectedOpenPercent) : query.OrderBy(x => x.ExpectedOpenPercent);
                    break;
                case "openingGapValue":
                    query = isDescending ? query.OrderByDescending(x => x.OpeningGapValue) : query.OrderBy(x => x.OpeningGapValue);
                    break;
                case "openingGapRate":
                    query = isDescending ? query.OrderByDescending(x => x.OpeningGapRate) : query.OrderBy(x => x.OpeningGapRate);
                    break;
                case "UpperLimitValue":
                    query = isDescending ? query.OrderByDescending(x => x.UpperLimitValue) : query.OrderBy(x => x.UpperLimitValue);
                    break;
                case "UpperLimitRate":
                    query = isDescending ? query.OrderByDescending(x => x.UpperLimitRate) : query.OrderBy(x => x.UpperLimitRate);
                    break;
                case "LowerLimitValue":
                    query = isDescending ? query.OrderByDescending(x => x.LowerLimitValue) : query.OrderBy(x => x.LowerLimitValue);
                    break;
                case "LowerLimitRate":
                    query = isDescending ? query.OrderByDescending(x => x.LowerLimitRate) : query.OrderBy(x => x.LowerLimitRate);
                    break;
                case "ChangeValue":
                    query = isDescending ? query.OrderByDescending(x => x.ChangeValue) : query.OrderBy(x => x.ChangeValue);
                    break;
                case "ChangeRate":
                    query = isDescending ? query.OrderByDescending(x => x.ChangeRate) : query.OrderBy(x => x.ChangeRate);
                    break;
                case "Sclose":
                    query = isDescending ? query.OrderByDescending(x => x.Sclose) : query.OrderBy(x => x.Sclose);
                    break;
                case "SclosePercent":
                    query = isDescending ? query.OrderByDescending(x => ((x.PrevShigh - x.Sclose) / x.Sclose) * 100) : query.OrderBy(x => ((x.PrevShigh - x.Sclose) / x.Sclose) * 100);
                    break;
            }

            return query.ToList();
        }

        private async Task<int> GetDayNumberByDateAsync(DateTime? date)
        {
            if (date == null) return 1;
            var result = await _context.StockTables
                .FirstOrDefaultAsync(x => x.Createddate == date);
            return result?.DayNo ?? 1;
        }

        private async Task<string> GetCompanyNameAsync(string code)
        {
            var company = await _context.CompanyTables
                .FirstOrDefaultAsync(x => x.CompanyCode == code);
            return company?.CompanyName ?? string.Empty;
        }

        private async Task<double> GetSettingDoubleAsync(string name)
        {
            var setting = await _context.Settings
                .FirstOrDefaultAsync(x => x.Name == name);
            return setting != null ? ConvertHelper.ToDoubleZ(setting.Value) : 0;
        }

        private async Task<DateTime?> GetNextDateAsync(DateTime? selectedDate)
        {
            if (selectedDate == null) return null;
            var current = await _context.StockTables
                .FirstOrDefaultAsync(x => x.Createddate == selectedDate);
            if (current == null) return null;
            return await _context.StockTables
                .Where(x => x.Sticker == current.Sticker && x.DayNo == current.DayNo - 1)
                .Select(x => x.Createddate)
                .FirstOrDefaultAsync();
        }

        private async Task<DateTime?> GetPreviousDateAsync(DateTime? selectedDate)
        {
            if (selectedDate == null) return null;
            var current = await _context.StockTables
                .FirstOrDefaultAsync(x => x.Createddate == selectedDate);
            if (current == null) return null;
            return await _context.StockTables
                .Where(x => x.Sticker == current.Sticker && x.DayNo == current.DayNo + 1)
                .Select(x => x.Createddate)
                .FirstOrDefaultAsync();
        }
    }


    // Helper Class
    public static class ConvertHelper
    {
        public static string FormatDouble(object value)
        {
            if (ToDoubleZ(value) == 0) return "0";
            string formattedString = string.Format("{0:0.00}", Math.Round(ToDoubleZ(value), 2));
            if (formattedString.Substring(formattedString.LastIndexOf('.') + 1, 2) == "00")
            {
                formattedString = formattedString.Remove(formattedString.LastIndexOf('.'));
            }
            return formattedString;
        }

        public static double ToDoubleZ(object value)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString())) return 0;
            return Convert.ToDouble(value);
        }

        public static string GetChangeValueControl(object ChangeValueObject)
        {
            double ChangeValue = ToDoubleZ(ChangeValueObject);
            string FormattedValue = FormatDouble(ChangeValueObject);
            return ChangeValue > 0 ? $"<span style='color: green'>{FormattedValue}</span>" :
                   ChangeValue < 0 ? $"<span style='color: red'>{FormattedValue}</span>" :
                   $"<span style='color: black'>{FormattedValue}</span>";
        }

        public static string GetChangeValueControlPercent(object ChangeValueObject)
        {
            double ChangeValue = ToDoubleZ(ChangeValueObject);
            string FormattedValue = FormatDouble(ChangeValueObject);
            return ChangeValue > 0 ? $"<span style='color: green'>{FormattedValue}%</span>" :
                   ChangeValue < 0 ? $"<span style='color: red'>{FormattedValue}%</span>" :
                   $"<span style='color: black'>{FormattedValue}%</span>";
        }

        public static string GetControlWithCompareValues(object MainValueObject, object OtherValueObject)
        {
            double MainValue = Math.Round(ToDoubleZ(MainValueObject), 2);
            double OtherValue = ToDoubleZ(OtherValueObject);
            string FormattedValue = FormatDouble(MainValueObject);
            return MainValueObject != null && MainValue != 0 ?
                   MainValue > OtherValue ? $"<span style='color: green'>{FormattedValue}</span>" :
                   MainValue < OtherValue ? $"<span style='color: red'>{FormattedValue}</span>" :
                   $"<span style='color: black'>{FormattedValue}</span>" :
                   $"<span style='color: black'>{FormattedValue}</span>";
        }

        public static string GetControlWithCompareValuesAndControlValue(object PreviousValueObject, object CurrentValueObject, object ControlValueObject, bool withPercent)
        {
            double PreviousValue = ToDoubleZ(PreviousValueObject);
            double CurrentValue = ToDoubleZ(CurrentValueObject);
            double ControlValue = ToDoubleZ(ControlValueObject);
            string FormattedValue = FormatDouble(ControlValueObject);
            if (PreviousValue == 0 && CurrentValue == 0)
            {
                return $"<span style='color: black'>{FormattedValue}{(withPercent ? "%" : "")}</span>";
            }
            if (withPercent)
            {
                return ControlValueObject != null ?
                       CurrentValue > PreviousValue ? $"<span style='color: green'>{FormattedValue}%</span>" :
                       CurrentValue < PreviousValue ? $"<span style='color: red'>{FormattedValue}%</span>" :
                       $"<span style='color: black'>{FormattedValue}%</span>" :
                       $"<span style='color: black'>{FormattedValue}%</span>";
            }
            else
            {
                return ControlValueObject != null ?
                       CurrentValue > PreviousValue ? $"<span style='color: green'>{FormattedValue}</span>" :
                       CurrentValue < PreviousValue ? $"<span style='color: red'>{FormattedValue}</span>" :
                       $"<span style='color: black'>{FormattedValue}</span>" :
                       $"<span style='color: black'>{FormattedValue}</span>";
            }
        }

        public static string GetExpectedOpenControlWithCompareValuesAndControlValue(object MainValueObject, object OtherValueObject, object ControlValueObject, bool withPercent, object BaseValueObject)
        {
            double MainValue = ToDoubleZ(MainValueObject);
            double OtherValue = ToDoubleZ(OtherValueObject);
            double BaseValue = ToDoubleZ(BaseValueObject);
            string FormattedValue = FormatDouble(ControlValueObject);
            if (BaseValue == 0)
            {
                return $"<span style='color: black'>{BaseValue}</span>";
            }
            if (withPercent)
            {
                return ControlValueObject != null ?
                       MainValue > OtherValue ? $"<span style='color: green'>{FormattedValue}%</span>" :
                       MainValue < OtherValue ? $"<span style='color: red'>{FormattedValue}%</span>" :
                       $"<span style='color: black'>{FormattedValue}%</span>" :
                       $"<span style='color: black'>{FormattedValue}%</span>";
            }
            else
            {
                return ControlValueObject != null ?
                       MainValue > OtherValue ? $"<span style='color: green'>{FormattedValue}</span>" :
                       MainValue < OtherValue ? $"<span style='color: red'>{FormattedValue}</span>" :
                       $"<span style='color: black'>{FormattedValue}</span>" :
                       $"<span style='color: black'>{FormattedValue}</span>";
            }
        }

        public static string GetStopLossChangeValueColor(object SupportValueObject, object MinCloseValueObject)
        {
            double SupportValue = Math.Round(ToDoubleZ(SupportValueObject), 2);
            double MinCloseValue = ToDoubleZ(MinCloseValueObject);
            string FormattedValue = FormatDouble(SupportValueObject);
            return MinCloseValue != 0 ?
                   MinCloseValue < SupportValue ? $"<span style='color: red'>{FormattedValue}</span>" :
                   $"<span style='color: black'>{FormattedValue}</span>" :
                   $"<span style='color: black'>{FormattedValue}</span>";
        }

        public static string GetSupportChangeValueColor(object SupportValueObject, object MinLowValueObject)
        {
            double SupportValue = Math.Round(ToDoubleZ(SupportValueObject), 2);
            double MinLowValue = ToDoubleZ(MinLowValueObject);
            string FormattedValue = FormatDouble(SupportValueObject);
            return MinLowValue != 0 ?
                   MinLowValue <= SupportValue ? $"<span style='color: red'>{FormattedValue}</span>" :
                   $"<span style='color: black'>{FormattedValue}</span>" :
                   $"<span style='color: black'>{FormattedValue}</span>";
        }

        public static string GetTargetChangeValueColor(object TargetValueObject, object MaxHighValueObject)
        {
            double TargetValue = Math.Round(ToDoubleZ(TargetValueObject), 2);
            double MaxHighValue = ToDoubleZ(MaxHighValueObject);
            string FormattedValue = FormatDouble(TargetValueObject);
            return MaxHighValue != 0 ?
                   MaxHighValue >= TargetValue ? $"<span style='color: green'>{FormattedValue}</span>" :
                   $"<span style='color: black'>{FormattedValue}</span>" :
                   $"<span style='color: black'>{FormattedValue}</span>";
        }
    }





}