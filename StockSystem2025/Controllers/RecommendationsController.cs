using Microsoft.AspNetCore.Mvc;
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
                var companies = _context.CompanyTables
                    .Where(v => v.ParentIndicator == sticer)
                    .Select(c => c.CompanyCode)
                    .ToList();

                var stickers = _context.StockTables
                    .Where(c => companies.Contains(c.Sticker))
                    .Select(s => s.Sname)
                    .Distinct()
                    .ToList();

                ViewBag.Stickers = stickers;
                return View();
            }
            else
            {
                var stickers = _context.StockTables
                    .Where(c => sticer == null || c.Sticker == sticer)
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
            timeframe = "1D";
            Debug.WriteLine($"Fetching data for sticker: {sticker}, timeframe: {timeframe}");

            if (string.IsNullOrEmpty(sticker))
            {
                Debug.WriteLine("Error: Sticker is null or empty");
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
            Debug.WriteLine($"Retrieved {stockData.Count} data points from database");

            if (!stockData.Any())
            {
                Debug.WriteLine("Error: No data found for the selected sticker");
                return Json(new { success = false, message = "لا توجد بيانات لرمز السهم المختار." });
            }

            var groupedData = stockData
                .Select(d => new
                {
                    date = DateTime.TryParse(d.time, out var date) ? date : DateTime.MinValue,
                    open = d.open.HasValue ? d.open.Value : 0,
                    high = d.high.HasValue ? d.high.Value : 0,
                    low = d.low.HasValue ? d.low.Value : 0,
                    close = d.close.HasValue ? d.close.Value : 0,
                    volume = d.volume.HasValue ? d.volume.Value : 0
                })
                .Where(d => d.date != DateTime.MinValue && d.close > 0)
                .OrderBy(d => d.date)
                .ToList();

            Debug.WriteLine($"Filtered to {groupedData.Count} valid data points");

            if (!groupedData.Any())
            {
                Debug.WriteLine("Error: No valid data after filtering");
                return Json(new { success = false, message = "تنسيق التاريخ أو البيانات غير صحيح." });
            }

            // Apply timeframe filtering first
            var latestDate = groupedData.Max(d => d.date);
            var filteredData = new List<object>();
            switch (timeframe.ToUpper())
            {
                case "1D":
                    filteredData = groupedData
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
                    filteredData = groupedData
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
                    filteredData = groupedData
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
                    filteredData = groupedData
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
                    filteredData = groupedData
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
                    filteredData = groupedData
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
                    filteredData = groupedData
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
                    filteredData = groupedData
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
                    filteredData = groupedData
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
                    filteredData = groupedData
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

            // Filter groupedData to match the timeframe for signal generation
            DateTime minDate;
            switch (timeframe.ToUpper())
            {
                case "1D": minDate = latestDate.Date; break;
                case "1W": minDate = latestDate.AddDays(-7); break;
                case "2W": minDate = latestDate.AddDays(-14); break;
                case "1M": minDate = latestDate.AddMonths(-1); break;
                case "2M": minDate = latestDate.AddMonths(-2); break;
                case "6M": minDate = latestDate.AddMonths(-6); break;
                case "1Y": minDate = latestDate.AddYears(-1); break;
                case "2Y": minDate = latestDate.AddYears(-2); break;
                case "6Y": minDate = latestDate.AddYears(-6); break;
                case "ALL": minDate = DateTime.MinValue; break;
                default: minDate = DateTime.MinValue; break;
            }

            var timeframeData = groupedData
                .Where(d => d.date >= minDate)
                .ToList();

            // Generate signals only for the filtered timeframe data
            var signals = new List<object>();
            var signalHistory = new Dictionary<DateTime, string>(); // لتخزين إشارات الأيام السابقة
            if (timeframeData.Count >= 5) // Need 5 days for breakout lookback
            {
                Debug.WriteLine("Starting signal generation with price breakout strategy");

                for (int i = 4; i <= timeframeData.Count - 1; i++)
                {
                    var currentClose = timeframeData[i].close;
                    var currentVolume = timeframeData[i].volume;
                    var currentDate = timeframeData[i].date;
                    var lookbackData = timeframeData.Skip(i - 5).Take(5).ToList();
                    var highestClose = lookbackData.Max(d => d.close);
                    var lowestClose = lookbackData.Min(d => d.close);
                    var previousData = timeframeData.Take(i).LastOrDefault(d => d.date < currentDate);
                    var previousClose = previousData?.close ?? 0;

                    bool validVolume = currentVolume > 0;
                    bool buySignal = currentClose > highestClose;
                    bool sellSignal = currentClose < lowestClose;
                    bool isCloseToPrevious = previousClose > 0 && Math.Abs(currentClose - previousClose) / previousClose < 0.01; // أقل من 1%

                    // التحقق مما إذا كان اليوم السابق لديه إشارة "buy"
                    bool previousDayWasBuy = false;
                    if (previousData != null)
                    {
                        signalHistory.TryGetValue(previousData.date, out string previousSignal);
                        previousDayWasBuy = previousSignal == "buy";
                    }

                    string signalType = null;
                    if ((buySignal && validVolume) || (isCloseToPrevious && validVolume && previousDayWasBuy))
                    {
                        signalType = "buy";
                    }
                    else if (sellSignal && validVolume)
                    {
                        signalType = "sell";
                    }

                    Debug.WriteLine($"Index {i}: Date={currentDate}, Close={currentClose}, PreviousClose={previousClose}, HighestClose={highestClose}, LowestClose={lowestClose}, Volume={currentVolume}, BuySignal={buySignal}, SellSignal={sellSignal}, IsCloseToPrevious={isCloseToPrevious}, PreviousDayWasBuy={previousDayWasBuy}, ValidVolume={validVolume}, Signal={signalType ?? "None"}");

                    if (signalType != null)
                    {
                        Debug.WriteLine($"{signalType.ToUpper()} Signal at Index {i}: Date={currentDate}, Close={currentClose}");
                        signals.Add(new
                        {
                            time = new DateTimeOffset(currentDate).ToUnixTimeSeconds(),
                            signal = signalType
                        });
                        // تخزين الإشارة في signalHistory
                        signalHistory[currentDate] = signalType;
                    }
                }

                Debug.WriteLine($"Generated {signals.Count} Price Breakout signals");
            }
            else
            {
                Debug.WriteLine("Not enough data for Price Breakout signal generation (need at least 5 data points)");
            }



            // Simulate trading strategy to calculate success rate, profit percentage, total profit, and total trades
            double successRate = 0.0;
            double profitPercentage = 0.0;
            double totalProfit = 0.0;
            int totalTrades = 0;
            double initialCapital = 2000.0;
            double currentCapital = initialCapital;

            if (signals.Any())
            {
                var trades = new List<(long EntryTime, double EntryPrice, long ExitTime, double ExitPrice, double Profit, bool Successful, double CapitalUsed)>();
                (long Time, string Signal)? currentTrade = null;

                // Convert signals to list for processing
                var signalList = signals.Select(s => new
                {
                    Time = (long)s.GetType().GetProperty("time").GetValue(s),
                    Signal = (string)s.GetType().GetProperty("signal").GetValue(s)
                }).OrderBy(s => s.Time).ToList();

                // Convert filteredData to dictionary for price lookup
                var priceDict = filteredData.Select(d => new
                {
                    Time = (long)d.GetType().GetProperty("time").GetValue(d),
                    Close = (double)d.GetType().GetProperty("close").GetValue(d)
                }).ToDictionary(d => d.Time, d => d.Close);

                // Process signals to simulate trades
                for (int i = 0; i < signalList.Count; i++)
                {
                    var signal = signalList[i];

                    if (signal.Signal == "buy" && currentTrade == null)
                    {
                        // Start new trade with current capital
                        if (priceDict.ContainsKey(signal.Time))
                        {
                            currentTrade = (signal.Time, signal.Signal);
                        }
                    }
                    else if (currentTrade != null)
                    {
                        // Check if the current day has a "buy" signal; if not, exit
                        bool isBuySignal = signal.Signal == "buy";
                        if (!isBuySignal)
                        {
                            if (priceDict.ContainsKey(signal.Time) && priceDict.ContainsKey(currentTrade.Value.Time))
                            {
                                double entryPrice = priceDict[currentTrade.Value.Time];
                                double exitPrice = priceDict[signal.Time];
                                double shares = currentCapital / entryPrice;
                                double profit = (exitPrice - entryPrice) * shares;
                                bool isSuccessful = exitPrice > entryPrice;

                                // Update current capital
                                currentCapital += profit;

                                // Ensure capital doesn't go negative
                                if (currentCapital < 0) currentCapital = 0;

                                trades.Add((currentTrade.Value.Time, entryPrice, signal.Time, exitPrice, profit, isSuccessful, currentCapital));
                                currentTrade = null;
                            }
                        }
                    }
                }

                // Handle open trade at the end
                if (currentTrade != null && priceDict.ContainsKey(currentTrade.Value.Time))
                {
                    double entryPrice = priceDict[currentTrade.Value.Time];
                    // Use the last available price as exit price
                    var lastData = filteredData.Last();
                    var lastTime = (long)lastData.GetType().GetProperty("time").GetValue(lastData);
                    var lastPrice = (double)lastData.GetType().GetProperty("close").GetValue(lastData);
                    double shares = currentCapital / entryPrice;
                    double profit = (lastPrice - entryPrice) * shares;
                    bool isSuccessful = lastPrice > entryPrice;

                    // Update current capital
                    currentCapital += profit;

                    // Ensure capital doesn't go negative
                    if (currentCapital < 0) currentCapital = 0;

                    trades.Add((currentTrade.Value.Time, entryPrice, lastTime, lastPrice, profit, isSuccessful, currentCapital));
                }

                // Calculate success rate, profit percentage, total profit, and total trades
                if (trades.Any())
                {
                    int successfulTrades = trades.Count(t => t.Successful);
                    totalTrades = trades.Count;
                    successRate = (double)successfulTrades / totalTrades * 100;
                    totalProfit = trades.Sum(t => t.Profit);
                    profitPercentage = (totalProfit / initialCapital) * 100;

                    Debug.WriteLine($"Total Trades: {totalTrades}, Successful Trades: {successfulTrades}");
                    Debug.WriteLine($"Success Rate: {successRate:F2}%");
                    Debug.WriteLine($"Total Profit: {totalProfit:F2} JOD");
                    Debug.WriteLine($"Profit Percentage: {profitPercentage:F2}%");
                    Debug.WriteLine($"Final Capital: {currentCapital:F2} JOD");
                }
            }

            Debug.WriteLine($"Returning {filteredData.Count} data points");
            return Json(new { success = true, data = filteredData, signals, successRate, profitPercentage, totalProfit, totalTrades });
        }




    }
}