﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models;
using StockSystem2025.Services;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace StockSystem2025.Controllers
{
    public class RecommendationsController : Controller
    {
        private readonly RecommendationsStockService _stockService;
        private readonly StockdbContext _context;
        public RecommendationsController(RecommendationsStockService stockService, StockdbContext context)
        {
            _stockService = stockService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Result(List<string>?    CompaniesSticer, int? id, DateTime? date, int viewIndex = 1, string sortColumn = "Sticker", string sortOrder = "ASC")
        {
            var model = await _stockService.GetRecommendationResultAsync(id, date, viewIndex, sortColumn, sortOrder, CompaniesSticer);
            return View(model);
        }

        public async Task<IActionResult> Result(string? CompaniesSticer, int? id, DateTime? date, int viewIndex = 1, string sortColumn = "Sticker", string sortOrder = "ASC")
        {
            List<string>? Companiesplited = null;
            if (CompaniesSticer != null)
                Companiesplited= CompaniesSticer.Split(",").ToList();

            

                var model = await _stockService.GetRecommendationResultAsync(id, date, viewIndex, sortColumn, sortOrder, Companiesplited);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddToFollowList(int followListId, string companyCode, int? criteriaId, DateTime? date, int viewIndex, string sortColumn, string sortOrder)
        {
            await _stockService.AddToFollowListAsync(followListId, companyCode);
            return RedirectToAction("Result", new { id = criteriaId, date, viewIndex, sortColumn, sortOrder });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeDate(DateTime SelectedDate, int? criteriaId, int viewIndex, string sortColumn, string sortOrder, List<string>? CompaniesSticer)
        {
            if (await _stockService.CheckIfDateExistsAsync(SelectedDate))
            {
                return RedirectToAction("Result", new { id = criteriaId, date = SelectedDate, viewIndex, sortColumn, sortOrder, CompaniesSticer });
            }
            var model = await _stockService.GetRecommendationResultAsync(criteriaId, null, viewIndex, sortColumn, sortOrder, CompaniesSticer);
            model.ShowError = true;
            return View("Result", model);
        }

        [HttpPost]
        public async Task<IActionResult> NextDate(DateTime? currentDate, int? criteriaId, int viewIndex, string sortColumn, string sortOrder,List<string>?    CompaniesSticer)
        {
            var nextDate = await _stockService.GetNextDateAsync(currentDate);
            if (nextDate.HasValue && await _stockService.CheckIfDateExistsAsync(nextDate.Value))
            {
                return RedirectToAction("Result", new { id = criteriaId, date = nextDate, viewIndex, sortColumn, sortOrder });
            }
            var model = await _stockService.GetRecommendationResultAsync(criteriaId, currentDate, viewIndex, sortColumn, sortOrder, CompaniesSticer);
            model.ShowError = true;
            return View("Result", model);
        }

        [HttpPost]
        public async Task<IActionResult> PreviousDate(DateTime? currentDate, int? criteriaId, int viewIndex, string sortColumn, string sortOrder, List<string>? CompaniesSticer)
        {
            var prevDate = await _stockService.GetPreviousDateAsync(currentDate);
            if (prevDate.HasValue && await _stockService.CheckIfDateExistsAsync(prevDate.Value))
            {
                return RedirectToAction("Result", new { id = criteriaId, date = prevDate, viewIndex, sortColumn, sortOrder });
            }
            var model = await _stockService.GetRecommendationResultAsync(criteriaId, currentDate, viewIndex, sortColumn, sortOrder, CompaniesSticer);
            model.ShowError = true;
            return View("Result", model);
        }

        [HttpPost]
        public async Task<IActionResult> SwitchView(int? CriteriaId, DateTime? SelectedDate, int ViewIndex, string SortColumn, string sortOrder, List<string>? CompaniesSticer)
        {
            int newViewIndex = ViewIndex == 0 ? 1 : 0;
            return RedirectToAction("Result", new { id = CriteriaId, SelectedDate, viewIndex = ViewIndex, SortColumn, sortOrder, CompaniesSticer });
        }


        public IActionResult IndexChart(string? sticer)
        {
            int parsedNumber;
            bool isNumeric = int.TryParse(sticer, out parsedNumber);

            if (!isNumeric)
            {

                var companis = _context.CompanyTables.Where(v => v.ParentIndicator == sticer).Select(c => c.CompanyCode).ToList();

                var stickers = _context.StockTables.Where(c => companis.Contains(c.Sticker))
                    .Select(s => s.Sname)
                    .Distinct()
                    .ToList();

                ViewBag.Stickers = stickers;
                return View();

            }
            else
            {
                var stickers = _context.StockTables.Where(c => sticer == null || c.Sticker == sticer)
                     .Select(s => s.Sname)
                     .Distinct()
                     .ToList();
                ViewBag.Stickers = stickers;
                return View();

            }



        }

        [HttpGet]
        public async Task<IActionResult> GetStockData(string sticker, string timeframe = "1D")
        {
            if (string.IsNullOrEmpty(sticker))
            {
                return BadRequest(new { success = false, message = "يرجى اختيار رمز سهم صالح." });
            }

            var stockDataQuery = _context.StockTables
                .Where(s => s.Sname == sticker)
                .OrderBy(s => s.Sdate)
                .Select(s => new
                {
                    time = s.Sdate,
                    open = s.Sopen,
                    high = s.Shigh,
                    low = s.Slow,
                    close = s.Sclose,
                    volume = s.Svol
                });

            var stockData = await stockDataQuery.ToListAsync();

            if (!stockData.Any())
            {
                return Json(new { success = false, message = "لا توجد بيانات لرمز السهم المختار." });
            }

            var groupedData = stockData
                .Select(d => new
                {
                    date = DateTime.TryParse(d.time, out var date) ? date : DateTime.MinValue,
                    open = d.open,
                    high = d.high,
                    low = d.low,
                    close = d.close,
                    volume = d.volume
                })
                .Where(d => d.date != DateTime.MinValue)
                .ToList();

            if (!groupedData.Any())
            {
                return Json(new { success = false, message = "تنسيق التاريخ غير صحيح." });
            }

            var latestDate = groupedData.Max(d => d.date);
            var aggregatedData = new List<object>();
            switch (timeframe.ToUpper())
            {
                case "1D":
                    aggregatedData = groupedData
                        .Where(d => d.date >= latestDate.Date)
                        .Select(d => new
                        {
                            time = new DateTimeOffset(d.date).ToUnixTimeSeconds(),
                            open = d.open,
                            high = d.high,
                            low = d.low,
                            close = d.close,
                            volume = d.volume,
                            moneyFlow = ((d.high + d.low + d.close) / 3) * d.volume
                        })
                        .ToList<object>();
                    break;

                case "1W":
                    aggregatedData = groupedData
                        .Where(d => d.date >= latestDate.AddDays(-7))
                        .Select(d => new
                        {
                            time = new DateTimeOffset(d.date).ToUnixTimeSeconds(),
                            open = d.open,
                            high = d.high,
                            low = d.low,
                            close = d.close,
                            volume = d.volume,
                            moneyFlow = ((d.high + d.low + d.close) / 3) * d.volume
                        })
                        .ToList<object>();
                    break;

                case "2W":
                    aggregatedData = groupedData
                        .Where(d => d.date >= latestDate.AddDays(-14))
                        .Select(d => new
                        {
                            time = new DateTimeOffset(d.date).ToUnixTimeSeconds(),
                            open = d.open,
                            high = d.high,
                            low = d.low,
                            close = d.close,
                            volume = d.volume,
                            moneyFlow = ((d.high + d.low + d.close) / 3) * d.volume
                        })
                        .ToList<object>();
                    break;

                case "1M":
                    aggregatedData = groupedData
                        .Where(d => d.date >= latestDate.AddMonths(-1))
                        .Select(d => new
                        {
                            time = new DateTimeOffset(d.date).ToUnixTimeSeconds(),
                            open = d.open,
                            high = d.high,
                            low = d.low,
                            close = d.close,
                            volume = d.volume,
                            moneyFlow = ((d.high + d.low + d.close) / 3) * d.volume
                        })
                        .ToList<object>();
                    break;

                case "2M":
                    aggregatedData = groupedData
                        .Where(d => d.date >= latestDate.AddMonths(-2))
                        .Select(d => new
                        {
                            time = new DateTimeOffset(d.date).ToUnixTimeSeconds(),
                            open = d.open,
                            high = d.high,
                            low = d.low,
                            close = d.close,
                            volume = d.volume,
                            moneyFlow = ((d.high + d.low + d.close) / 3) * d.volume
                        })
                        .ToList<object>();
                    break;

                case "6M":
                    aggregatedData = groupedData
                        .Where(d => d.date >= latestDate.AddMonths(-6))
                        .Select(d => new
                        {
                            time = new DateTimeOffset(d.date).ToUnixTimeSeconds(),
                            open = d.open,
                            high = d.high,
                            low = d.low,
                            close = d.close,
                            volume = d.volume,
                            moneyFlow = ((d.high + d.low + d.close) / 3) * d.volume
                        })
                        .ToList<object>();
                    break;

                case "1Y":
                    aggregatedData = groupedData
                        .Where(d => d.date >= latestDate.AddYears(-1))
                        .Select(d => new
                        {
                            time = new DateTimeOffset(d.date).ToUnixTimeSeconds(),
                            open = d.open,
                            high = d.high,
                            low = d.low,
                            close = d.close,
                            volume = d.volume,
                            moneyFlow = ((d.high + d.low + d.close) / 3) * d.volume
                        })
                        .ToList<object>();
                    break;

                case "2Y":
                    aggregatedData = groupedData
                        .Where(d => d.date >= latestDate.AddYears(-2))
                        .Select(d => new
                        {
                            time = new DateTimeOffset(d.date).ToUnixTimeSeconds(),
                            open = d.open,
                            high = d.high,
                            low = d.low,
                            close = d.close,
                            volume = d.volume,
                            moneyFlow = ((d.high + d.low + d.close) / 3) * d.volume
                        })
                        .ToList<object>();
                    break;

                case "6Y":
                    aggregatedData = groupedData
                        .Where(d => d.date >= latestDate.AddYears(-6))
                        .Select(d => new
                        {
                            time = new DateTimeOffset(d.date).ToUnixTimeSeconds(),
                            open = d.open,
                            high = d.high,
                            low = d.low,
                            close = d.close,
                            volume = d.volume,
                            moneyFlow = ((d.high + d.low + d.close) / 3) * d.volume
                        })
                        .ToList<object>();
                    break;

                case "ALL":
                    aggregatedData = groupedData
                        .Select(d => new
                        {
                            time = new DateTimeOffset(d.date).ToUnixTimeSeconds(),
                            open = d.open,
                            high = d.high,
                            low = d.low,
                            close = d.close,
                            volume = d.volume,
                            moneyFlow = ((d.high + d.low + d.close) / 3) * d.volume
                        })
                        .ToList<object>();
                    break;
            }

            return Json(new { success = true, data = aggregatedData });
        }



    }
}