using Microsoft.AspNetCore.Mvc;

using StockSystem2025.SFLModels;
using StockSystem2025.SFLServices;
using System;
using System.Threading.Tasks;

namespace StockSystem2025.Controllers
{
    public class SFLSpecialFollowListController : Controller
    {
        private readonly SFLIFollowListService _followListService;
        private readonly SFLIStockService _stockService;
        private readonly SFLISettingsService _settingsService;

        public SFLSpecialFollowListController(
            SFLIFollowListService followListService,
            SFLIStockService stockService,
            SFLISettingsService settingsService)
        {
            _followListService = followListService;
            _stockService = stockService;
            _settingsService = settingsService;
        }

        public async Task<IActionResult> SFLIndex(int? followListId, DateTime? recommendationDate, string sortColumn = "", string sortOrder = "asc")
        {
            var userId = GetCurrentUserId();
            var model = new SFLSpecialFollowListViewModel
            {
                SFLFollowLists = await _followListService.SFLGetFollowListsAsync(userId),
                SFLRecommendationDate = recommendationDate ?? await _stockService.SFLGetLastDateAsync(),
                SFLMinDate = await _stockService.SFLGetMinDateAsync(),
                SFLMaxDate = await _stockService.SFLGetMaxDateAsync(),
                SFLSortColumn = sortColumn,
                SFLSortOrder = sortOrder
            };

            if (model.SFLFollowLists.Count == 0)
            {
                return RedirectToAction("Settings", "Account");
            }

            model.SFLFollowListId = followListId ?? model.SFLFollowLists.First().Id;
            model.SFLGeneralIndicator = await _stockService.SFLGetGeneralIndicatorAsync(model.SFLRecommendationDate);
            model.SFLRecommendations = await _stockService.SFLGetRecommendationsAsync(model.SFLFollowListId, model.SFLRecommendationDate, sortColumn, sortOrder);
            model.SFLSpecialCompanyColor = await _settingsService.SFLGetSettingValueAsync("SpecialCompanyColor");
            model.SFLStopLossColor = await _settingsService.SFLGetSettingValueAsync("StopLossColor");
            model.SFLFirstSupportColor = await _settingsService.SFLGetSettingValueAsync("FirstSupportColor");
            model.SFLSecondSupportColor = await _settingsService.SFLGetSettingValueAsync("SecondSupportColor");
            model.SFLFirstTargetColor = await _settingsService.SFLGetSettingValueAsync("FirstTargetColor");
            model.SFLSecondTargetColor = await _settingsService.SFLGetSettingValueAsync("SecondTargetColor");
            model.SFLThirdTargetColor = await _settingsService.SFLGetSettingValueAsync("ThirdTargetColor");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SFLChangeDate(int followListId, string sortColumn, string sortOrder, DateTime? newDate, string direction = null)
        {
            DateTime targetDate;

            if (!string.IsNullOrEmpty(direction))
            {
                var currentDate = newDate ?? await _stockService.SFLGetLastDateAsync();
                targetDate = direction switch
                {
                    "next" => await _stockService.SFLGetNextRecommendationDateAsync(currentDate),
                    "prev" => await _stockService.SFLGetPreviousRecommendationDateAsync(currentDate),
                    _ => currentDate
                };
            }
            else if (newDate.HasValue && await _stockService.SFLDateExistsAsync(newDate.Value))
            {
                targetDate = newDate.Value;
            }
            else
            {
                TempData["Error"] = "يرجى إدخال تاريخ صحيح ضمن النطاق المتاح";
                return RedirectToAction("SFLIndex", new { followListId, sortColumn, sortOrder });
            }

            return RedirectToAction("SFLIndex", new { followListId, recommendationDate = targetDate, sortColumn, sortOrder });
        }

        [HttpPost]
        public async Task<IActionResult> SFLChangeFollowList(int followListId, DateTime recommendationDate, string sortColumn, string sortOrder)
        {
            return RedirectToAction("SFLIndex", new { followListId, recommendationDate, sortColumn, sortOrder });
        }

        [HttpPost]
        public async Task<IActionResult> SFLDeleteCompany(string companyCode, int followListId, DateTime recommendationDate, string sortColumn, string sortOrder)
        {
            await _followListService.SFLDeleteCompanyAsync(followListId, companyCode);
            return RedirectToAction("SFLIndex", new { followListId, recommendationDate, sortColumn, sortOrder });
        }

        [HttpPost]
        public async Task<IActionResult> SFLDeleteAllCompanies(int followListId, DateTime recommendationDate, string sortColumn, string sortOrder)
        {
            await _followListService.SFLDeleteAllCompaniesAsync(followListId);
            return RedirectToAction("SFLIndex", new { followListId, recommendationDate, sortColumn, sortOrder });
        }

        public IActionResult SFLSort(string sortColumn, int followListId, DateTime recommendationDate, string currentSortOrder)
        {
            string sortOrder = currentSortOrder == "asc" ? "desc" : "asc";
            return RedirectToAction("SFLIndex", new { followListId, recommendationDate, sortColumn, sortOrder });
        }

        private int GetCurrentUserId()
        {
            // Implement user ID retrieval (e.g., from claims)
            return 1; // Placeholder
        }
    }
}