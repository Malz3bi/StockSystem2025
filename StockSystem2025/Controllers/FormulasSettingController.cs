using System.Diagnostics;
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

        public async Task<IActionResult> Index()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoadData(int id)
        {
            var model = new FormulasSettingViewModel
            {
                ContentTitle = "عرض التوصيات",
                Criteria = await _context.Criterias.ToListAsync(),
                GeneralIndicators = await _context.StockPrevDayViews.AsNoTracking().Where(x => x.Sticker == "TASI" && x.DayNo == 1).ToListAsync(),
                Companies = await _context.CompanyTables.ToListAsync(),
                MinDate = await _context.StockTables.MinAsync(x => x.Createddate),
                MaxDate = await _context.StockTables.MaxAsync(x => x.Createddate),
            };

            var settings = await _context.Settings.FirstOrDefaultAsync(x => x.Name == "ShowCompaniesCount");
            bool viewCompaniesCount = settings?.Value == "1";

            string? criteriaStartDateString = HttpContext.Session.GetString("CriteriaStartDate");
            DateTime? criteriaStartDate = criteriaStartDateString != null ? DateTime.Parse(criteriaStartDateString) : null;

            var criteriaResult = _context.Criterias.ToList();

            var CriteriaViewMode = new List<CriteriaViewModel>();
            if (viewCompaniesCount && criteriaResult.Any())
            {
                foreach (var criteriaItem in criteriaResult)
                {
                    var oneCriteriaVM = new CriteriaViewModel();
                    oneCriteriaVM.Criteria = criteriaItem;


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




                        var baseQuery = _context.StockPrevDayViews.AsNoTracking()
                            .Where(x => x.ParentIndicator != null);

                        if (criteriaItem.IsIndicator == 0)
                            baseQuery = baseQuery.Where(x => x.IsIndicator == false);
                        else if (criteriaItem.IsIndicator == 1)
                            baseQuery = baseQuery.Where(x => x.IsIndicator == true);

                        var requiredDays = criteriaItem.Formulas.Select(f => startDayNo + f.Day - 1).Distinct().ToList();
                        baseQuery = baseQuery.Where(x => requiredDays.Contains(x.DayNo));

                        var stockPrevDayViews = await baseQuery.ToListAsync();

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


                                #region fomulas                            
                                switch (FormulaItem.FormulaType)
                                {
                                    case 1:
                                        formula1 = new Formula1()
                                        {
                                            TypeAll = Convert.ToBoolean(FormulaValuesArray[0]),
                                            TypePositive = Convert.ToBoolean(FormulaValuesArray[1]),
                                            TypeNegative = Convert.ToBoolean(FormulaValuesArray[2]),
                                            TypeNoChange = Convert.ToBoolean(FormulaValuesArray[3]),
                                            GreaterThan = Convert.ToDouble(FormulaValuesArray[4]),
                                            LessThan = Convert.ToDouble(FormulaValuesArray[5])
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
                                            GreaterThan = Convert.ToDouble(FormulaValuesArray[0]),
                                            LessThan = Convert.ToDouble(FormulaValuesArray[1])
                                        };

                                        stockResult = stockResult.Where(x =>
                                        {
                                            if (x.Sopen == 0) return false;

                                            var volatility = (x.Shigh - x.Slow) / x.Sopen * 100;
                                            bool isValid = true;
                                            if (formula2.GreaterThan != null)
                                                isValid &= volatility > formula2.GreaterThan;

                                            if (formula2.LessThan != null)
                                                isValid &= volatility < formula2.LessThan;

                                            return isValid;
                                        }).ToList();

                                        break;

                                    case 3:
                                        formula3 = new Formula3
                                        {
                                            TypeAll = Convert.ToBoolean(FormulaValuesArray[0]),
                                            TypeHigherGap = Convert.ToBoolean(FormulaValuesArray[1]),
                                            TypeLowerGap = Convert.ToBoolean(FormulaValuesArray[2]),
                                            GreaterThan = Convert.ToDouble(FormulaValuesArray[3]),
                                            LessThan = Convert.ToDouble(FormulaValuesArray[4])
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
                                        formula4 = new Formula4()
                                        {
                                            TypeAll = Convert.ToBoolean(FormulaValuesArray[0]),
                                            TypeHigher = Convert.ToBoolean(FormulaValuesArray[1]),
                                            TypeLower = Convert.ToBoolean(FormulaValuesArray[2]),
                                            GreaterThan = Convert.ToDouble(FormulaValuesArray[3]),
                                            LessThan = Convert.ToDouble(FormulaValuesArray[4])
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
                                            TypeAll = Convert.ToBoolean(FormulaValuesArray[0]),
                                            TypeHigherGap = Convert.ToBoolean(FormulaValuesArray[1]),
                                            TypeLowerGap = Convert.ToBoolean(FormulaValuesArray[2])
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
                                            Between = Convert.ToDouble(FormulaValuesArray[0]),
                                            And = Convert.ToDouble(FormulaValuesArray[1])
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
                                            Between = Convert.ToDouble(FormulaValuesArray[0]),
                                            And = Convert.ToDouble(FormulaValuesArray[1])
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
                                            Between = Convert.ToDouble(FormulaValuesArray[0]),
                                            And = Convert.ToDouble(FormulaValuesArray[1])
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
                                            TypeAll = Convert.ToBoolean(FormulaValuesArray[0]),
                                            TypePositive = Convert.ToBoolean(FormulaValuesArray[1]),
                                            TypeNegative = Convert.ToBoolean(FormulaValuesArray[2]),
                                            GreaterThan = Convert.ToDouble(FormulaValuesArray[3]),
                                            LessThan = Convert.ToDouble(FormulaValuesArray[4])
                                        };

                                        if (formula9.TypeAll)
                                        {
                                            if (formula9.GreaterThan != null || formula9.LessThan != null)
                                            {
                                                var stockResult1 = stockResult.Where(x => ((x.Sclose - x.Sopen) / x.Sopen * 100) > 0).ToList();
                                                if (formula9.GreaterThan != null)
                                                {
                                                    stockResult1 = stockResult1.Where(x => ((x.Sclose - x.Sopen) / x.Sopen * 100) > formula9.GreaterThan).ToList();
                                                }
                                                if (formula9.LessThan != null)
                                                {
                                                    stockResult1 = stockResult1.Where(x => ((x.Sclose - x.Sopen) / x.Sopen * 100) < formula9.LessThan).ToList();
                                                }

                                                var stockResult2 = stockResult.Where(x => ((x.Sclose - x.Sopen) / x.Sopen * 100) < 0).ToList();
                                                if (formula9.GreaterThan != null)
                                                {
                                                    stockResult2 = stockResult2.Where(x => -((x.Sclose - x.Sopen) / x.Sopen * 100) > formula9.GreaterThan).ToList();
                                                }
                                                if (formula9.LessThan != null)
                                                {
                                                    stockResult2 = stockResult2.Where(x => -((x.Sclose - x.Sopen) / x.Sopen * 100) < formula9.LessThan).ToList();
                                                }
                                                stockResult1.AddRange(stockResult2);
                                                stockResult = stockResult1;
                                            }
                                        }
                                        else
                                        {
                                            if (formula9.TypePositive)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Sclose - x.Sopen) / x.Sopen * 100) > 0).ToList();
                                                if (formula9.GreaterThan != null)
                                                {
                                                    stockResult = stockResult.Where(x => ((x.Sclose - x.Sopen) / x.Sopen * 100) > formula9.GreaterThan).ToList();
                                                }
                                                if (formula9.LessThan != null)
                                                {
                                                    stockResult = stockResult.Where(x => ((x.Sclose - x.Sopen) / x.Sopen * 100) < formula9.LessThan).ToList();
                                                }
                                            }
                                            else if (formula9.TypeNegative)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Sclose - x.Sopen) / x.Sopen * 100) < 0).ToList();
                                                if (formula9.GreaterThan != null)
                                                {
                                                    stockResult = stockResult.Where(x => -((x.Sclose - x.Sopen) / x.Sopen * 100) > formula9.GreaterThan).ToList();
                                                }
                                                if (formula9.LessThan != null)
                                                {
                                                    stockResult = stockResult.Where(x => -((x.Sclose - x.Sopen) / x.Sopen * 100) < formula9.LessThan).ToList();
                                                }
                                            }
                                        }

                                        break;

                                    case 10:
                                        formula10 = new Formula10()
                                        {
                                            Between = Convert.ToDouble(FormulaValuesArray[0]),
                                            And = Convert.ToDouble(FormulaValuesArray[1])
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
                                            MaximumAll = Convert.ToBoolean(FormulaValuesArray[0]),
                                            MaximumGreater = Convert.ToBoolean(FormulaValuesArray[1]),
                                            MaximumLess = Convert.ToBoolean(FormulaValuesArray[2]),
                                            MaximumBetween = Convert.ToDouble(FormulaValuesArray[3]),
                                            MaximumAnd = Convert.ToDouble(FormulaValuesArray[4]),
                                            MinimumAll = Convert.ToBoolean(FormulaValuesArray[5]),
                                            MinimumGreater = Convert.ToBoolean(FormulaValuesArray[6]),
                                            MinimumLess = Convert.ToBoolean(FormulaValuesArray[7]),
                                            MinimumBetween = Convert.ToDouble(FormulaValuesArray[8]),
                                            MinimumAnd = Convert.ToDouble(FormulaValuesArray[9])
                                        };

                                        if (formula11.MaximumAll)
                                        {
                                            if (formula11.MaximumBetween != null || formula11.MaximumAnd != null)
                                            {
                                                var stockResult1 = stockResult.Where(x => ((x.Shigh - x.PrevShigh) / x.PrevShigh * 100) >= 0).ToList();
                                                if (formula11.MaximumBetween != null)
                                                {
                                                    stockResult1 = stockResult1.Where(x => ((x.Shigh - x.PrevShigh) / x.PrevShigh * 100) >= formula11.MaximumBetween).ToList();
                                                }
                                                if (formula11.MaximumAnd != null)
                                                {
                                                    stockResult1 = stockResult1.Where(x => ((x.Shigh - x.PrevShigh) / x.PrevShigh * 100) <= formula11.MaximumAnd).ToList();
                                                }

                                                var stockResult2 = stockResult.Where(x => ((x.Shigh - x.PrevShigh) / x.PrevShigh * 100) <= 0).ToList();
                                                if (formula11.MaximumBetween != null)
                                                {
                                                    stockResult2 = stockResult2.Where(x => -((x.Shigh - x.PrevShigh) / x.PrevShigh * 100) >= formula11.MaximumBetween).ToList();
                                                }
                                                if (formula11.MaximumAnd != null)
                                                {
                                                    stockResult2 = stockResult2.Where(x => -((x.Shigh - x.PrevShigh) / x.PrevShigh * 100) <= formula11.MaximumAnd).ToList();
                                                }
                                                stockResult1.AddRange(stockResult2);
                                                stockResult = stockResult1;
                                            }
                                        }
                                        else
                                        {
                                            if (formula11.MaximumGreater)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Shigh - x.PrevShigh) / x.PrevShigh * 100) >= 0).ToList();
                                                if (formula11.MaximumBetween != null)
                                                {
                                                    stockResult = stockResult.Where(x => ((x.Shigh - x.PrevShigh) / x.PrevShigh * 100) >= formula11.MaximumBetween).ToList();
                                                }
                                                if (formula11.MaximumAnd != null)
                                                {
                                                    stockResult = stockResult.Where(x => ((x.Shigh - x.PrevShigh) / x.PrevShigh * 100) <= formula11.MaximumAnd).ToList();
                                                }
                                            }
                                            else if (formula11.MaximumLess)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Shigh - x.PrevShigh) / x.PrevShigh * 100) <= 0).ToList();
                                                if (formula11.MaximumBetween != null)
                                                {
                                                    stockResult = stockResult.Where(x => -((x.Shigh - x.PrevShigh) / x.PrevShigh * 100) >= formula11.MaximumBetween).ToList();
                                                }
                                                if (formula11.MaximumAnd != null)
                                                {
                                                    stockResult = stockResult.Where(x => -((x.Shigh - x.PrevShigh) / x.PrevShigh * 100) <= formula11.MaximumAnd).ToList();
                                                }
                                            }
                                        }

                                        if (formula11.MinimumAll)
                                        {
                                            if (formula11.MinimumBetween != null || formula11.MinimumAnd != null)
                                            {
                                                var stockResult1 = stockResult.Where(x => ((x.Slow - x.PrevSlow) / x.PrevSlow * 100) >= 0).ToList();
                                                if (formula11.MinimumBetween != null)
                                                {
                                                    stockResult1 = stockResult1.Where(x => ((x.Slow - x.PrevSlow) / x.PrevSlow * 100) >= formula11.MinimumBetween).ToList();
                                                }
                                                if (formula11.MinimumAnd != null)
                                                {
                                                    stockResult1 = stockResult1.Where(x => ((x.Slow - x.PrevSlow) / x.PrevSlow * 100) <= formula11.MinimumAnd).ToList();
                                                }

                                                var stockResult2 = stockResult.Where(x => ((x.Slow - x.PrevSlow) / x.PrevSlow * 100) <= 0).ToList();
                                                if (formula11.MinimumBetween != null)
                                                {
                                                    stockResult2 = stockResult2.Where(x => -((x.Slow - x.PrevSlow) / x.PrevSlow * 100) >= formula11.MinimumBetween).ToList();
                                                }
                                                if (formula11.MinimumAnd != null)
                                                {
                                                    stockResult2 = stockResult2.Where(x => -((x.Slow - x.PrevSlow) / x.PrevSlow * 100) <= formula11.MinimumAnd).ToList();
                                                }
                                                stockResult1.AddRange(stockResult2);
                                                stockResult = stockResult1;
                                            }
                                        }
                                        else
                                        {
                                            if (formula11.MinimumGreater)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Slow - x.PrevSlow) / x.PrevSlow * 100) >= 0).ToList();
                                                if (formula11.MinimumBetween != null)
                                                {
                                                    stockResult = stockResult.Where(x => ((x.Slow - x.PrevSlow) / x.PrevSlow * 100) >= formula11.MinimumBetween).ToList();
                                                }
                                                if (formula11.MinimumAnd != null)
                                                {
                                                    stockResult = stockResult.Where(x => ((x.Slow - x.PrevSlow) / x.PrevSlow * 100) <= formula11.MinimumAnd).ToList();
                                                }
                                            }
                                            else if (formula11.MinimumLess)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Slow - x.PrevSlow) / x.PrevSlow * 100) <= 0).ToList();
                                                if (formula11.MinimumBetween != null)
                                                {
                                                    stockResult = stockResult.Where(x => -((x.Slow - x.PrevSlow) / x.PrevSlow * 100) >= formula11.MinimumBetween).ToList();
                                                }
                                                if (formula11.MinimumAnd != null)
                                                {
                                                    stockResult = stockResult.Where(x => -((x.Slow - x.PrevSlow) / x.PrevSlow * 100) <= formula11.MinimumAnd).ToList();
                                                }
                                            }
                                        }

                                        break;

                                    case 12:
                                        formula12 = new Formula12()
                                        {
                                            TypeAll = Convert.ToBoolean(FormulaValuesArray[0]),
                                            TypeGreater = Convert.ToBoolean(FormulaValuesArray[1]),
                                            TypeLess = Convert.ToBoolean(FormulaValuesArray[2]),
                                            GreaterThan = Convert.ToDouble(FormulaValuesArray[3]),
                                            LessThan = Convert.ToDouble(FormulaValuesArray[4])
                                        };

                                        if (formula12.TypeAll)
                                        {
                                            if (formula12.GreaterThan != null || formula12.LessThan != null) { }
                                            {
                                                var stockResult1 = stockResult.Where(x => ((x.Svol - x.PrevSvol) / x.PrevSvol * 100) > 0).ToList();
                                                if (formula12.GreaterThan != null)
                                                {
                                                    stockResult1 = stockResult1.Where(x => ((x.Svol - x.PrevSvol) / x.PrevSvol * 100) > formula12.GreaterThan).ToList();
                                                }
                                                if (formula12.LessThan != null)
                                                {
                                                    stockResult1 = stockResult1.Where(x => ((x.Svol - x.PrevSvol) / x.PrevSvol * 100) < formula12.LessThan).ToList();
                                                }

                                                var stockResult2 = stockResult.Where(x => ((x.Svol - x.PrevSvol) / x.PrevSvol * 100) < 0).ToList();
                                                if (formula12.GreaterThan != null)
                                                {
                                                    stockResult2 = stockResult2.Where(x => -((x.Svol - x.PrevSvol) / x.PrevSvol * 100) > formula12.GreaterThan).ToList();
                                                }
                                                if (formula12.LessThan != null)
                                                {
                                                    stockResult2 = stockResult2.Where(x => -((x.Svol - x.PrevSvol) / x.PrevSvol * 100) < formula12.LessThan).ToList();
                                                }
                                                stockResult1.AddRange(stockResult2);
                                                stockResult = stockResult1;
                                            }
                                        }
                                        else
                                        {
                                            if (formula12.TypeGreater)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Svol - x.PrevSvol) / x.PrevSvol * 100) > 0).ToList();
                                                if (formula12.GreaterThan != null)
                                                {
                                                    stockResult = stockResult.Where(x => ((x.Svol - x.PrevSvol) / x.PrevSvol * 100) > formula12.GreaterThan).ToList();
                                                }
                                                if (formula12.LessThan != null)
                                                {
                                                    stockResult = stockResult.Where(x => ((x.Svol - x.PrevSvol) / x.PrevSvol * 100) < formula12.LessThan).ToList();
                                                }
                                            }
                                            else if (formula12.TypeLess)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Svol - x.PrevSvol) / x.PrevSvol * 100) < 0).ToList();
                                                if (formula12.GreaterThan != null)
                                                {
                                                    stockResult = stockResult.Where(x => -((x.Svol - x.PrevSvol) / x.PrevSvol * 100) > formula12.GreaterThan).ToList();
                                                }
                                                if (formula12.LessThan != null)
                                                {
                                                    stockResult = stockResult.Where(x => -((x.Svol - x.PrevSvol) / x.PrevSvol * 100) < formula12.LessThan).ToList();
                                                }
                                            }
                                        }

                                        break;

                                    case 13:
                                        formula13 = new Formula13()
                                        {
                                            TypeAll = Convert.ToBoolean(FormulaValuesArray[0]),
                                            TypePositive = Convert.ToBoolean(FormulaValuesArray[1]),
                                            TypeNegative = Convert.ToBoolean(FormulaValuesArray[2]),
                                            Days = Convert.ToNullableInt(FormulaValuesArray[3]),
                                            GreaterThan = Convert.ToDouble(FormulaValuesArray[4]),
                                            LessThan = Convert.ToDouble(FormulaValuesArray[5])
                                        };

                                        if (formula13.Days != null && formula13.Days.Value > 0)
                                        {
                                            // MRamadan remove .ToList() before .GroupBy(x => x.Sticker)
                                            var midSclose = db.StockPrevDayViews.AsNoTracking().Where(x => x.DayNo >= FormulaDayNo).GroupBy(x => x.Sticker).Select(g => new
                                            {
                                                sticker = g.Key,
                                                sumSclose = g.OrderBy(x => x.DayNo).Take(formula13.Days.Value).Sum(x => x.PrevSclose) / formula13.Days.Value
                                            });

                                            if (formula13.TypeAll)
                                            {
                                                if (formula13.GreaterThan != null || formula13.LessThan != null)
                                                {
                                                    var stockResult1 = stockResult.Where(x => ((x.Sclose - (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose)) / (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose) * 100) >= 0).ToList();
                                                    if (formula13.GreaterThan != null)
                                                    {
                                                        stockResult1 = stockResult1.Where(x => ((x.Sclose - (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose)) / (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose) * 100) >= formula13.GreaterThan).ToList();
                                                    }
                                                    if (formula13.LessThan != null)
                                                    {
                                                        stockResult1 = stockResult1.Where(x => ((x.Sclose - (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose)) / (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose) * 100) <= formula13.LessThan).ToList();
                                                    }

                                                    var stockResult2 = stockResult.Where(x => ((x.Sclose - (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose)) / (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose) * 100) <= 0).ToList();
                                                    if (formula13.GreaterThan != null)
                                                    {
                                                        stockResult2 = stockResult2.Where(x => -((x.Sclose - (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose)) / (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose) * 100) >= formula13.GreaterThan).ToList();
                                                    }
                                                    if (formula13.LessThan != null)
                                                    {
                                                        stockResult2 = stockResult2.Where(x => -((x.Sclose - (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose)) / (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose) * 100) <= formula13.LessThan).ToList();
                                                    }
                                                    stockResult1.AddRange(stockResult2);
                                                    stockResult = stockResult1;
                                                }
                                            }
                                            else
                                            {
                                                if (formula13.TypePositive)
                                                {
                                                    stockResult = stockResult.Where(x => ((x.Sclose - (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose)) / (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose) * 100) >= 0).ToList();
                                                    if (formula13.GreaterThan != null)
                                                    {
                                                        stockResult = stockResult.Where(x => ((x.Sclose - (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose)) / (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose) * 100) >= formula13.GreaterThan).ToList();
                                                    }
                                                    if (formula13.LessThan != null)
                                                    {
                                                        stockResult = stockResult.Where(x => ((x.Sclose - (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose)) / (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose) * 100) <= formula13.LessThan).ToList();
                                                    }
                                                }
                                                else if (formula13.TypeNegative)
                                                {
                                                    stockResult = stockResult.Where(x => ((x.Sclose - (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose)) / (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose) * 100) <= 0).ToList();
                                                    if (formula13.GreaterThan != null)
                                                    {
                                                        stockResult = stockResult.Where(x => -((x.Sclose - (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose)) / (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose) * 100) >= formula13.GreaterThan).ToList();
                                                    }
                                                    if (formula13.LessThan != null)
                                                    {
                                                        stockResult = stockResult.Where(x => -((x.Sclose - (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose)) / (midSclose.Where(y => y.sticker == x.Sticker).First().sumSclose) * 100) <= formula13.LessThan).ToList();
                                                    }
                                                }
                                            }
                                        }

                                        break;

                                    case 14:
                                        formula14 = new Formula14()
                                        {
                                            TypeAll = Convert.ToBoolean(FormulaValuesArray[0]),
                                            TypeGreater = Convert.ToBoolean(FormulaValuesArray[1]),
                                            TypeLess = Convert.ToBoolean(FormulaValuesArray[2]),
                                            GreaterThan = Convert.ToDouble(FormulaValuesArray[3]),
                                            LessThan = Convert.ToDouble(FormulaValuesArray[4])
                                        };

                                        if (formula14.TypeAll)
                                        {
                                            if (formula14.GreaterThan != null || formula14.LessThan != null)
                                            {
                                                var stockResult1 = stockResult.Where(x => ((x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3)) / ((x.Sclose + x.Slow + x.Shigh) / 3) * 100) >= 0).ToList();
                                                if (formula14.GreaterThan != null)
                                                {
                                                    stockResult1 = stockResult1.Where(x => ((x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3)) / ((x.Sclose + x.Slow + x.Shigh) / 3) * 100) > formula14.GreaterThan).ToList();
                                                }
                                                if (formula14.LessThan != null)
                                                {
                                                    stockResult1 = stockResult1.Where(x => ((x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3)) / ((x.Sclose + x.Slow + x.Shigh) / 3) * 100) < formula14.LessThan).ToList();
                                                }

                                                var stockResult2 = stockResult.Where(x => ((x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3)) / ((x.Sclose + x.Slow + x.Shigh) / 3) * 100) <= 0).ToList();
                                                if (formula14.GreaterThan != null)
                                                {
                                                    stockResult2 = stockResult2.Where(x => -((x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3)) / ((x.Sclose + x.Slow + x.Shigh) / 3) * 100) > formula14.GreaterThan).ToList();
                                                }
                                                if (formula14.LessThan != null)
                                                {
                                                    stockResult2 = stockResult2.Where(x => -((x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3)) / ((x.Sclose + x.Slow + x.Shigh) / 3) * 100) < formula14.LessThan).ToList();
                                                }
                                                stockResult1.AddRange(stockResult2);
                                                stockResult = stockResult1;
                                            }
                                        }
                                        else
                                        {
                                            if (formula14.TypeGreater)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3)) / ((x.Sclose + x.Slow + x.Shigh) / 3) * 100) >= 0).ToList();
                                                if (formula14.GreaterThan != null)
                                                {
                                                    stockResult = stockResult.Where(x => ((x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3)) / ((x.Sclose + x.Slow + x.Shigh) / 3) * 100) > formula14.GreaterThan).ToList();
                                                }
                                                if (formula14.LessThan != null)
                                                {
                                                    stockResult = stockResult.Where(x => ((x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3)) / ((x.Sclose + x.Slow + x.Shigh) / 3) * 100) < formula14.LessThan).ToList();
                                                }
                                            }
                                            else if (formula14.TypeLess)
                                            {
                                                stockResult = stockResult.Where(x => ((x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3)) / ((x.Sclose + x.Slow + x.Shigh) / 3) * 100) <= 0).ToList();
                                                if (formula14.GreaterThan != null)
                                                {
                                                    stockResult = stockResult.Where(x => -((x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3)) / ((x.Sclose + x.Slow + x.Shigh) / 3) * 100) > formula14.GreaterThan).ToList();
                                                }
                                                if (formula14.LessThan != null)
                                                {
                                                    stockResult = stockResult.Where(x => -((x.Sclose - ((x.Sclose + x.Slow + x.Shigh) / 3)) / ((x.Sclose + x.Slow + x.Shigh) / 3) * 100) < formula14.LessThan).ToList();
                                                }
                                            }
                                        }

                                        break;

                                    case 15:
                                        formula15 = new Formula15()
                                        {
                                            Between = Convert.ToDouble(FormulaValuesArray[0]),
                                            And = Convert.ToDouble(FormulaValuesArray[1])
                                        };

                                        if (formula15.Between != null)
                                        {
                                            stockResult = stockResult.Where(x => ((x.Shigh - x.PrevSlow) / (x.PrevShigh - x.PrevSlow) * 100) >= formula15.Between).ToList();
                                        }
                                        if (formula15.And != null)
                                        {
                                            stockResult = stockResult.Where(x => ((x.Shigh - x.PrevSlow) / (x.PrevShigh - x.PrevSlow) * 100) <= formula15.And).ToList();
                                        }

                                        break;


                                    #region case 17
                                    case 17:
                                        formula17 = new Formula17()
                                        {
                                            TypeAll = Convert.ToBoolean(FormulaValuesArray[0]),
                                            TypeGreater = Convert.ToBoolean(FormulaValuesArray[1]),
                                            TypeLess = Convert.ToBoolean(FormulaValuesArray[2]),
                                            FromDays = Convert.ToNullableInt(FormulaValuesArray[3]),
                                            ToDays = Convert.ToNullableInt(FormulaValuesArray[4]),
                                            GreaterThan = Convert.ToDouble(FormulaValuesArray[5]),
                                            LessThan = Convert.ToDouble(FormulaValuesArray[6])
                                        };

                                        if (formula17.FromDays != null && formula17.FromDays.Value > 0 && formula17.ToDays != null && formula17.ToDays.Value > 0)
                                        {
                                            var listOfCodes = stockResult.Select(y => y.Sticker).ToList();
                                            int relatedDayNo = startDayNo - 1;
                                            int totalFromDayNo = formula17.FromDays.Value + relatedDayNo;
                                            int totalToDayNo = formula17.ToDays.Value + relatedDayNo;
                                            var filteredStockResult = db.RecommendationsResultsViews.AsNoTracking()
                                                                    .Where(x => x.DayNo >= totalFromDayNo && x.DayNo <= totalToDayNo && listOfCodes.Contains(x.Sticker)
                                                                    && x.Shigh >= x.NextShigh && x.Shigh >= x.PrevShigh);
                                            //filteredStockResult = filteredStockResult.Where(x => x.Shigh >= x.NextShigh && x.Shigh >= x.PrevShigh).ToList();
                                            List<Formula17PenetrationObject> PenetrationPointResult = new List<Formula17PenetrationObject>();

                                            //Parallel.ForEach(filteredStockResult, filteredStockItem =>
                                            foreach (var filteredStockItem in filteredStockResult)
                                            {
                                                var result = db.RecommendationsResultsViews.AsNoTracking()
                                                    .Where(x => x.Sticker == filteredStockItem.Sticker
                                                    && x.DayNo < filteredStockItem.DayNo
                                                    && x.Shigh > filteredStockItem.Shigh);
                                                if (result.Count() == 1)
                                                {
                                                    if (result.First().DayNo == startDayNo)
                                                    {
                                                        var PenetrationPointItems = new Formula17PenetrationObject()
                                                        {
                                                            LatestHigh = result.First().Shigh.Value,
                                                            StockItem = filteredStockItem
                                                        };
                                                        PenetrationPointResult.Add(PenetrationPointItems);
                                                    }
                                                }
                                            }

                                            if (formula17.TypeAll)
                                            {
                                                if (formula17.GreaterThan != null || formula17.LessThan != null)
                                                {
                                                    var PenetrationPointResult1 = PenetrationPointResult.Where(x => (((x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh) * 100) >= 0).ToList();
                                                    if (formula17.GreaterThan != null)
                                                    {
                                                        PenetrationPointResult1 = PenetrationPointResult1.Where(x => (((x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh) * 100) >= formula17.GreaterThan).ToList();
                                                    }
                                                    if (formula17.LessThan != null)
                                                    {
                                                        PenetrationPointResult1 = PenetrationPointResult1.Where(x => (((x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh) * 100) <= formula17.LessThan).ToList();
                                                    }

                                                    var PenetrationPointResult2 = PenetrationPointResult.Where(x => (((x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh) * 100) <= 0).ToList();
                                                    if (formula17.GreaterThan != null)
                                                    {
                                                        PenetrationPointResult2 = PenetrationPointResult2.Where(x => -(((x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh) * 100) >= formula17.GreaterThan).ToList();
                                                    }
                                                    if (formula17.LessThan != null)
                                                    {
                                                        PenetrationPointResult2 = PenetrationPointResult2.Where(x => -(((x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh) * 100) <= formula17.LessThan).ToList();
                                                    }
                                                    PenetrationPointResult1.AddRange(PenetrationPointResult2);
                                                    PenetrationPointResult = PenetrationPointResult1;
                                                }
                                            }
                                            else
                                            {
                                                if (formula17.TypeGreater)
                                                {
                                                    PenetrationPointResult = PenetrationPointResult.Where(x => (((x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh) * 100) >= 0).ToList();
                                                    if (formula17.GreaterThan != null)
                                                    {
                                                        PenetrationPointResult = PenetrationPointResult.Where(x => (((x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh) * 100) >= formula17.GreaterThan).ToList();
                                                    }
                                                    if (formula17.LessThan != null)
                                                    {
                                                        PenetrationPointResult = PenetrationPointResult.Where(x => (((x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh) * 100) <= formula17.LessThan).ToList();
                                                    }
                                                }
                                                else if (formula17.TypeLess)
                                                {
                                                    PenetrationPointResult = PenetrationPointResult.Where(x => (((x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh) * 100) <= 0).ToList();
                                                    if (formula17.GreaterThan != null)
                                                    {
                                                        PenetrationPointResult = PenetrationPointResult.Where(x => -(((x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh) * 100) >= formula17.GreaterThan).ToList();
                                                    }
                                                    if (formula17.LessThan != null)
                                                    {
                                                        PenetrationPointResult = PenetrationPointResult.Where(x => -(((x.LatestHigh - x.StockItem.Shigh) / x.StockItem.Shigh) * 100) <= formula17.LessThan).ToList();
                                                    }
                                                }

                                            };

                                            // filter
                                            List<StockPrevDayView> newStockResult = new List<StockPrevDayView>();
                                            var finalPenetrationPointResult = PenetrationPointResult.Select(x => x.StockItem.Sticker).Distinct();
                                            foreach (var StickerItem in finalPenetrationPointResult)
                                            {
                                                newStockResult.Add(db.StockPrevDayViews.Where(x => x.Sticker.Equals(StickerItem) && x.DayNo == FormulaDayNo).FirstOrDefault());
                                            }
                                            stockResult = newStockResult;
                                        }

                                        break;
                                    #endregion
                                    default:
                                        break;
                                }
                                #endregion

                            }

                        }

                        var companiesResult = stockResult.GroupBy(x => x.ParentIndicator).ToList();
                        string ParentIndicator = "";
                        foreach (var item in companiesResult)
                        {
                            ParentIndicator = item.First().ParentIndicator;

                            var parentIndicator = db.StockPrevDayViews.AsNoTracking().Where(x => x.Sticker.Equals(ParentIndicator)
                                                                        && x.DayNo == startDayNo && x.ParentIndicator != null).FirstOrDefault();
                            if (parentIndicator != null)
                            {
                                stockResultSortedList.Add(parentIndicator);
                            }
                            if (item.Key != null && item.Key.Equals("TASI") && companiesResult.Count() > 1)
                            {
                                var temp = item.ToList();
                                foreach (var indicatorItem in item)
                                {
                                    if (stockResult.Contains(indicatorItem))
                                    {
                                        temp.Remove(indicatorItem);
                                    }
                                }
                                stockResultSortedList.AddRange(temp);
                            }
                            else
                            {
                                stockResultSortedList.AddRange(item);
                            }
                        }



                    }




















                    oneCriteriaVM.CompaniesCount = stockResultSortedList.Where(x => x.IsIndicator == false).Count();
                    if (oneCriteriaVM.CompaniesCount == 0)
                        oneCriteriaVM.CompaniesCount = stockResultSortedList.Where(x => x.IsIndicator).Count();

                    CriteriaViewMode.Add(oneCriteriaVM);
                }
            }
            model.criteriaVM = CriteriaViewMode;

            return View(model);
        }



















        protected void criteriaStartDate_SelectedDateChanged(object sender, Telerik.Web.UI.Calendar.SelectedDateChangedEventArgs e)
        {
            if (CriteriaCompaniesBLL.ChackIfDateExists(e.NewDate))
            {
                SessionHelper.CriteriaStartDate = e.NewDate;
                SessionHelper.RecommendationStartDate = e.NewDate;
                //Response.Redirect("FormulasSetting.aspx", true);
                LoadChangeDateNewDate();
            }
            else
            {
                // message
                WrongDateLabel.Visible = true;
            }
        }

        protected void criteriaNextDate_Click(object sender, EventArgs e)
        {
            bool success = false;
            if (SessionHelper.CriteriaStartDate == null)
            {
                SessionHelper.CriteriaStartDate = criteriaStartDate.SelectedDate;
            }
            if (SessionHelper.CriteriaStartDate != null)
            {
                var newDate = CriteriaCompaniesBLL.GetRecommendationpNextDate(SessionHelper.CriteriaStartDate);
                if (CriteriaCompaniesBLL.ChackIfDateExists(newDate))
                {
                    success = true;
                    SessionHelper.CriteriaStartDate = newDate;
                    SessionHelper.RecommendationStartDate = newDate;
                    //Response.Redirect("FormulasSetting.aspx", true);
                    LoadChangeDateNewDate();
                }
            }

            if (!success)
            {
                // message
                WrongDateLabel.Visible = true;
            }
        }

        protected void criteriaPrevDate_Click(object sender, EventArgs e)
        {
            bool success = false;
            if (SessionHelper.CriteriaStartDate == null)
            {
                SessionHelper.CriteriaStartDate = criteriaStartDate.SelectedDate;
            }
            if (SessionHelper.CriteriaStartDate != null)
            {
                var newDate = CriteriaCompaniesBLL.GetRecommendationpPreviousDate(SessionHelper.CriteriaStartDate);
                if (CriteriaCompaniesBLL.ChackIfDateExists(newDate))
                {
                    success = true;
                    SessionHelper.CriteriaStartDate = newDate;
                    SessionHelper.RecommendationStartDate = newDate;
                    //Response.Redirect("FormulasSetting.aspx", true);
                    LoadChangeDateNewDate();
                }
            }

            if (!success)
            {
                // message
                WrongDateLabel.Visible = true;
            }
        }

        protected void CriteriaGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string color = ((Criteria)e.Row.DataItem).Color;
                if (!string.IsNullOrEmpty(color))
                {
                    string colorName = color;
                    colorName = "#" + colorName.Substring(3, colorName.Length - 3) + "75";
                    e.Row.Style.Add("background-color", colorName);
                }
            }
        }

        protected void CriteriaGridView_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

        }

        //protected void CriteriaGridView1_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        //{
        //    //CriteriaGridView1.DataSource = CriteriaBLL.GetCriterias();
        //}

        protected void CriteriaGridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            CriteriaGridView.PageIndex = e.NewPageIndex;
            CriteriaGridView.DataSource = CriteriaBLL.GetCriterias();
            CriteriaGridView.DataBind();
        }

