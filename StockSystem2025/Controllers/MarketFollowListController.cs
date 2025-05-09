using Microsoft.AspNetCore.Mvc;
using StockSystem2025.Extensions;
using StockSystem2025.Models;
using StockSystem2025.Services;
using StockSystem2025.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockSystem2025.Controllers
{
    public class MarketFollowListController : Controller
    {
        private readonly IStockService _stockService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICriteriaService _criteriaService;

        public MarketFollowListController(
            IStockService stockService,
            ICurrentUserService currentUserService,
            ICriteriaService criteriaService)
        {
            _stockService = stockService;
            _currentUserService = currentUserService;
            _criteriaService = criteriaService;
        }

        public async Task<IActionResult> Index(string q = "1", string code = "")
        {
            var model = await CreateViewModel(q, code);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDate(string date)
        {
            if (DateTime.TryParse(date, out var parsedDate) && await _stockService.CheckIfDateExists(parsedDate))
            {
                HttpContext.Session.SetRecommendationStartDate(parsedDate);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> NextDate(string currentDate)
        {
            if (DateTime.TryParse(currentDate, out var parsedDate))
            {
                var newDate = await _stockService.GetRecommendationNextDate(parsedDate);
                if (await _stockService.CheckIfDateExists(newDate))
                {
                    HttpContext.Session.SetRecommendationStartDate(newDate);
                    return Json(new { success = true, newDate = newDate.ToString("yyyy/MM/dd") });
                }
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<IActionResult> PreviousDate(string currentDate)
        {
            if (DateTime.TryParse(currentDate, out var parsedDate))
            {
                var newDate = await _stockService.GetRecommendationPreviousDate(parsedDate);
                if (await _stockService.CheckIfDateExists(newDate))
                {
                    HttpContext.Session.SetRecommendationStartDate(newDate);
                    return Json(new { success = true, newDate = newDate.ToString("yyyy/MM/dd") });
                }
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> AddToFollowList(string companyCode, int followListId)
        {
            await _stockService.AddToFollowList(followListId, companyCode);
            return Json(new { success = true });
        }

        private async Task<MarketFollowListViewModel> CreateViewModel(string q, string code)
        {
            var userId = 1 /*_currentUserService.GetCurrentUser().Id*/;
            var recommendationDate = HttpContext.Session.GetRecommendationStartDate() ?? await _stockService.GetLastDate();
            var dayNo = await _stockService.GetDayNumberByDate(recommendationDate);

            var stocks = await GetStocks(q, code, dayNo);
            var generalIndicators = await GetGeneralIndicators(dayNo);
            var followLists = await _stockService.GetFollowLists(userId);
            var followListCompanies = await _stockService.GetFollowListCompanies(userId);
            var weeklyValues = await GetWeeklyValues(stocks, dayNo);

            var settings = await _stockService.GetSettings();
            var specialCompanyColor = settings.FirstOrDefault(s => s.Name == "SpecialCompanyColor")?.Value ?? "#ffffff";

            return new MarketFollowListViewModel
            {
                CriteriaStartDate = recommendationDate,
                GeneralIndicators = generalIndicators,
                Stocks = stocks,
                FollowLists = followLists,
                FollowListCompanies = followListCompanies,
                WeeklyValues = weeklyValues,
                MinDate = await _stockService.GetMinDate(),
                MaxDate = await _stockService.GetMaxDate(),
                StopLossValue = 5.0, // Should come from settings or FollowList
                FirstSupportValue = 3.0,
                SecondSupportValue = 4.0,
                FirstTargetValue = 3.0,
                SecondTargetValue = 4.0,
                ThirdTargetValue = 5.0,
                SpecialCompanyColor = specialCompanyColor,
                CurrentDayNo = dayNo
            };
        }

        private async Task<List<StockPrevDayView>> GetGeneralIndicators(int dayNo)
        {
            var indicators = await _stockService.GetGeneralIndicators(dayNo);
            if (!indicators.Any())
            {
                indicators.Add(new StockPrevDayView
                {
                    Sticker = "TASI",
                    Sname = await _stockService.GetCompanyName("TASI"),
                    Sopen = 0,
                    Shigh = 0,
                    Slow = 0,
                    Sclose = 0,
                    Svol = 0,
                    PrevSclose = 0
                });
            }
            return indicators;
        }

        private async Task<List<StockPrevDayView>> GetStocks(string q, string code, int dayNo)
        {
            var stocks = await _stockService.GetStockPrevDayByDayNo(dayNo);
            return q switch
            {
                "1" => stocks.Where(c => !c.IsIndicator).ToList(),
                "2" => stocks,
                "3" => stocks.Where(c => c.IsIndicator).ToList(),
                "4" => stocks.Where(c => c.Sticker == code || c.ParentIndicator == code)
                             .OrderByDescending(c => c.IsIndicator).ToList(),
                _ => stocks
            };
        }

        private async Task<Dictionary<string, WeeklyValue>> GetWeeklyValues(List<StockPrevDayView> stocks, int currentDayNo)
        {
            var result = new Dictionary<string, WeeklyValue>();
            foreach (var stock in stocks)
            {
                var maxHigh = await _stockService.GetMaxHigh(stock.Sticker, currentDayNo);
                result[stock.Sticker] = new WeeklyValue { MaxHigh = maxHigh ?? 0 };
            }
            return result;
        }
    }
}