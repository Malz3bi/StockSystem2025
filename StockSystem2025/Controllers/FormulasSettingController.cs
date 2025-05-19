using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using StockSystem2025.Models;
using StockSystem2025.ViewModel;

namespace StockSystem2025.Controllers
{
    public class FormulasSettingController : Controller
    {
        private readonly ILogger<UploadFileController> _logger;
        private readonly IMemoryCache _cache;
        private readonly StockdbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public FormulasSettingController(
            StockdbContext context,
            ILogger<UploadFileController> logger,
            IMemoryCache cache,
            IServiceProvider serviceProvider)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
            _serviceProvider = serviceProvider;
        }

        public async Task<IActionResult> FormulasSettingIndex(int page = 1, int pageSize = 50)
        {
            return View(await LoadData(page, pageSize));
        }

        private async Task<FormulasSettingViewModel> LoadData(int page = 1, int pageSize = 50)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // التخزين المؤقت للبيانات الثابتة
                var cacheKey = "StaticData";
                if (!_cache.TryGetValue(cacheKey, out (List<CompanyTable> Companies, bool ViewCompaniesCount) staticData))
                {
                    staticData.Companies = await _context.CompanyTables
                        .AsNoTracking()
                        .Select(c => new CompanyTable { CompanyId = c.CompanyId, CompanyName = c.CompanyName })
                        .ToListAsync();

                    var settings = await _context.Settings
                        .AsNoTracking()
                        .Where(x => x.Name == "ShowCompaniesCount")
                        .Select(x => x.Value == "1")
                        .FirstOrDefaultAsync();
                    staticData.ViewCompaniesCount = settings;

                    _cache.Set(cacheKey, staticData, TimeSpan.FromHours(1));
                }

                // تحميل البيانات الأساسية
                var totalRecords = await _context.Criterias.CountAsync();

                var model = new FormulasSettingViewModel
                {
                    ContentTitle = "عرض التوصيات",
                    Criteria = new List<CriteriaViewModel>(),
                    GeneralIndicators = await _context.StockPrevDayViews
                        .AsNoTracking()
                        .Where(x => x.Sticker == "TASI" && x.DayNo == 1)
                        .Select(x => new StockPrevDayView
                        {
                            Sticker = x.Sticker,
                            DayNo = x.DayNo,
                            Sclose = x.Sclose
                        })
                        .ToListAsync(),
                    Companies = staticData.Companies,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords
                };

                // تحميل المعايير
                var criteria = await _context.Criterias
                    .AsNoTracking()
                    .Select(c => new Criteria
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Type = c.Type,
                        Note = c.Note,
                        Separater = c.Separater,
                        OrderNo = c.OrderNo,
                        Description = c.Description,
                        Color = c.Color,
                        ImageUrl = c.ImageUrl,
                        IsIndicator = c.IsIndicator,
                        IsGeneral = c.IsGeneral,
                        UserId = c.UserId,
                        Formulas = c.Formulas.Select(f => new Formula
                        {
                            Id = f.Id,
                            Day = f.Day,
                            FormulaType = f.FormulaType,
                            FormulaValues = f.FormulaValues
                        }).ToList()
                    })
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // تحميل MinDate و MaxDate في استعلام واحد
                var dateRange = await _context.StockTables
                    .AsNoTracking()
                    .GroupBy(x => 1)
                    .Select(g => new { MinDate = g.Min(x => x.Createddate), MaxDate = g.Max(x => x.Createddate) })
                    .FirstOrDefaultAsync();
                model.MinDate = dateRange?.MinDate;
                model.MaxDate = dateRange?.MaxDate;

                // تحميل DayNo للتاريخ الأقصى
                var maxDateDayNo = await _context.StockTables
                    .AsNoTracking()
                    .Where(x => x.Createddate == model.MaxDate)
                    .Select(x => x.DayNo)
                    .FirstOrDefaultAsync();
                int startDayNo = maxDateDayNo > 0 ? maxDateDayNo : 1;

                // تحميل StockPrevDayViews للأيام المطلوبة
                var requiredDays = criteria
                    .SelectMany(c => c.Formulas)
                    .Select(f => startDayNo + f.Day - 1)
                    .Distinct()
                    .ToList();

                var stockPrevDayViews = await _context.StockPrevDayViews
                    .AsNoTracking()
                    .Where(x => requiredDays.Contains(x.DayNo) && x.ParentIndicator != null)
                    .Select(x => new StockPrevDayView
                    {
                        Sticker = x.Sticker,
                        DayNo = x.DayNo,
                        Sopen = x.Sopen,
                        Sclose = x.Sclose,
                        Shigh = x.Shigh,
                        Slow = x.Slow,
                        Svol = x.Svol,
                        PrevSopen = x.PrevSopen,
                        PrevSclose = x.PrevSclose,
                        PrevShigh = x.PrevShigh,
                        PrevSlow = x.PrevSlow,
                        PrevSvol = x.PrevSvol,
                        IsIndicator = x.IsIndicator,
                        ParentIndicator = x.ParentIndicator
                    })
                    .ToDictionaryAsync(x => (x.Sticker, x.DayNo), x => x);

                // تحميل تاريخ بداية المعايير
                string? criteriaStartDateString = HttpContext.Session.GetString("CriteriaStartDate");
                DateTime? criteriaStartDate = criteriaStartDateString != null
                    ? DateTime.Parse(criteriaStartDateString)
                    : model.MaxDate;

                // إعداد قاموس الصيغ
                var formulaFactories = new Dictionary<int, Func<string[], object>>
                {
                    { 1, values => new Formula1
                        {
                            TypeAll = bool.Parse(values[0]),
                            TypePositive = bool.Parse(values[1]),
                            TypeNegative = bool.Parse(values[2]),
                            TypeNoChange = bool.Parse(values[3]),
                            GreaterThan = double.TryParse(values[4], out var gt) ? gt : (double?)null,
                            LessThan = double.TryParse(values[5], out var lt) ? lt : (double?)null
                        }
                    },
                    { 2, values => new Formula2
                        {
                            GreaterThan = double.TryParse(values[0], out var gt) ? gt : (double?)null,
                            LessThan = double.TryParse(values[1], out var lt) ? lt : (double?)null
                        }
                    },
                    { 3, values => new Formula3
                        {
                            TypeAll = bool.Parse(values[0]),
                            TypeHigherGap = bool.Parse(values[1]),
                            TypeLowerGap = bool.Parse(values[2]),
                            GreaterThan = double.TryParse(values[3], out var gt) ? gt : (double?)null,
                            LessThan = double.TryParse(values[4], out var lt) ? lt : (double?)null
                        }
                    },
                    { 4, values => new Formula4
                        {
                            TypeAll = bool.Parse(values[0]),
                            TypeHigher = bool.Parse(values[1]),
                            TypeLower = bool.Parse(values[2]),
                            GreaterThan = double.TryParse(values[3], out var gt) ? gt : (double?)null,
                            LessThan = double.TryParse(values[4], out var lt) ? lt : (double?)null
                        }
                    },
                    { 5, values => new Formula5
                        {
                            TypeAll = bool.Parse(values[0]),
                            TypeHigherGap = bool.Parse(values[1]),
                            TypeLowerGap = bool.Parse(values[2])
                        }
                    },
                    { 6, values => new Formula6
                        {
                            Between = double.TryParse(values[0], out var bt) ? bt : (double?)null,
                            And = double.TryParse(values[1], out var and) ? and : (double?)null
                        }
                    },
                    { 7, values => new Formula7
                        {
                            Between = double.TryParse(values[0], out var bt) ? bt : (double?)null,
                            And = double.TryParse(values[1], out var and) ? and : (double?)null
                        }
                    },
                    { 8, values => new Formula8
                        {
                            Between = double.TryParse(values[0], out var bt) ? bt : (double?)null,
                            And = double.TryParse(values[1], out var and) ? and : (double?)null
                        }
                    },
                    { 9, values => new Formula9
                        {
                            TypeAll = bool.Parse(values[0]),
                            TypePositive = bool.Parse(values[1]),
                            TypeNegative = bool.Parse(values[2]),
                            GreaterThan = double.TryParse(values[3], out var gt) ? gt : (double?)null,
                            LessThan = double.TryParse(values[4], out var lt) ? lt : (double?)null
                        }
                    },
                    { 10, values => new Formula10
                        {
                            Between = double.TryParse(values[0], out var bt) ? bt : (double?)null,
                            And = double.TryParse(values[1], out var and) ? and : (double?)null
                        }
                    },
                    { 11, values => new Formula11
                        {
                            MaximumAll = bool.Parse(values[0]),
                            MaximumGreater = bool.Parse(values[1]),
                            MaximumLess = bool.Parse(values[2]),
                            MaximumBetween = double.TryParse(values[3], out var mb) ? mb : (double?)null,
                            MaximumAnd = double.TryParse(values[4], out var ma) ? ma : (double?)null,
                            MinimumAll = bool.Parse(values[5]),
                            MinimumGreater = bool.Parse(values[6]),
                            MinimumLess = bool.Parse(values[7]),
                            MinimumBetween = double.TryParse(values[8], out var mib) ? mib : (double?)null,
                            MinimumAnd = double.TryParse(values[9], out var mia) ? mia : (double?)null
                        }
                    },
                    { 12, values => new Formula12
                        {
                            TypeAll = bool.Parse(values[0]),
                            TypeGreater = bool.Parse(values[1]),
                            TypeLess = bool.Parse(values[2]),
                            GreaterThan = double.TryParse(values[3], out var gt) ? gt : (double?)null,
                            LessThan = double.TryParse(values[4], out var lt) ? lt : (double?)null
                        }
                    },
                    { 13, values => new Formula13
                        {
                            TypeAll = bool.Parse(values[0]),
                            TypePositive = bool.Parse(values[1]),
                            TypeNegative = bool.Parse(values[2]),
                            Days = int.TryParse(values[3], out var days) ? days : (int?)null,
                            GreaterThan = double.TryParse(values[4], out var gt) ? gt : (double?)null,
                            LessThan = double.TryParse(values[5], out var lt) ? lt : (double?)null
                        }
                    },
                    { 14, values => new Formula14
                        {
                            TypeAll = bool.Parse(values[0]),
                            TypeGreater = bool.Parse(values[1]),
                            TypeLess = bool.Parse(values[2]),
                            GreaterThan = double.TryParse(values[3], out var gt) ? gt : (double?)null,
                            LessThan = double.TryParse(values[4], out var lt) ? lt : (double?)null
                        }
                    },
                    { 15, values => new Formula15
                        {
                            Between = double.TryParse(values[0], out var bt) ? bt : (double?)null,
                            And = double.TryParse(values[1], out var and) ? and : (double?)null
                        }
                    },
                    { 16, values => new Formula16 { } },
                    { 17, values => new Formula17
                        {
                            TypeAll = bool.Parse(values[0]),
                            TypeGreater = bool.Parse(values[1]),
                            TypeLess = bool.Parse(values[2]),
                            FromDays = int.TryParse(values[3], out var fd) ? fd : (int?)null,
                            ToDays = int.TryParse(values[4], out var td) ? td : (int?)null,
                            GreaterThan = double.TryParse(values[5], out var gt) ? gt : (double?)null,
                            LessThan = double.TryParse(values[6], out var lt) ? lt : (double?)null
                        }
                    }
                };

                // معالجة المعايير باستخدام المعالجة الموازية
                var criteriaList = new ConcurrentBag<CriteriaViewModel>();
                if (staticData.ViewCompaniesCount && criteria.Any())
                {
                    // تحميل البيانات المشتركة مسبقًا
                    var recommendationsList = await _context.RecommendationsResultsViews
                        .AsNoTracking()
                        .Select(x => new RecommendationsResultsView
                        {
                            Sticker = x.Sticker,
                            DayNo = x.DayNo,
                            Shigh = x.Shigh,
                            NextShigh = x.NextShigh,
                            PrevShigh = x.PrevShigh
                        })
                        .ToListAsync();

                    await Parallel.ForEachAsync(criteria, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (criteriaItem, ct) =>
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<StockdbContext>();

                            var oneCriteriaVM = new CriteriaViewModel
                            {
                                Criteria = new Criteria
                                {
                                    Id = criteriaItem.Id,
                                    Name = criteriaItem.Name,
                                    Type = criteriaItem.Type,
                                    Note = criteriaItem.Note,
                                    Separater = criteriaItem.Separater,
                                    OrderNo = criteriaItem.OrderNo,
                                    Description = criteriaItem.Description,
                                    Color = criteriaItem.Color,
                                    ImageUrl = criteriaItem.ImageUrl,
                                    IsIndicator = criteriaItem.IsIndicator,
                                    IsGeneral = criteriaItem.IsGeneral,
                                    UserId = criteriaItem.UserId
                                }
                            };

                            // معالجة الصيغ
                            var stockResult = stockPrevDayViews
                                .Where(x => x.Value.ParentIndicator != null && x.Value.DayNo == startDayNo)
                                .Select(x => x.Value)
                                .ToList();

                            if (criteriaItem.IsIndicator == 0)
                                stockResult = stockResult.Where(x => x.IsIndicator == false).ToList();
                            else if (criteriaItem.IsIndicator == 1)
                                stockResult = stockResult.Where(x => x.IsIndicator == true).ToList();

                            var formulasGroup = criteriaItem.Formulas.OrderByDescending(x => x.Day).GroupBy(x => x.Day);
                            foreach (var groupItem in formulasGroup)
                            {
                                int formulaDayNo = startDayNo + groupItem.First().Day - 1;

                              
                                var stockStickers = new HashSet<string>(stockResult.Select(y => y.Sticker));
                                var res = stockPrevDayViews
                                    .Where(x => x.Key.DayNo == formulaDayNo && stockStickers.Contains(x.Key.Sticker))
                                    .Select(x => x.Value)
                                    .ToList();

                                stockResult = res;
                                foreach (var formulaItem in groupItem)
                                {
                                    string[] formulaValuesArray = formulaItem.FormulaValues.Split(';');
                                    if (!formulaFactories.TryGetValue(formulaItem.FormulaType, out var factory))
                                        continue;

                                    var formula = factory(formulaValuesArray);
                                    switch (formulaItem.FormulaType)
                                    {
                                        case 1:
                                            var formula1 = (Formula1)formula;
                                            if (formula1.GreaterThan.HasValue && formula1.LessThan.HasValue && formula1.GreaterThan > formula1.LessThan)
                                                throw new ArgumentException("GreaterThan must be less than or equal to LessThan.");

                                            Func<double, bool> applyRangeFilter1 = change =>
                                                (formula1.GreaterThan == null || change > formula1.GreaterThan) &&
                                                (formula1.LessThan == null || change < formula1.LessThan);

                                            if (formula1.TypeAll)
                                            {
                                                stockResult = stockResult.Where(stock =>
                                                    stock.Sclose.HasValue &&
                                                    stock.Sclose != 0 &&
                                                    stock.Shigh.HasValue &&
                                                    stock.Shigh != 0 &&
                                                    stock.Slow.HasValue &&
                                                    stock.Slow != 0 &&
                                                    stock.PrevShigh.HasValue &&
                                                    stock.PrevShigh != 0 &&
                                                    stock.PrevSlow.HasValue &&
                                                    stock.PrevSlow != 0 &&
                                                    stock.PrevSclose.HasValue &&
                                                    stock.PrevSclose != 0
                                                ).ToList();
                                            }
                                            else
                                            {
                                                stockResult = stockResult.Where(x =>
                                                {
                                                    var change = x.Sclose.HasValue && x.PrevSclose.HasValue && x.PrevSclose != 0
                                                        ? (x.Sclose.Value - x.PrevSclose.Value) / x.PrevSclose.Value * 100
                                                        : 0;
                                                    if (formula1.TypePositive && change > 0)
                                                        return applyRangeFilter1(change);
                                                    if (formula1.TypeNegative && change < 0)
                                                        return applyRangeFilter1(-change);
                                                    if (formula1.TypeNoChange && change == 0)
                                                        return true;
                                                    return false;
                                                }).ToList();
                                            }
                                            break;

                                        case 2:
                                            var formula2 = (Formula2)formula;
                                            if (formula2.GreaterThan != null)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.Sopen != 0 && ((x.Shigh - x.Slow) / x.Sopen * 100) > formula2.GreaterThan)
                                                    .ToList();
                                            }
                                            if (formula2.LessThan != null)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.Sopen != 0 && ((x.Shigh - x.Slow) / x.Sopen * 100) < formula2.LessThan)
                                                    .ToList();
                                            }
                                            break;

                                        case 3:
                                            var formula3 = (Formula3)formula;
                                            if (formula3.TypeAll == true)
                                                break;

                                            bool InRange3(double gap) =>
                                                (!formula3.GreaterThan.HasValue || gap > formula3.GreaterThan.Value) &&
                                                (!formula3.LessThan.HasValue || gap < formula3.LessThan.Value);

                                            var fastList3 = new List<StockPrevDayView>();
                                            foreach (var x in stockResult)
                                            {
                                                if (x.Slow.HasValue && x.PrevShigh.HasValue && x.PrevShigh != 0)
                                                {
                                                    double gapUp = ((x.Slow.Value - x.PrevShigh.Value) / x.PrevShigh.Value) * 100;
                                                    if ((formula3.TypeAll || formula3.TypeHigherGap) && gapUp >= 0 && InRange3(gapUp))
                                                    {
                                                        fastList3.Add(x);
                                                        continue;
                                                    }
                                                }

                                                if ((formula3.TypeAll || formula3.TypeLowerGap) && x.Shigh.HasValue && x.PrevSlow.HasValue && x.PrevSlow != 0)
                                                {
                                                    double gapDown = ((x.Shigh.Value - x.PrevSlow.Value) / x.PrevSlow.Value) * 100;
                                                    if (gapDown <= 0 && InRange3(-gapDown))
                                                    {
                                                        fastList3.Add(x);
                                                    }
                                                }
                                            }

                                            stockResult = formula3.TypeAll
                                                ? fastList3.DistinctBy(x => x.Sticker).ToList()
                                                : fastList3;
                                            break;

                                        case 4:
                                            var formula4 = (Formula4)formula;
                                            if (formula4.TypeAll && (formula4.GreaterThan != null || formula4.LessThan != null))
                                            {
                                                var stockResult1 = stockResult
                                                    .Where(x => x.PrevShigh != 0)
                                                    .Select(x => new { Item = x, HighPercent = (x.Sopen - x.PrevShigh) / x.PrevShigh * 100 })
                                                    .Where(x => x.HighPercent >= 0)
                                                    .Where(x => (formula4.GreaterThan == null || x.HighPercent > formula4.GreaterThan) &&
                                                                (formula4.LessThan == null || x.HighPercent < formula4.LessThan))
                                                    .Select(x => x.Item);

                                                var stockResult2 = stockResult
                                                    .Where(x => x.PrevSlow != 0)
                                                    .Select(x => new { Item = x, LowPercent = (x.Sopen - x.PrevSlow) / x.PrevSlow * 100 })
                                                    .Where(x => x.LowPercent <= 0)
                                                    .Where(x => (formula4.GreaterThan == null || -x.LowPercent > formula4.GreaterThan) &&
                                                                (formula4.LessThan == null || -x.LowPercent < formula4.LessThan))
                                                    .Select(x => x.Item);

                                                stockResult = stockResult1.Concat(stockResult2).ToList();
                                            }
                                            else if (formula4.TypeHigher)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevShigh != 0)
                                                    .Select(x => new { Item = x, HighPercent = (x.Sopen - x.PrevShigh) / x.PrevShigh * 100 })
                                                    .Where(x => x.HighPercent >= 0)
                                                    .Where(x => (formula4.GreaterThan == null || x.HighPercent > formula4.GreaterThan) &&
                                                                (formula4.LessThan == null || x.HighPercent < formula4.LessThan))
                                                    .Select(x => x.Item)
                                                    .ToList();
                                            }
                                            else if (formula4.TypeLower)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevSlow != 0)
                                                    .Select(x => new { Item = x, LowPercent = (x.Sopen - x.PrevSlow) / x.PrevSlow * 100 })
                                                    .Where(x => x.LowPercent <= 0)
                                                    .Where(x => (formula4.GreaterThan == null || -x.LowPercent > formula4.GreaterThan) &&
                                                                (formula4.LessThan == null || -x.LowPercent < formula4.LessThan))
                                                    .Select(x => x.Item)
                                                    .ToList();
                                            }
                                            break;

                                        case 5:
                                            var formula5 = (Formula5)formula;
                                            if (formula5.TypeLowerGap)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x != null &&
                                                            x.Sopen.HasValue &&
                                                            x.PrevSopen.HasValue &&
                                                            x.PrevSclose.HasValue &&
                                                            x.PrevSlow.HasValue &&
                                                            x.Sopen.Value <= x.PrevSopen.Value &&
                                                            x.Sopen.Value <= x.PrevSclose.Value &&
                                                            x.Sopen.Value >= x.PrevSlow.Value).ToList();
                                            }
                                            else if (formula5.TypeHigherGap)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x != null &&
                                                            x.Sopen.HasValue &&
                                                            x.PrevSopen.HasValue &&
                                                            x.PrevSclose.HasValue &&
                                                            x.PrevShigh.HasValue &&
                                                            x.Sopen.Value >= x.PrevSopen.Value &&
                                                            x.Sopen.Value >= x.PrevSclose.Value &&
                                                            x.Sopen.Value <= x.PrevShigh.Value).ToList();
                                            }
                                            break;

                                        case 6:
                                            var formula6 = (Formula6)formula;
                                            if (formula6.Between != null)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.Shigh != x.Slow && ((x.Sopen - x.Slow) / (x.Shigh - x.Slow) * 100) >= formula6.Between)
                                                    .ToList();
                                            }
                                            if (formula6.And != null)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.Shigh != x.Slow && ((x.Sopen - x.Slow) / (x.Shigh - x.Slow) * 100) <= formula6.And)
                                                    .ToList();
                                            }
                                            break;

                                        case 7:
                                            var formula7 = (Formula7)formula;
                                            if (formula7.Between != null)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.Shigh != x.Slow && ((x.Sclose - x.Slow) / (x.Shigh - x.Slow) * 100) >= formula7.Between)
                                                    .ToList();
                                            }
                                            if (formula7.And != null)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.Shigh != x.Slow && ((x.Sclose - x.Slow) / (x.Shigh - x.Slow) * 100) <= formula7.And)
                                                    .ToList();
                                            }
                                            break;

                                        case 8:
                                            var formula8 = (Formula8)formula;
                                            if (formula8.Between != null)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevShigh != x.PrevSlow && ((x.Sopen - x.PrevSlow) / (x.PrevShigh - x.PrevSlow) * 100) >= formula8.Between)
                                                    .ToList();
                                            }
                                            if (formula8.And != null)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevShigh != x.PrevSlow && ((x.Sopen - x.PrevSlow) / (x.PrevShigh - x.PrevSlow) * 100) <= formula8.And)
                                                    .ToList();
                                            }
                                            break;

                                        case 9:
                                            var formula9 = (Formula9)formula;
                                            if (formula9.TypeAll && (formula9.GreaterThan != null || formula9.LessThan != null))
                                            {
                                                var stockResult1 = stockResult
                                                    .Where(x => x.Sopen != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Sclose - x.Sopen) / x.Sopen * 100 })
                                                    .Where(x => x.Percent > 0)
                                                    .Where(x => (formula9.GreaterThan == null || x.Percent > formula9.GreaterThan) &&
                                                                (formula9.LessThan == null || x.Percent < formula9.LessThan))
                                                    .Select(x => x.Item);

                                                var stockResult2 = stockResult
                                                    .Where(x => x.Sopen != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Sclose - x.Sopen) / x.Sopen * 100 })
                                                    .Where(x => x.Percent < 0)
                                                    .Where(x => (formula9.GreaterThan == null || -x.Percent > formula9.GreaterThan) &&
                                                                (formula9.LessThan == null || -x.Percent < formula9.LessThan))
                                                    .Select(x => x.Item);

                                                stockResult = stockResult1.Concat(stockResult2).ToList();
                                            }
                                            else if (formula9.TypePositive)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.Sopen != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Sclose - x.Sopen) / x.Sopen * 100 })
                                                    .Where(x => x.Percent > 0)
                                                    .Where(x => (formula9.GreaterThan == null || x.Percent > formula9.GreaterThan) &&
                                                                (formula9.LessThan == null || x.Percent < formula9.LessThan))
                                                    .Select(x => x.Item)
                                                    .ToList();
                                            }
                                            else if (formula9.TypeNegative)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.Sopen != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Sclose - x.Sopen) / x.Sopen * 100 })
                                                    .Where(x => x.Percent < 0)
                                                    .Where(x => (formula9.GreaterThan == null || -x.Percent > formula9.GreaterThan) &&
                                                                (formula9.LessThan == null || -x.Percent < formula9.LessThan))
                                                    .Select(x => x.Item)
                                                    .ToList();
                                            }
                                            break;

                                        case 10:
                                            var formula10 = (Formula10)formula;
                                            if (formula10.Between != null)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevShigh != x.PrevSlow && ((x.Sclose - x.PrevSlow) / (x.PrevShigh - x.PrevSlow) * 100) >= formula10.Between)
                                                    .ToList();
                                            }
                                            if (formula10.And != null)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevShigh != x.PrevSlow && ((x.Sclose - x.PrevSlow) / (x.PrevShigh - x.PrevSlow) * 100) <= formula10.And)
                                                    .ToList();
                                            }
                                            break;

                                        case 12:
                                            var formula12 = (Formula12)formula;
                                            if (formula12.TypeAll && (formula12.GreaterThan != null || formula12.LessThan != null))
                                            {
                                                var stockResult1 = stockResult
                                                    .Where(x => x.PrevSvol != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Svol - x.PrevSvol) / x.PrevSvol * 100 })
                                                    .Where(x => x.Percent > 0)
                                                    .Where(x => (formula12.GreaterThan == null || x.Percent > formula12.GreaterThan) &&
                                                                (formula12.LessThan == null || x.Percent < formula12.LessThan))
                                                    .Select(x => x.Item);

                                                var stockResult2 = stockResult
                                                    .Where(x => x.PrevSvol != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Svol - x.PrevSvol) / x.PrevSvol * 100 })
                                                    .Where(x => x.Percent < 0)
                                                    .Where(x => (formula12.GreaterThan == null || -x.Percent > formula12.GreaterThan) &&
                                                                (formula12.LessThan == null || -x.Percent < formula12.LessThan))
                                                    .Select(x => x.Item);

                                                stockResult = stockResult1.Concat(stockResult2).ToList();
                                            }
                                            else if (formula12.TypeGreater)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevSvol != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Svol - x.PrevSvol) / x.PrevSvol * 100 })
                                                    .Where(x => x.Percent > 0)
                                                    .Where(x => (formula12.GreaterThan == null || x.Percent > formula12.GreaterThan) &&
                                                                (formula12.LessThan == null || x.Percent < formula12.LessThan))
                                                    .Select(x => x.Item)
                                                    .ToList();
                                            }
                                            else if (formula12.TypeLess)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevSvol != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Svol - x.PrevSvol) / x.PrevSvol * 100 })
                                                    .Where(x => x.Percent < 0)
                                                    .Where(x => (formula12.GreaterThan == null || -x.Percent > formula12.GreaterThan) &&
                                                                (formula12.LessThan == null || -x.Percent < formula12.LessThan))
                                                    .Select(x => x.Item)
                                                    .ToList();
                                            }
                                            break;

                                        case 13:
                                            var formula13 = (Formula13)formula;
                                            if (formula13.Days != null && formula13.Days.Value > 0)
                                            {
                                                var midScloseDict = await dbContext.StockPrevDayViews
                                                    .AsNoTracking()
                                                    .Where(x => x.DayNo >= formulaDayNo)
                                                    .GroupBy(x => x.Sticker)
                                                    .Select(g => new
                                                    {
                                                        Sticker = g.Key,
                                                        SumSclose = g.OrderBy(x => x.DayNo).Take(formula13.Days.Value).Average(x => x.PrevSclose)
                                                    })
                                                    .ToDictionaryAsync(x => x.Sticker, x => x.SumSclose);

                                                if (formula13.TypeAll && (formula13.GreaterThan != null || formula13.LessThan != null))
                                                {
                                                    var stockResult1 = stockResult
                                                        .Where(x => midScloseDict.TryGetValue(x.Sticker, out var sumSclose) && sumSclose != 0)
                                                        .Select(x => new
                                                        {
                                                            Item = x,
                                                            Percent = (x.Sclose - midScloseDict[x.Sticker]) / midScloseDict[x.Sticker] * 100
                                                        })
                                                        .Where(x => x.Percent >= 0)
                                                        .Where(x => (formula13.GreaterThan == null || x.Percent >= formula13.GreaterThan) &&
                                                                    (formula13.LessThan == null || x.Percent <= formula13.LessThan))
                                                        .Select(x => x.Item);

                                                    var stockResult2 = stockResult
                                                        .Where(x => midScloseDict.TryGetValue(x.Sticker, out var sumSclose) && sumSclose != 0)
                                                        .Select(x => new
                                                        {
                                                            Item = x,
                                                            Percent = (x.Sclose - midScloseDict[x.Sticker]) / midScloseDict[x.Sticker] * 100
                                                        })
                                                        .Where(x => x.Percent <= 0)
                                                        .Where(x => (formula13.GreaterThan == null || -x.Percent >= formula13.GreaterThan) &&
                                                                    (formula13.LessThan == null || -x.Percent <= formula13.LessThan))
                                                        .Select(x => x.Item);

                                                    stockResult = stockResult1.Concat(stockResult2).ToList();
                                                }
                                                else if (formula13.TypePositive)
                                                {
                                                    stockResult = stockResult
                                                        .Where(x => midScloseDict.TryGetValue(x.Sticker, out var sumSclose) && sumSclose != 0)
                                                        .Select(x => new
                                                        {
                                                            Item = x,
                                                            Percent = (x.Sclose - midScloseDict[x.Sticker]) / midScloseDict[x.Sticker] * 100
                                                        })
                                                        .Where(x => x.Percent >= 0)
                                                        .Where(x => (formula13.GreaterThan == null || x.Percent >= formula13.GreaterThan) &&
                                                                    (formula13.LessThan == null || x.Percent <= formula13.LessThan))
                                                        .Select(x => x.Item)
                                                        .ToList();
                                                }
                                                else if (formula13.TypeNegative)
                                                {
                                                    stockResult = stockResult
                                                        .Where(x => midScloseDict.TryGetValue(x.Sticker, out var sumSclose) && sumSclose != 0)
                                                        .Select(x => new
                                                        {
                                                            Item = x,
                                                            Percent = (x.Sclose - midScloseDict[x.Sticker]) / midScloseDict[x.Sticker] * 100
                                                        })
                                                        .Where(x => x.Percent <= 0)
                                                        .Where(x => (formula13.GreaterThan == null || -x.Percent >= formula13.GreaterThan) &&
                                                                    (formula13.LessThan == null || -x.Percent <= formula13.LessThan))
                                                        .Select(x => x.Item)
                                                        .ToList();
                                                }
                                            }
                                            break;

                                        case 14:
                                            var formula14 = (Formula14)formula;
                                            if (formula14.TypeAll && (formula14.GreaterThan != null || formula14.LessThan != null))
                                            {
                                                var stockResult1 = stockResult
                                                    .Where(x => (x.Sclose + x.Slow + x.Shigh) != 0)
                                                    .Select(x => new
                                                    {
                                                        Item = x,
                                                        Avg = (x.Sclose + x.Slow + x.Shigh) / 3.0,
                                                        Percent = (x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3.0)) / ((x.Sclose + x.Slow + x.Shigh) / 3.0) * 100
                                                    })
                                                    .Where(x => x.Percent >= 0)
                                                    .Where(x => (formula14.GreaterThan == null || x.Percent > formula14.GreaterThan) &&
                                                                (formula14.LessThan == null || x.Percent < formula14.LessThan))
                                                    .Select(x => x.Item);

                                                var stockResult2 = stockResult
                                                    .Where(x => (x.Sclose + x.Slow + x.Shigh) != 0)
                                                    .Select(x => new
                                                    {
                                                        Item = x,
                                                        Avg = (x.Sclose + x.Slow + x.Shigh) / 3.0,
                                                        Percent = (x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3.0)) / ((x.Sclose + x.Slow + x.Shigh) / 3.0) * 100
                                                    })
                                                    .Where(x => x.Percent <= 0)
                                                    .Where(x => (formula14.GreaterThan == null || -x.Percent > formula14.GreaterThan) &&
                                                                (formula14.LessThan == null || -x.Percent < formula14.LessThan))
                                                    .Select(x => x.Item);

                                                stockResult = stockResult1.Concat(stockResult2).ToList();
                                            }
                                            else if (formula14.TypeGreater)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => (x.Sclose + x.Slow + x.Shigh) != 0)
                                                    .Select(x => new
                                                    {
                                                        Item = x,
                                                        Avg = (x.Sclose + x.Slow + x.Shigh) / 3.0,
                                                        Percent = (x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3.0)) / ((x.Sclose + x.Slow + x.Shigh) / 3.0) * 100
                                                    })
                                                    .Where(x => x.Percent >= 0)
                                                    .Where(x => (formula14.GreaterThan == null || x.Percent > formula14.GreaterThan) &&
                                                                (formula14.LessThan == null || x.Percent < formula14.LessThan))
                                                    .Select(x => x.Item)
                                                    .ToList();
                                            }
                                            else if (formula14.TypeLess)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => (x.Sclose + x.Slow + x.Shigh) != 0)
                                                    .Select(x => new
                                                    {
                                                        Item = x,
                                                        Avg = (x.Sclose + x.Slow + x.Shigh) / 3.0,
                                                        Percent = (x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3.0)) / ((x.Sclose + x.Slow + x.Shigh) / 3.0) * 100
                                                    })
                                                    .Where(x => x.Percent <= 0)
                                                    .Where(x => (formula14.GreaterThan == null || -x.Percent > formula14.GreaterThan) &&
                                                                (formula14.LessThan == null || -x.Percent < formula14.LessThan))
                                                    .Select(x => x.Item)
                                                    .ToList();
                                            }
                                            break;

                                        case 15:
                                            var formula15 = (Formula15)formula;
                                            if (formula15.Between != null || formula15.And != null)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevShigh != x.PrevSlow)
                                                    .Select(x => new
                                                    {
                                                        Item = x,
                                                        Percent = (x.Shigh - x.PrevSlow) / (x.PrevShigh - x.PrevSlow) * 100
                                                    })
                                                    .Where(x => (formula15.Between == null || x.Percent >= formula15.Between) &&
                                                                (formula15.And == null || x.Percent <= formula15.And))
                                                    .Select(x => x.Item)
                                                    .ToList();
                                            }
                                            break;

                                        case 17:
                                            var formula17 = (Formula17)formula;
                                            if (formula17.FromDays != null && formula17.FromDays.Value > 0 && formula17.ToDays != null && formula17.ToDays.Value > 0)
                                            {
                                                var listOfCodes = stockResult.Select(y => y.Sticker).Distinct().ToList();
                                                int relatedDayNo = startDayNo - 1;
                                                int totalFromDayNo = formula17.FromDays.Value + relatedDayNo;
                                                int totalToDayNo = formula17.ToDays.Value + relatedDayNo;

                                                var filteredRecommendations = recommendationsList
                                                    .Where(x => x.DayNo >= totalFromDayNo && x.DayNo <= totalToDayNo && listOfCodes.Contains(x.Sticker))
                                                    .ToList();

                                                var recommendationsDict = filteredRecommendations
                                                    .GroupBy(x => x.Sticker)
                                                    .ToDictionary(g => g.Key, g => g.ToList());

                                                var filteredStockResult = recommendationsDict
                                                    .SelectMany(kvp => kvp.Value)
                                                    .Where(x => x.Shigh >= x.NextShigh && x.Shigh >= x.PrevShigh)
                                                    .ToList();

                                                var penetrationPointResult = new List<Formula17PenetrationObject>();
                                                foreach (var filteredStockItem in filteredStockResult)
                                                {
                                                    if (recommendationsDict.TryGetValue(filteredStockItem.Sticker, out var relatedItems))
                                                    {
                                                        var matchingItem = relatedItems
                                                            .FirstOrDefault(x => x.DayNo < filteredStockItem.DayNo &&
                                                                                x.Shigh > filteredStockItem.Shigh &&
                                                                                x.DayNo == startDayNo);

                                                        if (matchingItem != null)
                                                        {
                                                            penetrationPointResult.Add(new Formula17PenetrationObject
                                                            {
                                                                LatestHigh = matchingItem.Shigh.HasValue ? matchingItem.Shigh.Value : 0,
                                                                StockItem = filteredStockItem
                                                            });
                                                        }
                                                    }
                                                }

                                                if (formula17.TypeAll && (formula17.GreaterThan != null || formula17.LessThan != null))
                                                {
                                                    var penetrationPointResult1 = penetrationPointResult
                                                        .Where(x => x.StockItem.Shigh != 0)
                                                        .Select(x => new { Item = x, Percent = (x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh * 100 })
                                                        .Where(x => x.Percent >= 0)
                                                        .Where(x => (formula17.GreaterThan == null || x.Percent >= formula17.GreaterThan) &&
                                                                    (formula17.LessThan == null || x.Percent <= formula17.LessThan))
                                                        .Select(x => x.Item);

                                                    var penetrationPointResult2 = penetrationPointResult
                                                        .Where(x => x.StockItem.Shigh != 0)
                                                        .Select(x => new { Item = x, Percent = (x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh * 100 })
                                                        .Where(x => x.Percent <= 0)
                                                        .Where(x => (formula17.GreaterThan == null || -x.Percent >= formula17.GreaterThan) &&
                                                                    (formula17.LessThan == null || -x.Percent <= formula17.LessThan))
                                                        .Select(x => x.Item);

                                                    penetrationPointResult = penetrationPointResult1.Concat(penetrationPointResult2).ToList();
                                                }
                                                else if (formula17.TypeGreater)
                                                {
                                                    penetrationPointResult = penetrationPointResult
                                                        .Where(x => x.StockItem.Shigh != 0)
                                                        .Select(x => new { Item = x, Percent = (x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh * 100 })
                                                        .Where(x => x.Percent >= 0)
                                                        .Where(x => (formula17.GreaterThan == null || x.Percent >= formula17.GreaterThan) &&
                                                                    (formula17.LessThan == null || x.Percent <= formula17.LessThan))
                                                        .Select(x => x.Item)
                                                        .ToList();
                                                }
                                                else if (formula17.TypeLess)
                                                {
                                                    penetrationPointResult = penetrationPointResult
                                                        .Where(x => x.StockItem.Shigh != 0)
                                                        .Select(x => new { Item = x, Percent = (x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh * 100 })
                                                        .Where(x => x.Percent <= 0)
                                                        .Where(x => (formula17.GreaterThan == null || -x.Percent >= formula17.GreaterThan) &&
                                                                    (formula17.LessThan == null || -x.Percent <= formula17.LessThan))
                                                        .Select(x => x.Item)
                                                        .ToList();
                                                }

                                                var finalStickers = penetrationPointResult.Select(x => x.StockItem.Sticker).Distinct().ToList();
                                                stockResult = stockPrevDayViews
                                                    .Where(x => x.Key.DayNo == formulaDayNo && finalStickers.Contains(x.Key.Sticker))
                                                    .Select(x => x.Value)
                                                    .ToList();
                                            }
                                            break;

                                        default:
                                            break;
                                    }
                                }
                            }

                            // تجميع النتائج
                            var stockResultSortedList = new List<StockPrevDayView>();
                            var companiesResult = stockResult.GroupBy(x => x.ParentIndicator).ToList();

                            foreach (var group in companiesResult)
                            {
                                if (!string.IsNullOrEmpty(group.Key))
                                {
                                    var parentIndicator = stockPrevDayViews
                                        .FirstOrDefault(x => x.Key.Sticker == group.Key && x.Key.DayNo == startDayNo && x.Value.ParentIndicator != null)
                                        .Value;

                                    if (parentIndicator != null)
                                        stockResultSortedList.Add(parentIndicator);
                                }

                                if (group.Key == "TASI" && companiesResult.Count > 1)
                                {
                                    var temp = group.Where(indicatorItem => !stockResult.Contains(indicatorItem)).ToList();
                                    stockResultSortedList.AddRange(temp);
                                }
                                else
                                {
                                    stockResultSortedList.AddRange(group);
                                }
                            }

                            int companiesCount = stockResultSortedList.Count(x => !x.IsIndicator);
                            var CompaniesSticer = stockResultSortedList.Where(x => !x.IsIndicator).Select(x => x.Sticker).ToList();
                            if (companiesCount == 0)
                            {
                                companiesCount = stockResultSortedList.Count(x => x.IsIndicator);
                            }

                            oneCriteriaVM.CompaniesCount = companiesCount;
                            oneCriteriaVM.CompaniesSticer = CompaniesSticer;
                            criteriaList.Add(oneCriteriaVM);
                        }
                    });
                }

                model.Criteria = criteriaList.ToList();

                stopwatch.Stop();
                _logger.LogInformation($"LoadData took {stopwatch.ElapsedMilliseconds} ms");

                return model;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"LoadData failed after {stopwatch.ElapsedMilliseconds} ms");
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var criteria = await _context.Criterias.FindAsync(id);
                if (criteria == null)
                {
                    return Json(new { success = false, message = "الإستراتيجية غير موجودة" });
                }

                _context.Criterias.Remove(criteria);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting criteria");
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف" });
            }
        }
    }
}