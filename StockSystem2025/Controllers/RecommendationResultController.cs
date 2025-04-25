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

namespace StockSystem2025.Controllers
{
    public class RecommendationResultController : Controller
    {
        private readonly StockdbContext _context;
        private readonly ILogger<RecommendationResultController> _logger;

        public RecommendationResultController(ILogger<RecommendationResultController> logger, StockdbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> RecommendationResult(int criteriaId, string selectedDate)
        {
            try
            {
                var userId = 1; // Replace with authentication logic (e.g., User.FindFirstValue(ClaimTypes.NameIdentifier))
                DateTime parsedDate;
                if (!DateTime.TryParse(selectedDate, out parsedDate))
                {
                    var maxDate = await _context.StockTables.MaxAsync(x => x.Sdate);
                    parsedDate = DateTime.Parse(maxDate ?? DateTime.Today.ToString("yyyy-MM-dd"));
                }

                var model = new RecommendationResultViewModel
                {
                    CriteriaId = criteriaId,
                    SelectedDate = parsedDate,
                    StopLossValue = await GetSettingDouble("StopLossValue", 3),
                    FirstSupportValue = await GetSettingDouble("FirstSupportValue", 1),
                    SecondSupportValue = await GetSettingDouble("SecondSupportValue", 2),
                    FirstTargetValue = await GetSettingDouble("FirstTargetValue", 1),
                    SecondTargetValue = await GetSettingDouble("SecondTargetValue", 4),
                    ThirdTargetValue = await GetSettingDouble("ThirdTargetValue", 5),
                    // Update the following line to handle the possible null value of `criteria.Type`:
                    SpecialCompanyColor = await _context.Settings.Where(x => x.Name == "SpecialCompanyColor").Select(x => x.Value ?? "#f0f0f0").FirstOrDefaultAsync()
                };

                // Fetch criteria and calculate index
                var criteria = await _context.Criterias.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == criteriaId);
                if (criteria != null)
                {
                    model.CriteriaName = criteria.Name ?? string.Empty;
                    model.CriteriaType = criteria.Type ?? string.Empty;
                    model.CriteriaNote = criteria.Note ?? string.Empty;
                    model.CriteriaSeparater = criteria.Separater ?? string.Empty;
                    model.CriteriaIndex = await _context.Criterias
                        .AsNoTracking()
                        .CountAsync(x => x.Id < criteriaId) + 1;
                }
                else
                {
                    _logger.LogWarning("Criteria with Id {CriteriaId} not found.", criteriaId);
                }

                // Load general indicators
                var sDate = parsedDate.ToString("yyyy-MM-dd");

                string formattedSDate = DateTime.ParseExact(sDate, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                   .ToString("yyyy/MM/dd"); // تحويل إلى "2021/10/14"


                model.GeneralIndicators = await _context.StockPrevDayViews.AsNoTracking()
                    .Where(x => x.Sticker == "TASI" && x.Sdate == formattedSDate)
                    .Select(x => new GeneralIndicator
                    {
                        Sticker = x.Sticker,
                        Sname = x.Sname ?? string.Empty,
                        Sopen = x.Sopen ?? 0,
                        Shigh = x.Shigh ?? 0,
                        Slow = x.Slow ?? 0,
                        Sclose = x.Sclose ?? 0,
                        Svol = x.Svol ?? 0,
                        ChangeValue = x.ChangeValue ?? 0,
                        ChangeRate = x.ChangeRate ?? 0,
                        IndicatorIn = x.IndicatorIn ?? 0,
                        IndicatorOut = x.IndicatorOut ?? 0
                    }).ToListAsync();
                if (!model.GeneralIndicators.Any())
                {
                    model.GeneralIndicators.Add(new GeneralIndicator
                    {
                        Sticker = "TASI",
                        Sname = "TASI Index",
                        Sopen = 0,
                        Shigh = 0,
                        Slow = 0,
                        Sclose = 0,
                        Svol = 0
                    });
                }

                // Sector and company statistics
                model.IndicatorsUpCount = await _context.StockPrevDayViews.AsNoTracking()
                    .CountAsync(x => x.IsIndicator && x.Sdate == formattedSDate && x.ChangeRate > 0);
                model.IndicatorsDownCount = await _context.StockPrevDayViews.AsNoTracking()
                    .CountAsync(x => x.IsIndicator && x.Sdate == formattedSDate && x.ChangeRate < 0);
                model.IndicatorsNoChangeCount = await _context.StockPrevDayViews.AsNoTracking()
                    .CountAsync(x => x.IsIndicator && x.Sdate == formattedSDate && x.ChangeRate == 0);
                model.CompaniesUpCount = await _context.StockPrevDayViews.AsNoTracking()
                    .CountAsync(x => !x.IsIndicator && x.Sdate == formattedSDate && x.ChangeRate > 0);
                model.CompaniesDownCount = await _context.StockPrevDayViews.AsNoTracking()
                    .CountAsync(x => !x.IsIndicator && x.Sdate == formattedSDate && x.ChangeRate < 0);
                model.CompaniesNoChangeCount = await _context.StockPrevDayViews.AsNoTracking()
                    .CountAsync(x => !x.IsIndicator && x.Sdate == formattedSDate && x.ChangeRate == 0);

                // Load follow lists
                model.FollowLists = await _context.FollowLists
                    .Where(f => f.UserId == userId)
                    .Select(f => new FollowList
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Color = f.Color ?? "#FFFFFF"
                    }).ToListAsync();

                // Date boundaries
                var dates = await _context.StockTables.Select(x => x.Sdate).ToListAsync();
                model.MinDate = dates.Any() ? dates.Min(d => DateTime.Parse(d)) : null;
                model.MaxDate = dates.Any() ? dates.Max(d => DateTime.Parse(d)) : null;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RecommendationResult for CriteriaId: {CriteriaId}, SelectedDate: {SelectedDate}", criteriaId, selectedDate);
                throw; // Rethrow for debugging; consider redirecting to an error page in production
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToFollowList(int followListId, string companyCode)
        {
            try
            {
                if (string.IsNullOrEmpty(companyCode))
                {
                    return Json(new { success = false, message = "رمز الشركة غير صالح" });
                }

                if (!await _context.FollowListCompanies.AnyAsync(x => x.FollowListId == followListId && x.CompanyCode == companyCode))
                {
                    _context.FollowListCompanies.Add(new FollowListCompany
                    {
                        FollowListId = followListId,
                        CompanyCode = companyCode
                    });
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "الشركة موجودة بالفعل في قائمة المتابعة" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding company {CompanyCode} to follow list {FollowListId}", companyCode, followListId);
                return Json(new { success = false, message = "حدث خطأ أثناء إضافة الشركة" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecommendations(int criteriaId, string selectedDate, bool isSecondView)
        {
            try
            {
                // التحقق من صحة التاريخ
                if (string.IsNullOrEmpty(selectedDate) || !DateTime.TryParse(selectedDate, out _))
                {
                    _logger.LogWarning("Invalid date format: {SelectedDate}", selectedDate);
                    return Json(new { data = new List<object>(), error = "تاريخ غير صالح" });
                }

                var sDate = DateTime.Parse(selectedDate).ToString("yyyy-MM-dd");

                // جلب بيانات criteriaResult من GetCriteriaRecommendations
                var criteriaResult = await GetCriteriaRecommendations(criteriaId, sDate);

                // إذا لم تكن هناك بيانات، أرجع قائمة فارغة
                if (!criteriaResult.Any())
                {
                    _logger.LogInformation("No data found for criteriaId: {CriteriaId}, selectedDate: {SelectedDate}", criteriaId, selectedDate);
                    return Json(new { data = new List<object>() });
                }

                // استخراج قائمة Sticker من criteriaResult (تجاهل القيم الفارغة)
                var stickers = criteriaResult
                    .Select(r => r.Sticker)
                    .Where(s => s != null)
                    .Distinct()
                    .ToList();

                // Log the stickers list
                _logger.LogInformation("Stickers extracted: {Stickers}", string.Join(", ", stickers));

                // 1. سحب البيانات بدون فلترة
                var followListData = await _context.FollowListCompanies
                    .AsNoTracking()
                    .Join(_context.FollowLists,
                          fc => fc.FollowListId,
                          fl => fl.Id,
                          (fc, fl) => new { fc.CompanyCode, fl.Name, fl.Color })
                    .ToListAsync();

                // 2. فلترة محلياً باستخدام Contains
                followListData = followListData
                    .Where(x => stickers.Contains(x.CompanyCode!))
                    .ToList();

                // Log the filtered followListData
                _logger.LogInformation("Filtered followListData count: {Count}", followListData.Count);

                // تجميع FollowListNames في الذاكرة
                var followListNames = followListData
                    .Where(x => x.CompanyCode != null)
                    .GroupBy(x => x.CompanyCode!)
                    .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => x.Name).Where(n => !string.IsNullOrEmpty(n))));

                // Fix for CS8714 and CS8621
                var followListColors = followListData
                    .Where(x => x.CompanyCode != null)
                    .GroupBy(x => x.CompanyCode!)
                    .ToDictionary(g => g.Key, g => g.First().Color ?? "");

                // جلب قيم الإعدادات مرة واحدة
                var stopLossValue = await GetSettingDouble("StopLossValue", 3);
                var firstSupportValue = await GetSettingDouble("FirstSupportValue", 1);
                var secondSupportValue = await GetSettingDouble("SecondSupportValue", 2);
                var firstTargetValue = await GetSettingDouble("FirstTargetValue", 1);
                var secondTargetValue = await GetSettingDouble("SecondTargetValue", 4);
                var thirdTargetValue = await GetSettingDouble("ThirdTargetValue", 5);

                var data = criteriaResult.Select(r => new
                {
                    sticker = r.Sticker ?? "", // Lowercase
                    sname = r.Sname ?? "",
                    sclose = r.Sclose?.ToString("F2") ?? "0.00",
                    nextsclose = r.NextSclose?.ToString("F2") ?? "0.00",
                    expectedopen = r.ExpectedOpen?.ToString("F2") ?? "0.00",
                    expectedopenvalue = r.ExpectedOpenValue?.ToString("F2") ?? "0.00",
                    expectedopenpercent = r.ExpectedOpenPercent?.ToString("F2") + "%" ?? "0.00%",
                    prevsopen = r.PrevSopen?.ToString("F2") ?? "0.00",
                    openinggapvalue = r.OpeningGapValue?.ToString("F2") ?? "0.00",
                    openinggaprate = r.OpeningGapRate?.ToString("F2") + "%" ?? "0.00%",
                    prevshigh = r.PrevShigh?.ToString("F2") ?? "0.00",
                    upperlimitvalue = r.UpperLimitValue?.ToString("F2") ?? "0.00",
                    upperlimitrate = r.UpperLimitRate?.ToString("F2") + "%" ?? "0.00%",
                    prevslow = r.PrevSlow?.ToString("F2") ?? "0.00",
                    lowerlimitvalue = r.LowerLimitValue?.ToString("F2") ?? "0.00",
                    lowerlimitrate = r.LowerLimitRate?.ToString("F2") + "%" ?? "0.00%",
                    prevsclose = r.PrevSclose?.ToString("F2") ?? "0.00",
                    changevalue = r.ChangeValue?.ToString("F2") ?? "0.00",
                    changerate = r.ChangeRate?.ToString("F2") + "%" ?? "0.00%",
                    isindicator = r.IsIndicator,
                    isspecial = r.IsSpecial,
                    followlistnames = followListNames.ContainsKey(r.Sticker ?? "") ? followListNames[r.Sticker ?? ""] : "",
                    followlistcolor = followListColors.ContainsKey(r.Sticker ?? "") ? followListColors[r.Sticker ?? ""] : "",
                    stoploss = r.Sclose.HasValue ? (r.Sclose.Value * (1 - stopLossValue / 100)).ToString("F2") : "0.00",
                    firstsupport = r.Sclose.HasValue ? (r.Sclose.Value * (1 - firstSupportValue / 100)).ToString("F2") : "0.00",
                    secondsupport = r.Sclose.HasValue ? (r.Sclose.Value * (1 - secondSupportValue / 100)).ToString("F2") : "0.00",
                    firsttarget = r.Sclose.HasValue ? (r.Sclose.Value * (1 + firstTargetValue / 100)).ToString("F2") : "0.00",
                    secondtarget = r.Sclose.HasValue ? (r.Sclose.Value * (1 + secondTargetValue / 100)).ToString("F2") : "0.00",
                    thirdtarget = r.Sclose.HasValue ? (r.Sclose.Value * (1 + thirdTargetValue / 100)).ToString("F2") : "0.00",
                    currentprice = "0.00",
                    dailytargetvalue = r.PrevShigh?.ToString("F2") ?? "0.00",
                    dailytargetpercent = r.PrevShigh != 0 && r.Sclose.HasValue ? ((r.PrevShigh - r.Sclose) / r.Sclose * 100)?.ToString("F2") + "%" : "0.00%",
                    weeklytargetvalue = "0.00",
                    weeklytargetpercent = "0.00%"
                }).ToList();

                // Log the final data
                _logger.LogInformation("GetRecommendations returning data with {Count} items: {Data}", data.Count, JsonSerializer.Serialize(data));

                return Json(new { data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recommendations for CriteriaId: {CriteriaId}, SelectedDate: {SelectedDate}", criteriaId, selectedDate);
                return Json(new { data = new List<object>(), error = "حدث خطأ أثناء جلب التوصيات" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CheckDate(string date)
        {
            try
            {
                if (string.IsNullOrEmpty(date))
                {
                    return Json(new { exists = false });
                }
                var exists = await _context.StockTables.AnyAsync(x => x.Sdate == date);
                return Json(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking date: {Date}", date);
                return Json(new { exists = false });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNextDate(string currentDate)
        {
            try
            {
                if (string.IsNullOrEmpty(currentDate) || !DateTime.TryParse(currentDate, out _))
                {
                    return Json(new { success = false, date = "", message = "تاريخ غير صالح" });
                }

                var nextDate = await _context.StockTables
                    .Where(x => DateTime.Parse(x.Sdate) > DateTime.Parse(currentDate))
                    .MinAsync(x => x.Sdate);
                if (nextDate == null)
                {
                    return Json(new { success = false, date = "", message = "لا يوجد تاريخ لاحق" });
                }
                return Json(new { success = true, date = nextDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next date for CurrentDate: {CurrentDate}", currentDate);
                return Json(new { success = false, date = "", message = "حدث خطأ أثناء جلب التاريخ التالي" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPreviousDate(string currentDate)
        {
            try
            {
                if (string.IsNullOrEmpty(currentDate) || !DateTime.TryParse(currentDate, out _))
                {
                    return Json(new { success = false, date = "", message = "تاريخ غير صالح" });
                }

                var prevDate = await _context.StockTables
                    .Where(x => DateTime.Parse(x.Sdate) < DateTime.Parse(currentDate))
                    .MaxAsync(x => x.Sdate);
                if (prevDate == null)
                {
                    return Json(new { success = false, date = "", message = "لا يوجد تاريخ سابق" });
                }
                return Json(new { success = true, date = prevDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting previous date for CurrentDate: {CurrentDate}", currentDate);
                return Json(new { success = false, date = "", message = "حدث خطأ أثناء جلب التاريخ السابق" });
            }
        }

        private async Task<List<RecommendationsResultsView>> GetCriteriaRecommendations(int criteriaId, string sDate)
        {
            try
            {
                var formulaIds = await _context.Formulas
                    .Where(f => f.CriteriaId == criteriaId)
                    .Select(f => f.Id)
                    .ToListAsync();

                // افترض أن sDate هو "2021-10-14" (تنسيق yyyy-MM-dd)
                string formattedSDate = DateTime.ParseExact(sDate, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                    .ToString("yyyy/MM/dd"); // تحويل إلى "2021/10/14"

                // الاستعلام
                var criteriaResult = await _context.StockPrevDayViews.AsNoTracking()
                    .Where(x => x.Sdate == formattedSDate)
                    .ToListAsync();


              var  RecommendationsResultsViews= await _context.RecommendationsResultsViews.AsNoTracking()
                    .Where(x => x.Sdate == formattedSDate)
                    .ToListAsync();

                return RecommendationsResultsViews.Where(x => criteriaResult.Any(c => c.Sticker == x.Sticker)).ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting criteria recommendations for CriteriaId: {CriteriaId}, SDate: {SDate}", criteriaId, sDate);
                throw;
            }
        }

        private async Task<double> GetSettingDouble(string name, double defaultValue)
        {
            try
            {
                var value = await _context.Settings
                    .Where(x => x.Name == name)
                    .Select(x => x.Value)
                    .FirstOrDefaultAsync();
                return double.TryParse(value, out var result) ? result : defaultValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting setting: {Name}", name);
                return defaultValue;
            }
        }
    }
}