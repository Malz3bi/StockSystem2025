using Microsoft.AspNetCore.Mvc;
using StockSystem2025.Models;
using StockSystem2025.Services;
using Formula1 = StockSystem2025.ViewModel.Formula1;
using Formula2 = StockSystem2025.ViewModel.Formula2;
using Formula3 = StockSystem2025.ViewModel.Formula3;
using Formula4 = StockSystem2025.ViewModel.Formula4;
using Formula5 = StockSystem2025.ViewModel.Formula5;
using Formula6 = StockSystem2025.ViewModel.Formula6;
using Formula7 = StockSystem2025.ViewModel.Formula7;
using Formula8 = StockSystem2025.ViewModel.Formula8;
using Formula9 = StockSystem2025.ViewModel.Formula9;
using Formula10 = StockSystem2025.ViewModel.Formula10;
using Formula11 = StockSystem2025.ViewModel.Formula11;
using Formula12 = StockSystem2025.ViewModel.Formula12;
using Formula13 = StockSystem2025.ViewModel.Formula13;
using Formula14 = StockSystem2025.ViewModel.Formula14;
using Formula15 = StockSystem2025.ViewModel.Formula15;
using Formula16 = StockSystem2025.ViewModel.Formula16;
using Formula17 = StockSystem2025.ViewModel.Formula17;
using StockSystem2025.ViewModel;
using StockSystem2025.ViewModels;

namespace StockSystem2025.Controllers
{
    public class CriteriaController : Controller
    {
        private readonly ICriteriaService _criteriaService;
        private readonly ICurrentUserService _currentUserService;

        public CriteriaController(ICriteriaService criteriaService, ICurrentUserService currentUserService)
        {
            _criteriaService = criteriaService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> Manage(int? id)
        {
            var model = new CriteriaManageViewModel();
            if (id.HasValue)
            {
                var criteria = await _criteriaService.GetCriteriaByIdAsync(id.Value);
                if (criteria != null)
                {
                    model = MapToViewModel(criteria);
                    ViewData["IsEdit"] = true;
                }
            }
            else
            {
                model.Formulas.Add(new FormulaManageViewModel { Day = 1, FormulaType = 1 });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(CriteriaManageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var criteria = MapToCriteria(model);
            //var user = await _currentUserService.GetCurrentUserAsync();
            criteria.UserId = 1; /*user.Id;*/
            if (true/*user.Role == 10*/) // Assuming admin role
            {
                criteria.IsGeneral = model.IsGeneral;
            }

            await _criteriaService.SaveCriteriaAsync(criteria);
            return RedirectToAction("FormulasSettingIndex", "FormulasSetting");
        }

        [HttpPost]
        public IActionResult AddDay(CriteriaManageViewModel model)
        {
            var newDay = model.Formulas.Max(f => f.Day) + 1;
            model.Formulas.Add(new FormulaManageViewModel { Day = newDay, FormulaType = newDay });
            return View("Manage", model);
        }

        [HttpPost]
        public IActionResult DeleteDay(CriteriaManageViewModel model, int dayNo)
        {
            model.Formulas.RemoveAll(f => f.Day == dayNo);
            var day = 1;
            foreach (var formula in model.Formulas.OrderBy(f => f.Day))
            {
                formula.Day = day++;
            }
            return View("Manage", model);
        }

        private CriteriaManageViewModel MapToViewModel(Criteria criteria)
        {
            var model = new CriteriaManageViewModel
            {
                Id = criteria.Id,
                Name = criteria.Name,
                Type = criteria.Type,
                Note = criteria.Note,
                Separater = criteria.Separater,
                OrderNo = criteria.OrderNo,
                Description = criteria.Description,
                Color = criteria.Color,
                ImageUrl = criteria.ImageUrl,
                IsIndicator = criteria.IsIndicator,
                IsGeneral = criteria.IsGeneral ?? false,
                InsertToMarketPage = false
            };

            var formulaGroups = criteria.Formulas.GroupBy(f => f.Day);
            foreach (var group in formulaGroups)
            {
                var formulaVm = new FormulaManageViewModel { Day = group.Key, FormulaType = group.First().FormulaType };
                foreach (var formula in group)
                {
                    var values = formula.FormulaValues?.Split(';') ?? new string[10];
                    switch (formula.FormulaType)
                    {
                        case 1:
                            formulaVm.Formula1 = new Formula1
                            {
                                TypeAll = bool.TryParse(values.ElementAtOrDefault(0), out var ta) && ta,
                                TypePositive = bool.TryParse(values.ElementAtOrDefault(1), out var tp) && tp,
                                TypeNegative = bool.TryParse(values.ElementAtOrDefault(2), out var tn) && tn,
                                TypeNoChange = bool.TryParse(values.ElementAtOrDefault(3), out var tnc) && tnc,
                                GreaterThan = double.TryParse(values.ElementAtOrDefault(4), out var gt) ? gt : null,
                                LessThan = double.TryParse(values.ElementAtOrDefault(5), out var lt) ? lt : null
                            };
                            break;
                        case 2:
                            formulaVm.Formula2 = new Formula2
                            {
                                GreaterThan = double.TryParse(values.ElementAtOrDefault(0), out var gt2) ? gt2 : null,
                                LessThan = double.TryParse(values.ElementAtOrDefault(1), out var lt2) ? lt2 : null
                            };
                            break;
                        case 3:
                            formulaVm.Formula3 = new Formula3
                            {
                                TypeAll = bool.TryParse(values.ElementAtOrDefault(0), out var ta3) && ta3,
                                TypeHigherGap = bool.TryParse(values.ElementAtOrDefault(1), out var thg) && thg,
                                TypeLowerGap = bool.TryParse(values.ElementAtOrDefault(2), out var tlg) && tlg,
                                GreaterThan = double.TryParse(values.ElementAtOrDefault(3), out var gt3) ? gt3 : null,
                                LessThan = double.TryParse(values.ElementAtOrDefault(4), out var lt3) ? lt3 : null
                            };
                            break;
                        case 4:
                            formulaVm.Formula4 = new Formula4
                            {
                                TypeAll = bool.TryParse(values.ElementAtOrDefault(0), out var ta4) && ta4,
                                TypeHigher = bool.TryParse(values.ElementAtOrDefault(1), out var th) && th,
                                TypeLower = bool.TryParse(values.ElementAtOrDefault(2), out var tl) && tl,
                                GreaterThan = double.TryParse(values.ElementAtOrDefault(3), out var gt4) ? gt4 : null,
                                LessThan = double.TryParse(values.ElementAtOrDefault(4), out var lt4) ? lt4 : null
                            };
                            break;
                        case 5:
                            formulaVm.Formula5 = new Formula5
                            {
                                TypeAll = bool.TryParse(values.ElementAtOrDefault(0), out var ta5) && ta5,
                                TypeHigherGap = bool.TryParse(values.ElementAtOrDefault(1), out var thg5) && thg5,
                                TypeLowerGap = bool.TryParse(values.ElementAtOrDefault(2), out var tlg5) && tlg5
                            };
                            break;
                        case 6:
                            formulaVm.Formula6 = new Formula6
                            {
                                Between = double.TryParse(values.ElementAtOrDefault(0), out var b6) ? b6 : null,
                                And = double.TryParse(values.ElementAtOrDefault(1), out var a6) ? a6 : null
                            };
                            break;
                        case 7:
                            formulaVm.Formula7 = new Formula7
                            {
                                Between = double.TryParse(values.ElementAtOrDefault(0), out var b7) ? b7 : null,
                                And = double.TryParse(values.ElementAtOrDefault(1), out var a7) ? a7 : null
                            };
                            break;
                        case 8:
                            formulaVm.Formula8 = new Formula8
                            {
                                Between = double.TryParse(values.ElementAtOrDefault(0), out var b8) ? b8 : null,
                                And = double.TryParse(values.ElementAtOrDefault(1), out var a8) ? a8 : null
                            };
                            break;
                        case 9:
                            formulaVm.Formula9 = new Formula9
                            {
                                TypeAll = bool.TryParse(values.ElementAtOrDefault(0), out var ta9) && ta9,
                                TypePositive = bool.TryParse(values.ElementAtOrDefault(1), out var tp9) && tp9,
                                TypeNegative = bool.TryParse(values.ElementAtOrDefault(2), out var tn9) && tn9,
                                GreaterThan = double.TryParse(values.ElementAtOrDefault(3), out var gt9) ? gt9 : null,
                                LessThan = double.TryParse(values.ElementAtOrDefault(4), out var lt9) ? lt9 : null
                            };
                            break;
                        case 10:
                            formulaVm.Formula10 = new Formula10
                            {
                                Between = double.TryParse(values.ElementAtOrDefault(0), out var b10) ? b10 : null,
                                And = double.TryParse(values.ElementAtOrDefault(1), out var a10) ? a10 : null
                            };
                            break;
                        case 11:
                            formulaVm.Formula11 = new Formula11
                            {
                                MaximumAll = bool.TryParse(values.ElementAtOrDefault(0), out var ma) && ma,
                                MaximumGreater = bool.TryParse(values.ElementAtOrDefault(1), out var mg) && mg,
                                MaximumLess = bool.TryParse(values.ElementAtOrDefault(2), out var ml) && ml,
                                MaximumBetween = double.TryParse(values.ElementAtOrDefault(3), out var mb) ? mb : null,
                                MaximumAnd = double.TryParse(values.ElementAtOrDefault(4), out var ma11) ? ma11 : null,
                                MinimumAll = bool.TryParse(values.ElementAtOrDefault(5), out var mina) && mina,
                                MinimumGreater = bool.TryParse(values.ElementAtOrDefault(6), out var ming) && ming,
                                MinimumLess = bool.TryParse(values.ElementAtOrDefault(7), out var minl) && minl,
                                MinimumBetween = double.TryParse(values.ElementAtOrDefault(8), out var minb) ? minb : null,
                                MinimumAnd = double.TryParse(values.ElementAtOrDefault(9), out var mina11) ? mina11 : null
                            };
                            break;
                        case 12:
                            formulaVm.Formula12 = new Formula12
                            {
                                TypeAll = bool.TryParse(values.ElementAtOrDefault(0), out var ta12) && ta12,
                                TypeGreater = bool.TryParse(values.ElementAtOrDefault(1), out var tg12) && tg12,
                                TypeLess = bool.TryParse(values.ElementAtOrDefault(2), out var tl12) && tl12,
                                GreaterThan = double.TryParse(values.ElementAtOrDefault(3), out var gt12) ? gt12 : null,
                                LessThan = double.TryParse(values.ElementAtOrDefault(4), out var lt12) ? lt12 : null
                            };
                            break;
                        case 13:
                            formulaVm.Formula13 = new Formula13
                            {
                                TypeAll = bool.TryParse(values.ElementAtOrDefault(0), out var ta13) && ta13,
                                TypePositive = bool.TryParse(values.ElementAtOrDefault(1), out var tp13) && tp13,
                                TypeNegative = bool.TryParse(values.ElementAtOrDefault(2), out var tn13) && tn13,
                                Days = int.TryParse(values.ElementAtOrDefault(3), out var d13) ? d13 : null,
                                GreaterThan = double.TryParse(values.ElementAtOrDefault(4), out var gt13) ? gt13 : null,
                                LessThan = double.TryParse(values.ElementAtOrDefault(5), out var lt13) ? lt13 : null
                            };
                            break;
                        case 14:
                            formulaVm.Formula14 = new Formula14
                            {
                                TypeAll = bool.TryParse(values.ElementAtOrDefault(0), out var ta14) && ta14,
                                TypeGreater = bool.TryParse(values.ElementAtOrDefault(1), out var tg14) && tg14,
                                TypeLess = bool.TryParse(values.ElementAtOrDefault(2), out var tl14) && tl14,
                                GreaterThan = double.TryParse(values.ElementAtOrDefault(3), out var gt14) ? gt14 : null,
                                LessThan = double.TryParse(values.ElementAtOrDefault(4), out var lt14) ? lt14 : null
                            };
                            break;
                        case 15:
                            formulaVm.Formula15 = new Formula15
                            {
                                Between = double.TryParse(values.ElementAtOrDefault(0), out var b15) ? b15 : null,
                                And = double.TryParse(values.ElementAtOrDefault(1), out var a15) ? a15 : null
                            };
                            break;
                        case 16:
                            formulaVm.Formula16 = new Formula16
                            {
                                TypeAll = bool.TryParse(values.ElementAtOrDefault(0), out var ta16) && ta16,
                                TypePositive = bool.TryParse(values.ElementAtOrDefault(1), out var tp16) && tp16,
                                TypeNegative = bool.TryParse(values.ElementAtOrDefault(2), out var tn16) && tn16,
                                BetweenPositive = double.TryParse(values.ElementAtOrDefault(3), out var bp) ? bp : null,
                                AndPositive = double.TryParse(values.ElementAtOrDefault(4), out var ap) ? ap : null,
                                BetweenNegative = double.TryParse(values.ElementAtOrDefault(5), out var bn) ? bn : null,
                                AndNegative = double.TryParse(values.ElementAtOrDefault(6), out var an) ? an : null
                            };
                            break;
                        case 17:
                            formulaVm.Formula17 = new Formula17
                            {
                                TypeAll = bool.TryParse(values.ElementAtOrDefault(0), out var ta17) && ta17,
                                TypeGreater = bool.TryParse(values.ElementAtOrDefault(1), out var tg17) && tg17,
                                TypeLess = bool.TryParse(values.ElementAtOrDefault(2), out var tl17) && tl17,
                                FromDays = int.TryParse(values.ElementAtOrDefault(3), out var fd) ? fd : null,
                                ToDays = int.TryParse(values.ElementAtOrDefault(4), out var td) ? td : null,
                                GreaterThan = double.TryParse(values.ElementAtOrDefault(5), out var gt17) ? gt17 : null,
                                LessThan = double.TryParse(values.ElementAtOrDefault(6), out var lt17) ? lt17 : null
                            };
                            break;
                    }
                }
                model.Formulas.Add(formulaVm);
            }
            return model;
        }

        private Criteria MapToCriteria(CriteriaManageViewModel model)
        {
            var criteria = new Criteria
            {
                Id = model.Id,
                Name = model.Name,
                Type = model.Type,
                Note = model.Note,
                Separater = model.Separater,
                OrderNo = model.OrderNo,
                Description = model.Description,
                Color = model.Color,
                ImageUrl = model.ImageUrl,
                IsIndicator = model.IsIndicator,
                IsGeneral = model.IsGeneral
            };

            foreach (var formulaVm in model.Formulas)
            {
                for (int i = 1; i <= 17; i++)
                {
                    var formula = new Formula { Day = formulaVm.Day, FormulaType = i };
                    string formulaValues = string.Empty;
                    switch (i)
                    {
                        case 1:
                            formulaValues = $"{formulaVm.Formula1.TypeAll};{formulaVm.Formula1.TypePositive};{formulaVm.Formula1.TypeNegative};{formulaVm.Formula1.TypeNoChange};{formulaVm.Formula1.GreaterThan};{formulaVm.Formula1.LessThan}";
                            break;
                        case 2:
                            formulaValues = $"{formulaVm.Formula2.GreaterThan};{formulaVm.Formula2.LessThan}";
                            break;
                        case 3:
                            formulaValues = $"{formulaVm.Formula3.TypeAll};{formulaVm.Formula3.TypeHigherGap};{formulaVm.Formula3.TypeLowerGap};{formulaVm.Formula3.GreaterThan};{formulaVm.Formula3.LessThan}";
                            break;
                        case 4:
                            formulaValues = $"{formulaVm.Formula4.TypeAll};{formulaVm.Formula4.TypeHigher};{formulaVm.Formula4.TypeLower};{formulaVm.Formula4.GreaterThan};{formulaVm.Formula4.LessThan}";
                            break;
                        case 5:
                            formulaValues = $"{formulaVm.Formula5.TypeAll};{formulaVm.Formula5.TypeHigherGap};{formulaVm.Formula5.TypeLowerGap}";
                            break;
                        case 6:
                            formulaValues = $"{formulaVm.Formula6.Between};{formulaVm.Formula6.And}";
                            break;
                        case 7:
                            formulaValues = $"{formulaVm.Formula7.Between};{formulaVm.Formula7.And}";
                            break;
                        case 8:
                            formulaValues = $"{formulaVm.Formula8.Between};{formulaVm.Formula8.And}";
                            break;
                        case 9:
                            formulaValues = $"{formulaVm.Formula9.TypeAll};{formulaVm.Formula9.TypePositive};{formulaVm.Formula9.TypeNegative};{formulaVm.Formula9.GreaterThan};{formulaVm.Formula9.LessThan}";
                            break;
                        case 10:
                            formulaValues = $"{formulaVm.Formula10.Between};{formulaVm.Formula10.And}";
                            break;
                        case 11:
                            formulaValues = $"{formulaVm.Formula11.MaximumAll};{formulaVm.Formula11.MaximumGreater};{formulaVm.Formula11.MaximumLess};{formulaVm.Formula11.MaximumBetween};{formulaVm.Formula11.MaximumAnd};{formulaVm.Formula11.MinimumAll};{formulaVm.Formula11.MinimumGreater};{formulaVm.Formula11.MinimumLess};{formulaVm.Formula11.MinimumBetween};{formulaVm.Formula11.MinimumAnd}";
                            break;
                        case 12:
                            formulaValues = $"{formulaVm.Formula12.TypeAll};{formulaVm.Formula12.TypeGreater};{formulaVm.Formula12.TypeLess};{formulaVm.Formula12.GreaterThan};{formulaVm.Formula12.LessThan}";
                            break;
                        case 13:
                            formulaValues = $"{formulaVm.Formula13.TypeAll};{formulaVm.Formula13.TypePositive};{formulaVm.Formula13.TypeNegative};{formulaVm.Formula13.Days};{formulaVm.Formula13.GreaterThan};{formulaVm.Formula13.LessThan}";
                            break;
                        case 14:
                            formulaValues = $"{formulaVm.Formula14.TypeAll};{formulaVm.Formula14.TypeGreater};{formulaVm.Formula14.TypeLess};{formulaVm.Formula14.GreaterThan};{formulaVm.Formula14.LessThan}";
                            break;
                        case 15:
                            formulaValues = $"{formulaVm.Formula15.Between};{formulaVm.Formula15.And}";
                            break;
                        case 16:
                            formulaValues = $"{formulaVm.Formula16.TypeAll};{formulaVm.Formula16.TypePositive};{formulaVm.Formula16.TypeNegative};{formulaVm.Formula16.BetweenPositive};{formulaVm.Formula16.AndPositive};{formulaVm.Formula16.BetweenNegative};{formulaVm.Formula16.AndNegative}";
                            break;
                        case 17:
                            formulaValues = $"{formulaVm.Formula17.TypeAll};{formulaVm.Formula17.TypeGreater};{formulaVm.Formula17.TypeLess};{formulaVm.Formula17.FromDays};{formulaVm.Formula17.ToDays};{formulaVm.Formula17.GreaterThan};{formulaVm.Formula17.LessThan}";
                            break;
                    }
                    if (!string.IsNullOrEmpty(formulaValues) && formulaValues != "True;False;False;;;")
                    {
                        formula.FormulaValues = formulaValues;
                        criteria.Formulas.Add(formula);
                    }
                }
            }
            return criteria;
        }
    }
}