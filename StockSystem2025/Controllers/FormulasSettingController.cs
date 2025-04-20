using System.Diagnostics;
using System.Drawing.Printing;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models;
using StockSystem2025.ViewModel;

namespace StockSystem2025.Controllers
{
    public class FormulasSettingController : Controller
    {

        private readonly StockdbContext _context;

        public FormulasSettingController(StockdbContext context)
        {
            _context = context;

        }

        public async Task<IActionResult> FormulasSettingIndex()
        {

            return View();
        }


       





        [HttpGet]
        public async Task<IActionResult> LoadData(int page = 1, int pageSize = 10)
        {


            try
            {

                var totalRecords = await _context.Criterias.CountAsync();

                var model = new FormulasSettingViewModel
                {
                    ContentTitle = "عرض التوصيات",
                    Criteria = await _context.Criterias.Include(f => f.Formulas).AsNoTracking().Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(),
                    GeneralIndicators = await _context.StockPrevDayViews.AsNoTracking().Where(x => x.Sticker == "TASI" && x.DayNo == 1).ToListAsync(),
                    Companies = await _context.CompanyTables.ToListAsync(),
                    MinDate = await _context.StockTables.MinAsync(x => x.Createddate),
                    MaxDate = await _context.StockTables.MaxAsync(x => x.Createddate),
                };


                var settings = await _context.Settings.FirstOrDefaultAsync(x => x.Name == "ShowCompaniesCount");
                bool viewCompaniesCount = settings?.Value == "1";

                string? criteriaStartDateString = HttpContext.Session.GetString("CriteriaStartDate");
                DateTime? criteriaStartDate = criteriaStartDateString != null ? DateTime.Parse(criteriaStartDateString) : _context.StockTables.Max(x => x.Createddate); ;



                var criteriaList = new List<CriteriaViewModel>();
                if (viewCompaniesCount && model.Criteria.Any())
                {
                    foreach (var criteriaItem in model.Criteria)
                    {
                        var oneCriteriaVM = new CriteriaViewModel();



                        oneCriteriaVM.Criteria = new Criteria
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
                        } ;
                       


                        ///////////////////

                        int startDayNo = 1;
                        var result = _context.StockTables.Where(x => x.Createddate == model.MaxDate).FirstOrDefault();
                        if (result != null)
                            startDayNo = result.DayNo;

                        List<StockPrevDayView> stockResultSortedList = new List<StockPrevDayView>();


                        if (criteriaItem != null)
                        {
                            Formula1 formula1 = new Formula1();
                            Formula2 formula2 = new Formula2();
                            Formula3 formula3 = new Formula3();
                            Formula4 formula4 = new Formula4();
                            Formula5 formula5 = new Formula5();
                            Formula6 formula6 = new Formula6();
                            Formula7 formula7 = new Formula7();
                            Formula8 formula8 = new Formula8();
                            Formula9 formula9 = new Formula9();
                            Formula10 formula10 = new Formula10();
                            Formula11 formula11 = new Formula11();
                            Formula12 formula12 = new Formula12();
                            Formula13 formula13 = new Formula13();
                            Formula14 formula14 = new Formula14();
                            Formula15 formula15 = new Formula15();
                            Formula16 formula16 = new Formula16();
                            Formula17 formula17 = new Formula17();




                            var baseQuery = _context.StockPrevDayViews
                                .Where(x => x.ParentIndicator != null).ToList();

                            if (criteriaItem.IsIndicator == 0)
                                // Fix for CS0266: Ensure the type conversion is explicit by using .ToList() to convert the IEnumerable to a List.
                                baseQuery = baseQuery.Where(x => x.IsIndicator == false).ToList();
                            else if (criteriaItem.IsIndicator == 1)
                                baseQuery = baseQuery.Where(x => x.IsIndicator == true).ToList();

                            var requiredDays = criteriaItem.Formulas.Select(f => startDayNo + f.Day - 1).Distinct().ToList();
                            baseQuery = baseQuery.Where(x => requiredDays.Contains(x.DayNo)).ToList(); ;

                            var stockPrevDayViews = baseQuery.ToList();

                            var stockResult = stockPrevDayViews
                                .Where(x => x.DayNo == startDayNo)
                                .ToList();

                            var FormulasGrop = criteriaItem.Formulas.OrderByDescending(x => x.Day).GroupBy(x => x.Day);

                            foreach (var GropItem in FormulasGrop)
                            {
                                int FormulaDayNo = startDayNo + GropItem.First().Day - 1;
                                var stockStickers = stockResult.Select(y => y.Sticker).ToHashSet();
                                var res = stockPrevDayViews
                                    .Where(x => x.DayNo == FormulaDayNo && stockStickers.Contains(x.Sticker))
                                    .ToList();


                                stockResult = res;
                                foreach (var FormulaItem in GropItem)
                                {
                                    string[] FormulaValuesArray = FormulaItem.FormulaValues.Split(';');



                                    switch (FormulaItem.FormulaType)
                                    {
                                        case 1:
                                            if (FormulaValuesArray.Length < 6)
                                            {
                                                throw new ArgumentException("FormulaValuesArray does not contain enough elements for Formula1.");
                                            }

                                            formula1 = new Formula1()
                                            {
                                                TypeAll = bool.TryParse(FormulaValuesArray[0], out var typeAll1) ? typeAll1 : throw new FormatException($"Invalid boolean format for TypeAll: '{FormulaValuesArray[0]}'"),
                                                TypePositive = bool.TryParse(FormulaValuesArray[1], out var typePositive1) ? typePositive1 : throw new FormatException($"Invalid boolean format for TypePositive: '{FormulaValuesArray[1]}'"),
                                                TypeNegative = bool.TryParse(FormulaValuesArray[2], out var typeNegative1) ? typeNegative1 : throw new FormatException($"Invalid boolean format for TypeNegative: '{FormulaValuesArray[2]}'"),
                                                TypeNoChange = bool.TryParse(FormulaValuesArray[3], out var typeNoChange1) ? typeNoChange1 : throw new FormatException($"Invalid boolean format for TypeNoChange: '{FormulaValuesArray[3]}'"),
                                                GreaterThan = double.TryParse(FormulaValuesArray[4], out var greaterThan1) ? greaterThan1 : (double?)null,
                                                LessThan = double.TryParse(FormulaValuesArray[5], out var lessThan) ? lessThan : (double?)null
                                            };

                                            Func<StockPrevDayView, double?> getChangePercent = x => x.PrevSclose == 0 ? 0 : ((x.Sclose - x.PrevSclose) / x.PrevSclose * 100);

                                            if (formula1.TypeAll)
                                            {
                                                var stockResultFiltered = stockResult.Where(stock =>
                                                {
                                                    var change = getChangePercent(stock);
                                                    if (change > 0)
                                                    {
                                                        return (formula1.GreaterThan == null || change > formula1.GreaterThan) &&
                                                               (formula1.LessThan == null || change < formula1.LessThan);
                                                    }
                                                    else if (change < 0)
                                                    {
                                                        var negativeChange = -change;
                                                        return (formula1.GreaterThan == null || negativeChange > formula1.GreaterThan) &&
                                                               (formula1.LessThan == null || negativeChange < formula1.LessThan);
                                                    }
                                                    return false;
                                                }).ToList();

                                                stockResult = stockResultFiltered;
                                            }
                                            else
                                            {
                                                stockResult = stockResult.Where(x =>
                                                {
                                                    var change = getChangePercent(x);
                                                    if (formula1.TypePositive && change > 0)
                                                    {
                                                        return (formula1.GreaterThan == null || change > formula1.GreaterThan) &&
                                                               (formula1.LessThan == null || change < formula1.LessThan);
                                                    }

                                                    if (formula1.TypeNegative && change < 0)
                                                    {
                                                        var negativeChange = -change;
                                                        return (formula1.GreaterThan == null || negativeChange > formula1.GreaterThan) &&
                                                               (formula1.LessThan == null || negativeChange < formula1.LessThan);
                                                    }

                                                    if (formula1.TypeNoChange && change == 0)
                                                    {
                                                        return true;
                                                    }

                                                    return false;
                                                }).ToList();
                                            }
                                            break;

                                        case 2:
                                            formula2 = new Formula2()
                                            {
                                                GreaterThan = double.TryParse(FormulaValuesArray[0], out var greaterThan2) ? greaterThan2 : null, // Use 0.0 or another default
                                                LessThan = double.TryParse(FormulaValuesArray[1], out var lessThan2) ? lessThan2 : null // Use 0.0 or another default
                                            };
                                            if (formula2.GreaterThan != null)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Shigh - x.Slow) / x.Sopen * 100) > formula2.GreaterThan).ToList();
                                            }
                                            if (formula2.LessThan != null)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Shigh - x.Slow) / x.Sopen * 100) < formula2.LessThan).ToList();
                                            }

                                            break;

                                        case 3:
                                            formula3 = new Formula3
                                            {
                                                TypeAll = bool.TryParse(FormulaValuesArray[0], out var typeAll) ? typeAll : throw new FormatException($"Invalid boolean format for TypeAll: '{FormulaValuesArray[0]}'"),
                                                TypeHigherGap = bool.TryParse(FormulaValuesArray[1], out var typeHigherGap) ? typeHigherGap : throw new FormatException($"Invalid boolean format for TypeHigherGap: '{FormulaValuesArray[1]}'"),
                                                TypeLowerGap = bool.TryParse(FormulaValuesArray[2], out var typeLowerGap) ? typeLowerGap : throw new FormatException($"Invalid boolean format for TypeLowerGap: '{FormulaValuesArray[2]}'"),
                                                GreaterThan = double.TryParse(FormulaValuesArray[3], out var greaterThan3) ? greaterThan3 : (double?)null,
                                                LessThan = double.TryParse(FormulaValuesArray[4], out var lessThan3) ? lessThan3 : (double?)null
                                            };

                                            bool InRange(double gap)
                                            {
                                                if (formula3.GreaterThan.HasValue && gap <= formula3.GreaterThan.Value)
                                                    return false;
                                                if (formula3.LessThan.HasValue && gap >= formula3.LessThan.Value)
                                                    return false;
                                                return true;
                                            }

                                            var fastList = new List<StockPrevDayView>();

                                            foreach (var x in stockResult)
                                            {
                                                // Gap Up
                                                if (x.Slow.HasValue && x.PrevShigh.HasValue && x.PrevShigh != 0)
                                                {
                                                    double gapUp = ((x.Slow.Value - x.PrevShigh.Value) / x.PrevShigh.Value) * 100;
                                                    if (gapUp >= 0 && InRange(gapUp))
                                                    {
                                                        fastList.Add(x);
                                                        continue;
                                                    }
                                                }

                                                // Gap Down
                                                if ((formula3.TypeAll || formula3.TypeLowerGap) && x.Shigh.HasValue && x.PrevSlow.HasValue && x.PrevSlow != 0)
                                                {
                                                    double gapDown = ((x.Shigh.Value - x.PrevSlow.Value) / x.PrevSlow.Value) * 100;
                                                    if (gapDown <= 0 && InRange(-gapDown))
                                                    {
                                                        fastList.Add(x);
                                                    }
                                                }
                                            }

                                            // Remove duplicates if TypeAll is true
                                            stockResult = formula3.TypeAll
                                                ? fastList.DistinctBy(x => x.Sticker).ToList()
                                                : fastList;

                                            break;


                                        case 4:
                                            if (FormulaValuesArray.Length < 5)
                                            {
                                                throw new ArgumentException("FormulaValuesArray does not contain enough elements for Formula4.");
                                            }

                                            formula4 = new Formula4()
                                            {
                                                TypeAll = bool.TryParse(FormulaValuesArray[0], out var typeAll4) ? typeAll4 : throw new FormatException($"Invalid boolean format for TypeAll: '{FormulaValuesArray[0]}'"),
                                                TypeHigher = bool.TryParse(FormulaValuesArray[1], out var typeHigher) ? typeHigher : throw new FormatException($"Invalid boolean format for TypeHigher: '{FormulaValuesArray[1]}'"),
                                                TypeLower = bool.TryParse(FormulaValuesArray[2], out var typeLower) ? typeLower : throw new FormatException($"Invalid boolean format for TypeLower: '{FormulaValuesArray[2]}'"),
                                                GreaterThan = double.TryParse(FormulaValuesArray[3], out var greaterThan4) ? greaterThan4 : (double?)null,
                                                LessThan = double.TryParse(FormulaValuesArray[4], out var lessThan4) ? lessThan4 : (double?)null
                                            };

                                            if (formula4.TypeAll)
                                            {
                                                if (formula4.GreaterThan != null || formula4.LessThan != null)
                                                {
                                                    var stockResult1 = stockResult
                                                        .Select(x => new { Item = x, HighPercent = (x.Sopen - x.PrevShigh) / x.PrevShigh * 100 })
                                                        .Where(x => x.HighPercent >= 0)
                                                        .Where(x => (formula4.GreaterThan == null || x.HighPercent > formula4.GreaterThan) &&
                                                                    (formula4.LessThan == null || x.HighPercent < formula4.LessThan))
                                                        .Select(x => x.Item);

                                                    var stockResult2 = stockResult
                                                        .Select(x => new { Item = x, LowPercent = (x.Sopen - x.PrevSlow) / x.PrevSlow * 100 })
                                                        .Where(x => x.LowPercent <= 0)
                                                        .Where(x => (formula4.GreaterThan == null || -x.LowPercent > formula4.GreaterThan) &&
                                                                    (formula4.LessThan == null || -x.LowPercent < formula4.LessThan))
                                                        .Select(x => x.Item);

                                                    stockResult = stockResult1.Concat(stockResult2).ToList();
                                                }
                                            }
                                            else
                                            {
                                                if (formula4.TypeHigher)
                                                {
                                                    stockResult = stockResult
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
                                                        .Select(x => new { Item = x, LowPercent = (x.Sopen - x.PrevSlow) / x.PrevSlow * 100 })
                                                        .Where(x => x.LowPercent <= 0)
                                                        .Where(x => (formula4.GreaterThan == null || -x.LowPercent > formula4.GreaterThan) &&
                                                                    (formula4.LessThan == null || -x.LowPercent < formula4.LessThan))
                                                        .Select(x => x.Item)
                                                        .ToList();
                                                }
                                            }
                                            break;

                                        case 5:
                                            formula5 = new Formula5()
                                            {
                                                TypeAll = bool.TryParse(FormulaValuesArray[0], out var typeAll5) ? typeAll5 : false,
                                                TypeHigherGap = bool.TryParse(FormulaValuesArray[1], out var typeHigherGap5) ? typeHigherGap5 : false,
                                                TypeLowerGap = bool.TryParse(FormulaValuesArray[2], out var typeLowerGap5) ? typeLowerGap5 : false
                                            };

                                            if (formula5.TypeLowerGap)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.Sopen <= x.PrevSopen && x.Sopen <= x.PrevSclose && x.Sopen >= x.PrevSlow)
                                                    .ToList();
                                            }
                                            else if (formula5.TypeHigherGap)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.Sopen >= x.PrevSopen && x.Sopen >= x.PrevSclose && x.Sopen <= x.PrevShigh)
                                                    .ToList();
                                            }

                                            break;

                                        case 6:
                                            formula6 = new Formula6()
                                            {
                                                Between = double.TryParse(FormulaValuesArray[0], out var between) ? between : (double?)null,
                                                And = double.TryParse(FormulaValuesArray[1], out var and) ? and : (double?)null
                                            };

                                            if (formula6.Between != null)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Sopen - x.Slow) / (x.Shigh - x.Slow) * 100) >= formula6.Between).ToList();
                                            }
                                            if (formula6.And != null)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Sopen - x.Slow) / (x.Shigh - x.Slow) * 100) <= formula6.And).ToList();
                                            }

                                            break;

                                        case 7:
                                            formula7 = new Formula7()
                                            {
                                                Between = double.TryParse(FormulaValuesArray[0], out var between7) ? between7 : (double?)null,
                                                And = double.TryParse(FormulaValuesArray[1], out var and7) ? and7 : (double?)null
                                            };

                                            if (formula7.Between != null)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Sclose - x.Slow) / (x.Shigh - x.Slow) * 100) >= formula7.Between).ToList();
                                            }
                                            if (formula7.And != null)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Sclose - x.Slow) / (x.Shigh - x.Slow) * 100) <= formula7.And).ToList();
                                            }

                                            break;

                                        case 8:
                                            formula8 = new Formula8()
                                            {
                                                Between = double.TryParse(FormulaValuesArray[0], out var between8) ? between8 : (double?)null,
                                                And = double.TryParse(FormulaValuesArray[1], out var and8) ? and8 : (double?)null
                                            };

                                            if (formula8.Between != null)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Sopen - x.PrevSlow) / (x.PrevShigh - x.PrevSlow) * 100) >= formula8.Between).ToList();
                                            }
                                            if (formula8.And != null)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Sopen - x.PrevSlow) / (x.PrevShigh - x.PrevSlow) * 100) <= formula8.And).ToList();
                                            }

                                            break;

                                        case 9:
                                            formula9 = new Formula9()
                                            {
                                                TypeAll = bool.TryParse(FormulaValuesArray[0], out var typeAll9) ? typeAll9 : throw new FormatException($"Invalid boolean format for TypeAll: '{FormulaValuesArray[0]}'"),
                                                TypePositive = bool.TryParse(FormulaValuesArray[1], out var typePositive9) ? typePositive9 : throw new FormatException($"Invalid boolean format for TypePositive: '{FormulaValuesArray[1]}'"),
                                                TypeNegative = bool.TryParse(FormulaValuesArray[2], out var typeNegative9) ? typeNegative9 : throw new FormatException($"Invalid boolean format for TypeNegative: '{FormulaValuesArray[2]}'"),
                                                GreaterThan = double.TryParse(FormulaValuesArray[3], out var greaterThan9) ? greaterThan9 : (double?)null,
                                                LessThan = double.TryParse(FormulaValuesArray[4], out var lessThan9) ? lessThan9 : (double?)null
                                            };

                                            if (formula9.TypeAll && (formula9.GreaterThan != null || formula9.LessThan != null))
                                            {
                                                var stockResult1 = stockResult
                                                    .Where(x => x.Sopen != 0) // تجنب القسمة على صفر
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
                                            formula10 = new Formula10()
                                            {
                                                Between = double.TryParse(FormulaValuesArray[0], out var between10) ? between10 : (double?)null,
                                                And = double.TryParse(FormulaValuesArray[1], out var and10) ? and10 : (double?)null
                                            };

                                            if (formula10.Between != null)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Sclose - x.PrevSlow) / (x.PrevShigh - x.PrevSlow) * 100) >= formula10.Between).ToList();
                                            }
                                            if (formula10.And != null)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Sclose - x.PrevSlow) / (x.PrevShigh - x.PrevSlow) * 100) <= formula10.And).ToList();
                                            }

                                            break;

                                        case 11:
                                            formula11 = new Formula11()
                                            {
                                                MaximumAll = bool.TryParse(FormulaValuesArray[0], out var maxAll) ? maxAll : throw new FormatException($"Invalid boolean format for MaximumAll: '{FormulaValuesArray[0]}'"),
                                                MaximumGreater = bool.TryParse(FormulaValuesArray[1], out var maxGreater) ? maxGreater : throw new FormatException($"Invalid boolean format for MaximumGreater: '{FormulaValuesArray[1]}'"),
                                                MaximumLess = bool.TryParse(FormulaValuesArray[2], out var maxLess) ? maxLess : throw new FormatException($"Invalid boolean format for MaximumLess: '{FormulaValuesArray[2]}'"),
                                                MaximumBetween = double.TryParse(FormulaValuesArray[3], out var maxBetween) ? maxBetween : (double?)null,
                                                MaximumAnd = double.TryParse(FormulaValuesArray[4], out var maxAnd) ? maxAnd : (double?)null,
                                                MinimumAll = bool.TryParse(FormulaValuesArray[5], out var minAll) ? minAll : throw new FormatException($"Invalid boolean format for MinimumAll: '{FormulaValuesArray[5]}'"),
                                                MinimumGreater = bool.TryParse(FormulaValuesArray[6], out var minGreater) ? minGreater : throw new FormatException($"Invalid boolean format for MinimumGreater: '{FormulaValuesArray[6]}'"),
                                                MinimumLess = bool.TryParse(FormulaValuesArray[7], out var minLess) ? minLess : throw new FormatException($"Invalid boolean format for MinimumLess: '{FormulaValuesArray[7]}'"),
                                                MinimumBetween = double.TryParse(FormulaValuesArray[8], out var minBetween) ? minBetween : (double?)null,
                                                MinimumAnd = double.TryParse(FormulaValuesArray[9], out var minAnd) ? minAnd : (double?)null
                                            };

                                            // معالجة Maximum
                                            if (formula11.MaximumAll && (formula11.MaximumBetween != null || formula11.MaximumAnd != null))
                                            {
                                                var stockResult1 = stockResult
                                                    .Where(x => x.PrevShigh != 0) // تجنب القسمة على صفر
                                                    .Select(x => new { Item = x, Percent = (x.Shigh - x.PrevShigh) / x.PrevShigh * 100 })
                                                    .Where(x => x.Percent >= 0)
                                                    .Where(x => (formula11.MaximumBetween == null || x.Percent >= formula11.MaximumBetween) &&
                                                                (formula11.MaximumAnd == null || x.Percent <= formula11.MaximumAnd))
                                                    .Select(x => x.Item);

                                                var stockResult2 = stockResult
                                                    .Where(x => x.PrevShigh != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Shigh - x.PrevShigh) / x.PrevShigh * 100 })
                                                    .Where(x => x.Percent <= 0)
                                                    .Where(x => (formula11.MaximumBetween == null || -x.Percent >= formula11.MaximumBetween) &&
                                                                (formula11.MaximumAnd == null || -x.Percent <= formula11.MaximumAnd))
                                                    .Select(x => x.Item);

                                                stockResult = stockResult1.Concat(stockResult2).ToList();
                                            }
                                            else if (formula11.MaximumGreater)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevShigh != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Shigh - x.PrevShigh) / x.PrevShigh * 100 })
                                                    .Where(x => x.Percent >= 0)
                                                    .Where(x => (formula11.MaximumBetween == null || x.Percent >= formula11.MaximumBetween) &&
                                                                (formula11.MaximumAnd == null || x.Percent <= formula11.MaximumAnd))
                                                    .Select(x => x.Item)
                                                    .ToList();
                                            }
                                            else if (formula11.MaximumLess)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevShigh != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Shigh - x.PrevShigh) / x.PrevShigh * 100 })
                                                    .Where(x => x.Percent <= 0)
                                                    .Where(x => (formula11.MaximumBetween == null || -x.Percent >= formula11.MaximumBetween) &&
                                                                (formula11.MaximumAnd == null || -x.Percent <= formula11.MaximumAnd))
                                                    .Select(x => x.Item)
                                                    .ToList();
                                            }

                                            // معالجة Minimum
                                            if (formula11.MinimumAll && (formula11.MinimumBetween != null || formula11.MinimumAnd != null))
                                            {
                                                var stockResult1 = stockResult
                                                    .Where(x => x.PrevSlow != 0) // تجنب القسمة على صفر
                                                    .Select(x => new { Item = x, Percent = (x.Slow - x.PrevSlow) / x.PrevSlow * 100 })
                                                    .Where(x => x.Percent >= 0)
                                                    .Where(x => (formula11.MinimumBetween == null || x.Percent >= formula11.MinimumBetween) &&
                                                                (formula11.MinimumAnd == null || x.Percent <= formula11.MinimumAnd))
                                                    .Select(x => x.Item);

                                                var stockResult2 = stockResult
                                                    .Where(x => x.PrevSlow != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Slow - x.PrevSlow) / x.PrevSlow * 100 })
                                                    .Where(x => x.Percent <= 0)
                                                    .Where(x => (formula11.MinimumBetween == null || -x.Percent >= formula11.MinimumBetween) &&
                                                                (formula11.MinimumAnd == null || -x.Percent <= formula11.MinimumAnd))
                                                    .Select(x => x.Item);

                                                stockResult = stockResult1.Concat(stockResult2).ToList();
                                            }
                                            else if (formula11.MinimumGreater)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevSlow != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Slow - x.PrevSlow) / x.PrevSlow * 100 })
                                                    .Where(x => x.Percent >= 0)
                                                    .Where(x => (formula11.MinimumBetween == null || x.Percent >= formula11.MinimumBetween) &&
                                                                (formula11.MinimumAnd == null || x.Percent <= formula11.MinimumAnd))
                                                    .Select(x => x.Item)
                                                    .ToList();
                                            }
                                            else if (formula11.MinimumLess)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevSlow != 0)
                                                    .Select(x => new { Item = x, Percent = (x.Slow - x.PrevSlow) / x.PrevSlow * 100 })
                                                    .Where(x => x.Percent <= 0)
                                                    .Where(x => (formula11.MinimumBetween == null || -x.Percent >= formula11.MinimumBetween) &&
                                                                (formula11.MinimumAnd == null || -x.Percent <= formula11.MinimumAnd))
                                                    .Select(x => x.Item)
                                                    .ToList();
                                            }

                                            break;

                                        case 12:
                                            formula12 = new Formula12()
                                            {
                                                TypeAll = bool.TryParse(FormulaValuesArray[0], out var typeAll12) ? typeAll12 : throw new FormatException($"Invalid boolean format for TypeAll: '{FormulaValuesArray[0]}'"),
                                                TypeGreater = bool.TryParse(FormulaValuesArray[1], out var typeGreater) ? typeGreater : throw new FormatException($"Invalid boolean format for TypeGreater: '{FormulaValuesArray[1]}'"),
                                                TypeLess = bool.TryParse(FormulaValuesArray[2], out var typeLess) ? typeLess : throw new FormatException($"Invalid boolean format for TypeLess: '{FormulaValuesArray[2]}'"),
                                                GreaterThan = double.TryParse(FormulaValuesArray[3], out var greaterThan12) ? greaterThan12 : (double?)null,
                                                LessThan = double.TryParse(FormulaValuesArray[4], out var lessThan12) ? lessThan12 : (double?)null
                                            };

                                            if (formula12.TypeAll && (formula12.GreaterThan != null || formula12.LessThan != null))
                                            {
                                                var stockResult1 = stockResult
                                                    .Where(x => x.PrevSvol != 0) // تجنب القسمة على صفر
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
                                            formula13 = new Formula13()
                                            {
                                                TypeAll = bool.TryParse(FormulaValuesArray[0], out var typeAll13) ? typeAll13 : throw new FormatException($"Invalid boolean format for TypeAll: '{FormulaValuesArray[0]}'"),
                                                TypePositive = bool.TryParse(FormulaValuesArray[1], out var typePositive) ? typePositive : throw new FormatException($"Invalid boolean format for TypePositive: '{FormulaValuesArray[1]}'"),
                                                TypeNegative = bool.TryParse(FormulaValuesArray[2], out var typeNegative) ? typeNegative : throw new FormatException($"Invalid boolean format for TypeNegative: '{FormulaValuesArray[2]}'"),
                                                Days = int.TryParse(FormulaValuesArray[3], out var days) ? days : (int?)null,
                                                GreaterThan = double.TryParse(FormulaValuesArray[4], out var greaterThan13) ? greaterThan13 : (double?)null,
                                                LessThan = double.TryParse(FormulaValuesArray[5], out var lessThan13) ? lessThan13 : (double?)null
                                            };

                                            if (formula13.Days != null && formula13.Days.Value > 0)
                                            {
                                                // تحسين استعلام قاعدة البيانات وتحويله إلى قاموس
                                                var midScloseDict = _context.StockPrevDayViews
                                                    .AsNoTracking()
                                                    .Where(x => x.DayNo >= FormulaDayNo)
                                                    .GroupBy(x => x.Sticker)
                                                    .Select(g => new
                                                    {
                                                        Sticker = g.Key,
                                                        SumSclose = g.OrderBy(x => x.DayNo).Take(formula13.Days.Value).Sum(x => x.PrevSclose) / formula13.Days.Value
                                                    })
                                                    .ToDictionary(x => x.Sticker, x => x.SumSclose);

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
                                            formula14 = new Formula14()
                                            {
                                                TypeAll = bool.TryParse(FormulaValuesArray[0], out var typeAll14) ? typeAll14 : throw new FormatException($"Invalid boolean format for TypeAll: '{FormulaValuesArray[0]}'"),
                                                TypeGreater = bool.TryParse(FormulaValuesArray[1], out var typeGreater14) ? typeGreater14 : throw new FormatException($"Invalid boolean format for TypeGreater: '{FormulaValuesArray[1]}'"),
                                                TypeLess = bool.TryParse(FormulaValuesArray[2], out var typeLess14) ? typeLess14 : throw new FormatException($"Invalid boolean format for TypeLess: '{FormulaValuesArray[2]}'"),
                                                GreaterThan = double.TryParse(FormulaValuesArray[3], out var greaterThan14) ? greaterThan14 : (double?)null,
                                                LessThan = double.TryParse(FormulaValuesArray[4], out var lessThan14) ? lessThan14 : (double?)null
                                            };

                                            if (formula14.TypeAll && (formula14.GreaterThan != null || formula14.LessThan != null))
                                            {
                                                var stockResult1 = stockResult
                                                    .Where(x => (x.Sclose + x.Slow + x.Shigh) != 0) // تجنب القسمة على صفر
                                                    .Select(x => new
                                                    {
                                                        Item = x,
                                                        Avg = (x.Sclose + x.Slow + x.Shigh) / 3.0, // استخدام 3.0 لضمان القسمة كـ double
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
                                            formula15 = new Formula15()
                                            {
                                                Between = double.TryParse(FormulaValuesArray[0], out var between15) ? between15 : (double?)null,
                                                And = double.TryParse(FormulaValuesArray[1], out var and15) ? and15 : (double?)null
                                            };

                                            if (formula15.Between != null || formula15.And != null)
                                            {
                                                stockResult = stockResult
                                                    .Where(x => x.PrevShigh != x.PrevSlow) // تجنب القسمة على صفر
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
                                            formula17 = new Formula17()
                                            {
                                                TypeAll = bool.TryParse(FormulaValuesArray[0], out var typeAll17) ? typeAll17 : throw new FormatException($"Invalid boolean format for TypeAll: '{FormulaValuesArray[0]}'"),
                                                TypeGreater = bool.TryParse(FormulaValuesArray[1], out var typeGreater17) ? typeGreater17 : throw new FormatException($"Invalid boolean format for TypeGreater: '{FormulaValuesArray[1]}'"),
                                                TypeLess = bool.TryParse(FormulaValuesArray[2], out var typeLess17) ? typeLess17 : throw new FormatException($"Invalid boolean format for TypeLess: '{FormulaValuesArray[2]}'"),
                                                FromDays = int.TryParse(FormulaValuesArray[3], out var fromDays) ? fromDays : (int?)null,
                                                ToDays = int.TryParse(FormulaValuesArray[4], out var toDays) ? toDays : (int?)null,
                                                GreaterThan = double.TryParse(FormulaValuesArray[5], out var greaterThan17) ? greaterThan17 : (double?)null,
                                                LessThan = double.TryParse(FormulaValuesArray[6], out var lessThan17) ? lessThan17 : (double?)null
                                            };

                                            if (formula17.FromDays != null && formula17.FromDays.Value > 0 && formula17.ToDays != null && formula17.ToDays.Value > 0)
                                            {
                                                var listOfCodes = stockResult.Select(y => y.Sticker).Distinct().ToList();
                                                int relatedDayNo = startDayNo - 1;
                                                int totalFromDayNo = formula17.FromDays.Value + relatedDayNo;
                                                int totalToDayNo = formula17.ToDays.Value + relatedDayNo;

                                                var tmp = _context.RecommendationsResultsViews
                                                         .AsNoTracking()
                                                         .Where(x =>
                                                             x.DayNo >= totalFromDayNo &&
                                                             x.DayNo <= totalToDayNo &&
                                                             x.Shigh >= x.NextShigh &&
                                                             x.Shigh >= x.PrevShigh
                                                         )
                                                         .ToList();

                                                // 2) فلترة في الذاكرة على listOfCodes
                                                var filteredStockResult = tmp
                                                    .Where(x => listOfCodes.Contains(x.Sticker))
                                                    .ToList();




                                                // تحميل جميع السجلات ذات الصلة مسبقًا
                                                var tmpResults = _context.RecommendationsResultsViews
                                                      .AsNoTracking()
                                                      .Where(x => x.DayNo < totalToDayNo)
                                                      .ToList();

                                                // المرحلة الثانية: فلترة القائمة في الذاكرة حسب listOfCodes ووجود SHigh، ثم تجميعها في Dictionary
                                                var relatedResults = tmpResults
                                                    .Where(x => listOfCodes.Contains(x.Sticker) && x.Shigh != null)
                                                    .GroupBy(x => x.Sticker)
                                                    .ToDictionary(
                                                        g => g.Key,
                                                        g => g.ToList()
                                                    );

                                                // تخصيص سعة أولية لتقليل إعادة تخصيص الذاكرة
                                                var penetrationPointResult = new List<Formula17PenetrationObject>(filteredStockResult.Count);

                                                // استخدام Parallel.ForEach مع قفل
                                                var lockObject = new object();
                                                Parallel.ForEach(filteredStockResult, filteredStockItem =>
                                                {
                                                    if (relatedResults.TryGetValue(filteredStockItem.Sticker, out var relatedItems))
                                                    {
                                                        var matchingItem = relatedItems
                                                            .SingleOrDefault(x => x.DayNo < filteredStockItem.DayNo &&
                                                                                 x.Shigh > filteredStockItem.Shigh &&
                                                                                 x.DayNo == startDayNo);

                                                        if (matchingItem != null)
                                                        {
                                                            var penetrationPointItem = new Formula17PenetrationObject
                                                            {
                                                                LatestHigh = matchingItem.Shigh.HasValue ? matchingItem.Shigh.Value : 0,
                                                                StockItem = filteredStockItem
                                                            };

                                                            lock (lockObject)
                                                            {
                                                                penetrationPointResult.Add(penetrationPointItem);
                                                            }
                                                        }
                                                    }
                                                });

                                                // معالجة PenetrationPointResult بناءً على TypeAll, TypeGreater, TypeLess
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

                                                // تحسين التصفية النهائية
                                                var finalStickers = penetrationPointResult.Select(x => x.StockItem.Sticker).Distinct().ToList();
                                                var newStockResult = _context.StockPrevDayViews
                                                    .AsNoTracking()
                                                    .Where(x => x.DayNo == FormulaDayNo)
                                                    .ToList();
                                                newStockResult = newStockResult.Where(x => finalStickers.Contains(x.Sticker)).ToList();

                                                stockResult = newStockResult;
                                            }

                                            break;

                                        default:
                                            break;
                                    }


                                }

                            }


                            var companiesResult = stockResult.GroupBy(x => x.ParentIndicator).ToList();

                            foreach (var group in companiesResult)
                            {
                               

                                if (!string.IsNullOrEmpty(group.Key))
                                {
                                    var parentIndicator = _context.StockPrevDayViews.AsNoTracking()
                                        .FirstOrDefault(x => x.Sticker == group.Key && x.DayNo == startDayNo && x.ParentIndicator != null);

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

                            int CompaniesCount = stockResultSortedList.Count(x => !x.IsIndicator);
                            if (CompaniesCount == 0)
                            {
                                CompaniesCount = stockResultSortedList.Count(x => x.IsIndicator);
                            }

                            oneCriteriaVM.CompaniesCount = CompaniesCount;

                    

                            criteriaList.Add(oneCriteriaVM);

                        }

                    }
                }

                // إرجاع البيانات كـ JSON
                return Json(new
                {
                    success = true,
                    data = criteriaList, // List<CriteriaViewModel>
                    page,
                    pageSize,
                    totalRecords
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        public static class ConvertHelper
        {
            public static int? ToNullableInt(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                    return null;

                return int.TryParse(input, out int result) ? result : (int?)null;
            }
        }













    }
}


