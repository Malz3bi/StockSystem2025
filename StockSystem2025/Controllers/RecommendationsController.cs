using Microsoft.AspNetCore.Mvc;
using StockSystem2025.Models;
using StockSystem2025.Services;
using System;
using System.Threading.Tasks;

namespace StockSystem2025.Controllers
{
    public class RecommendationsController : Controller
    {
        private readonly StockService _stockService;

        public RecommendationsController(StockService stockService)
        {
            _stockService = stockService;
        }

        public async Task<IActionResult> Result(int? id, DateTime? date, int viewIndex = 1, string sortColumn = "Sticker", string sortOrder = "ASC")
        {
            var model = await _stockService.GetRecommendationResultAsync(id, date, viewIndex, sortColumn, sortOrder);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddToFollowList(int followListId, string companyCode, int? criteriaId, DateTime? date, int viewIndex, string sortColumn, string sortOrder)
        {
            await _stockService.AddToFollowListAsync(followListId, companyCode);
            return RedirectToAction("Result", new { id = criteriaId, date, viewIndex, sortColumn, sortOrder });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeDate(DateTime newDate, int? criteriaId, int viewIndex, string sortColumn, string sortOrder)
        {
            if (await _stockService.CheckIfDateExistsAsync(newDate))
            {
                return RedirectToAction("Result", new { id = criteriaId, date = newDate, viewIndex, sortColumn, sortOrder });
            }
            var model = await _stockService.GetRecommendationResultAsync(criteriaId, null, viewIndex, sortColumn, sortOrder);
            model.ShowError = true;
            return View("Result", model);
        }

        [HttpPost]
        public async Task<IActionResult> NextDate(DateTime? currentDate, int? criteriaId, int viewIndex, string sortColumn, string sortOrder)
        {
            var nextDate = await _stockService.GetNextDateAsync(currentDate);
            if (nextDate.HasValue && await _stockService.CheckIfDateExistsAsync(nextDate.Value))
            {
                return RedirectToAction("Result", new { id = criteriaId, date = nextDate, viewIndex, sortColumn, sortOrder });
            }
            var model = await _stockService.GetRecommendationResultAsync(criteriaId, currentDate, viewIndex, sortColumn, sortOrder);
            model.ShowError = true;
            return View("Result", model);
        }

        [HttpPost]
        public async Task<IActionResult> PreviousDate(DateTime? currentDate, int? criteriaId, int viewIndex, string sortColumn, string sortOrder)
        {
            var prevDate = await _stockService.GetPreviousDateAsync(currentDate);
            if (prevDate.HasValue && await _stockService.CheckIfDateExistsAsync(prevDate.Value))
            {
                return RedirectToAction("Result", new { id = criteriaId, date = prevDate, viewIndex, sortColumn, sortOrder });
            }
            var model = await _stockService.GetRecommendationResultAsync(criteriaId, currentDate, viewIndex, sortColumn, sortOrder);
            model.ShowError = true;
            return View("Result", model);
        }

        [HttpPost]
        public async Task<IActionResult> SwitchView(int? CriteriaId, DateTime? SelectedDate, int ViewIndex, string SortColumn, string sortOrder)
        {
            int newViewIndex = ViewIndex == 0 ? 1 : 0;
            return RedirectToAction("Result", new { id = CriteriaId, SelectedDate, viewIndex = ViewIndex, SortColumn, sortOrder });
        }
    }
}