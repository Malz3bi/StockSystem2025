using System.Diagnostics;
using System.Net;
using System.Text.Json;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Retry;
using StockSystem2025.Models;
using StockSystem2025.ViewModel;

namespace StockSystem2025.Controllers
{
    public class HomeController : Controller
    {
        private readonly StockdbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _cache;

        public HomeController(ILogger<HomeController> logger, StockdbContext context, IMemoryCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
        }
        public IActionResult IndexWep()
        {
            return View();
        }
        
        public IActionResult Index()
        {
            return View();
        }


        // جلب بيانات الأسهم لجميع الشركات وحفظها في قاعدة البيانات
        [HttpPost]
        public async Task<IActionResult> FetchAndSaveAllCompaniesStocks()
        {
            try
            {
                // جلب جميع الشركات من CompanyTable (غير المؤشرات)
                var companies = await _context.CompanyTables
                    .Where(c => !c.IsIndicator)
                    .Select(c => new { c.CompanyCode, c.CompanyName })
                    .ToListAsync();

                if (!companies.Any())
                {
                    _logger.LogWarning("لم يتم العثور على شركات في جدول CompanyTable.");
                    return BadRequest(new { success = false, message = "لم يتم العثور على شركات لجلب بياناتها.", failedCompanies = new List<object>() });
                }

                int totalCompanies = companies.Count;
                int processedCompanies = 0;
                int totalRecordsSaved = 0;
                var failedCompanies = new List<object>();

                // سياسة إعادة المحاولة للتعامل مع خطأ 429
                AsyncRetryPolicy retryPolicy = Policy
                    .Handle<HttpRequestException>(ex => ex.StatusCode == HttpStatusCode.TooManyRequests)
                    .WaitAndRetryAsync(
                        retryCount: 5,
                        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * 10),
                        onRetry: (exception, timeSpan, retryCount, context) =>
                        {
                            _logger.LogWarning($"Retry {retryCount}/5 after {timeSpan.TotalSeconds} seconds due to: {exception.Message}");
                        });

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");

                foreach (var company in companies)
                {
                    try
                    {
                        // إضافة .SR إلى CompanyCode إذا لم يكن موجودًا
                        string sticker = company.CompanyCode.EndsWith(".SR") ? company.CompanyCode : $"{company.CompanyCode}.SR";
                        string cacheKey = $"StockData_{sticker}";
                        List<DailyStockData> stockDataList;

                        // التحقق من الذاكرة المؤقتة
                        if (!_cache.TryGetValue(cacheKey, out stockDataList) || stockDataList == null || !stockDataList.Any())
                        {
                            // جلب البيانات من Yahoo Finance
                            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{sticker}?interval=1d&range=1mo";
                            _logger.LogInformation($"جلب البيانات للشركة {sticker} من {url}");

                            var response = await retryPolicy.ExecuteAsync(async () =>
                            {
                                var resp = await client.GetAsync(url);
                                resp.EnsureSuccessStatusCode();
                                return resp;
                            });

                            var responseBody = await response.Content.ReadAsStringAsync();
                            var json = JsonDocument.Parse(responseBody);

                            // التحقق من وجود بيانات في الاستجابة
                            if (!json.RootElement.TryGetProperty("chart", out var chart) ||
                                !chart.TryGetProperty("result", out var result) ||
                                result.GetArrayLength() == 0)
                            {
                                _logger.LogWarning($"لا توجد بيانات متاحة من Yahoo Finance لـ {sticker}.");
                                failedCompanies.Add(new { CompanyCode = company.CompanyCode, CompanyName = company.CompanyName, Reason = "استجابة فارغة من Yahoo Finance" });
                                continue;
                            }

                            var root = result[0];
                            if (!root.TryGetProperty("timestamp", out var timestamps) || timestamps.GetArrayLength() == 0)
                            {
                                _logger.LogWarning($"لا توجد تواريخ متاحة في استجابة {sticker}.");
                                failedCompanies.Add(new { CompanyCode = company.CompanyCode, CompanyName = company.CompanyName, Reason = "لا توجد تواريخ في الاستجابة" });
                                continue;
                            }

                            var quotes = root.GetProperty("indicators").GetProperty("quote")[0];
                            var opens = quotes.GetProperty("open");
                            var highs = quotes.GetProperty("high");
                            var lows = quotes.GetProperty("low");
                            var closes = quotes.GetProperty("close");
                            var volumes = quotes.GetProperty("volume");

                            stockDataList = new List<DailyStockData>();

                            for (int i = 0; i < timestamps.GetArrayLength(); i++)
                            {
                                var date = DateTimeOffset.FromUnixTimeSeconds(timestamps[i].GetInt64()).DateTime;

                                decimal? open = opens[i].ValueKind != JsonValueKind.Null ? opens[i].GetDecimal() : null;
                                decimal? high = highs[i].ValueKind != JsonValueKind.Null ? highs[i].GetDecimal() : null;
                                decimal? low = lows[i].ValueKind != JsonValueKind.Null ? lows[i].GetDecimal() : null;
                                decimal? close = closes[i].ValueKind != JsonValueKind.Null ? closes[i].GetDecimal() : null;
                                long? volume = volumes[i].ValueKind != JsonValueKind.Null ? volumes[i].GetInt64() : null;

                                stockDataList.Add(new DailyStockData
                                {
                                    Date = date,
                                    Open = open,
                                    High = high,
                                    Low = low,
                                    Close = close,
                                    Volume = volume
                                });
                            }

                            if (!stockDataList.Any())
                            {
                                _logger.LogWarning($"لا توجد بيانات أسهم لـ {sticker} بعد المعالجة.");
                                failedCompanies.Add(new { CompanyCode = company.CompanyCode, CompanyName = company.CompanyName, Reason = "لا توجد بيانات أسهم بعد المعالجة" });
                                continue;
                            }

                            // تخزين البيانات في الذاكرة المؤقتة
                            var cacheOptions = new MemoryCacheEntryOptions()
                                .SetSlidingExpiration(TimeSpan.FromHours(1));
                            _cache.Set(cacheKey, stockDataList, cacheOptions);
                        }

                        // تحويل البيانات إلى StockTable
                        var stockRecords = stockDataList.Select(data => new StockTable
                        {
                            Sticker = company.CompanyCode, // استخدام CompanyCode بدون .SR
                            Sname = company.CompanyName ?? "غير متوفر",
                            Sdate = data.Date.ToString("yyyy/MM/dd"),
                            Sopen = Convert.ToDouble(data.Open ?? 0),
                            Shigh = Convert.ToDouble(data.High ?? 0),
                            Slow = Convert.ToDouble(data.Low ?? 0),
                            Sclose = Convert.ToDouble(data.Close ?? 0),
                            Svol = Convert.ToDouble(data.Volume ?? 0),
                            Createddate = data.Date.Date, // التأكد من إزالة الوقت
                            DayNo = 0, // سيتم تحديثه لاحقًا
                            ExpectedOpen = null
                        }).ToList();

                        // جلب السجلات الموجودة لتجنب التكرار
                        var dates = stockDataList.Select(d => d.Date.Date).ToList(); // إزالة الوقت من التواريخ
                        var existingStocks = await _context.StockTables
                            .Where(s => s.Sticker == company.CompanyCode && dates.Contains(s.Createddate.Value))
                            .Select(s => new { s.Sticker, s.Sdate })
                            .ToListAsync();
                        var stockLookup = new HashSet<string>(existingStocks.Select(s => $"{s.Sticker}|{s.Sdate}"));

                        // تصفية السجلات الجديدة فقط
                        var newRecords = stockRecords
                            .Where(r => !stockLookup.Contains($"{r.Sticker}|{r.Sdate}"))
                            .ToList();

                        if (newRecords.Any())
                        {
                            // حفظ السجلات باستخدام BulkInsert
                            _context.Database.SetCommandTimeout(120);
                            await _context.BulkInsertAsync(newRecords);
                            totalRecordsSaved += newRecords.Count;
                            _logger.LogInformation($"تم حفظ {newRecords.Count} سجل للشركة {sticker}.");
                        }
                        else
                        {
                            _logger.LogInformation($"لا توجد بيانات جديدة للحفظ للشركة {sticker}.");
                        }

                        // إعادة ترتيب DayNo
                        await SortStockTables(company.CompanyCode);

                        processedCompanies++;
                        _logger.LogInformation($"تم معالجة {processedCompanies}/{totalCompanies} شركات: {sticker}");
                    }
                    catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        _logger.LogWarning($"خطأ 429 للشركة {company.CompanyCode}. سيتم تخطي هذه الشركة.");
                        failedCompanies.Add(new { CompanyCode = company.CompanyCode, CompanyName = company.CompanyName, Reason = "خطأ 429: عدد الطلبات كبير جدًا" });
                        continue;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"خطأ أثناء معالجة الشركة {company.CompanyCode}: {ex.Message}");
                        failedCompanies.Add(new { CompanyCode = company.CompanyCode, CompanyName = company.CompanyName, Reason = $"خطأ: {ex.Message}" });
                        continue;
                    }
                }

                string message = $"تم معالجة {processedCompanies} شركة وحفظ {totalRecordsSaved} سجل بنجاح.";
                if (failedCompanies.Any())
                {
                    message += $" فشل جلب بيانات {failedCompanies.Count} شركة.";
                }

                return Ok(new
                {
                    success = true,
                    message,
                    processedCompanies,
                    totalRecordsSaved,
                    failedCompanies
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ عام أثناء جلب وحفظ بيانات الأسهم.");
                return StatusCode(500, new { success = false, message = $"حدث خطأ: {ex.Message}", failedCompanies = new List<object>() });
            }
        }

        // إعادة ترتيب DayNo للأسهم بناءً على Sticker
        private async Task SortStockTables(string sticker)
        {
            try
            {
                var stocks = await _context.StockTables
                    .Where(x => x.Sticker == sticker)
                    .OrderByDescending(x => x.Sdate)
                    .ToListAsync();

                int dayNo = 1;
                foreach (var stock in stocks)
                {
                    stock.DayNo = dayNo++;
                }

                if (stocks.Any())
                {
                    _logger.LogInformation($"تحديث {stocks.Count} سجل لـ DayNo للشركة {sticker}.");
                    await _context.BulkUpdateAsync(stocks);
                }
                else
                {
                    _logger.LogInformation($"لا توجد سجلات لتحديث DayNo للشركة {sticker}.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطأ أثناء إعادة ترتيب DayNo للشركة {sticker}.");
                throw;
            }
        }


        public class DailyStockData
        {
            public DateTime Date { get; set; }
            public decimal? Open { get; set; }
            public decimal? High { get; set; }
            public decimal? Low { get; set; }
            public decimal? Close { get; set; }
            public long? Volume { get; set; }
        }


        public async Task<IActionResult> breakout()
        {
            var viewModel = new HistoricalPeaksViewModel();
            double breakoutThreshold = 0.05; // عتبة الاختراق السفلى 5%
            double upperThreshold = 0.05; // عتبة الاختراق العليا 5%
            int windowDays = 90; // 3 أشهر ≈ 90 يومًا

            // جلب جميع الشركات الفريدة
            var stickers = await _context.CompanyTables.Where(x=> !x.IsIndicator)
                .Select(s => s.CompanyCode)
                .Distinct()
                .ToListAsync();

            var breakoutCompanies = new List<BreakoutCompany>();

            foreach (var stk in stickers)
            {
                // جلب بيانات السهم مع Sname
                var stockDataRaw = await _context.StockTables
                    .Where(s => s.Sticker == stk && s.Shigh.HasValue)
                    .Select(s => new { s.Sdate, High = s.Shigh.Value, s.Sname })
                    .ToListAsync();

                try
                {
                    var stockData = stockDataRaw
                        .Select(s => new { Date = DateTime.Parse(s.Sdate), High = s.High, Name = s.Sname })
                        .OrderBy(s => s.Date)
                        .ToList();

                    if (stockData.Count < windowDays * 2 + 1) // التأكد من وجود بيانات كافية
                        continue;

                    // العثور على القمم التاريخية غير المخترقة
                    var peaks = new List<HistoricalPeak>();
                    for (int i = windowDays; i < stockData.Count - windowDays; i++)
                    {
                        var current = stockData[i];
                        bool isLocalPeak = true;
                        bool isUnbroken = true;
                        bool isNotBreached = true;

                        // التحقق من القمة المحلية (±90 يومًا)
                        for (int j = i - windowDays; j <= i + windowDays; j++)
                        {
                            if (j != i && stockData[j].High >= current.High)
                            {
                                isLocalPeak = false;
                                break;
                            }
                        }

                        // التحقق من عدم الاختراق في النافذة (+90 يومًا)
                        if (isLocalPeak)
                        {
                            for (int j = i + 1; j <= i + windowDays && j < stockData.Count; j++)
                            {
                                if (stockData[j].High > current.High)
                                {
                                    isUnbroken = false;
                                    break;
                                }
                            }
                        }

                        // التحقق من أن القمة لم تُخترق (لا يوجد Shigh يتجاوز القمة بأكثر من 5% بعد تاريخها)
                        if (isLocalPeak && isUnbroken)
                        {
                            for (int j = i + 1; j < stockData.Count; j++)
                            {
                                if (stockData[j].High > current.High * (1 + upperThreshold))
                                {
                                    isNotBreached = false;
                                    break;
                                }
                            }
                        }

                        if (isLocalPeak && isUnbroken && isNotBreached)
                        {
                            peaks.Add(new HistoricalPeak { Date = current.Date, HighPrice = current.High });
                        }
                    }

                    if (!peaks.Any())
                        continue;

                    // جلب السعر الحالي (آخر Shigh) واسم الشركة
                    var currentPrice = stockData.Last().High;
                    var companyName = stockData.Last().Name ?? "غير متوفر";

                    // إيجاد أحدث قمة تاريخية غير مخترقة
                    var latestPeak = peaks.OrderByDescending(p => p.Date).FirstOrDefault();

                    // اختيار القمة المناسبة
                    HistoricalPeak selectedPeak = null;
                    if (latestPeak != null)
                    {
                        // تحقق مما إذا كان السعر الحالي في النطاق المطلوب مقارنة بأحدث قمة
                        var lowerBound = latestPeak.HighPrice * (1 - breakoutThreshold); // 95% من القمة
                        var upperBound = latestPeak.HighPrice * (1 + upperThreshold); // 105% من القمة
                        if (currentPrice >= lowerBound && currentPrice <= upperBound)
                        {
                            selectedPeak = latestPeak;
                        }
                        else
                        {
                            // إذا كان السعر الحالي أعلى من الحد الأعلى، تحقق من الفرق
                            if (currentPrice > upperBound)
                            {
                                var latestPeakDiff = (latestPeak.HighPrice - currentPrice) / latestPeak.HighPrice;
                                if (latestPeakDiff <= breakoutThreshold)
                                {
                                    // إذا كانت أحدث قمة ضمن 5% تحت السعر الحالي، لا تبحث عن قمة سابقة
                                    continue;
                                }
                            }

                            // ابحث عن القمة السابقة غير المخترقة في النطاق
                            selectedPeak = peaks
                                .OrderByDescending(p => p.Date)
                                .Skip(1)
                                .FirstOrDefault(p => p != null &&
                                    currentPrice >= p.HighPrice * (1 - breakoutThreshold) &&
                                    currentPrice <= p.HighPrice * (1 + upperThreshold));
                        }
                    }

                    // التحقق من وجود قمة مناسبة
                    if (selectedPeak != null)
                    {
                        // حساب نسبة الاختراق
                        double breakoutPercentage = currentPrice > selectedPeak.HighPrice
                            ? upperThreshold * 100 // +5% إذا كان السعر الحالي أعلى
                            : -breakoutThreshold * 100; // -5% إذا كان السعر الحالي أقل أو يساوي

                        breakoutCompanies.Add(new BreakoutCompany
                        {
                            Sticker = stk,
                            Name = companyName,
                            CurrentPrice = currentPrice,
                            ClosestPeakPrice = selectedPeak.HighPrice,
                            ClosestPeakDate = selectedPeak.Date,
                            BreakoutPercentage = breakoutPercentage
                        });
                    }
                }
                catch (FormatException)
                {
                    _logger.LogWarning($"Invalid date format for Sticker {stk}. Skipping.");
                    continue;
                }
            }

            viewModel.BreakoutCompanies = breakoutCompanies;
            return View( viewModel);
        }




        public async Task<IActionResult> breakoutLow()
        {
            var viewModel = new HistoricalLowsViewModel();
            double breakoutThreshold = 0.05; // عتبة الاختراق السفلى 5%
            double upperThreshold = 0.05; // عتبة الاختراق العليا 5%
            int windowDays = 90; // 3 أشهر ≈ 90 يومًا

            // جلب جميع الشركات الفريدة
            var stickers = await _context.StockTables
                .Select(s => s.Sticker)
                .Distinct()
                .ToListAsync();

            var breakoutCompanies = new List<BreakoutCompanylow>();

            foreach (var stk in stickers)
            {
                // جلب بيانات السهم مع Sname و Slow
                var stockDataRaw = await _context.StockTables
                    .Where(s => s.Sticker == stk && s.Slow.HasValue)
                    .Select(s => new { s.Sdate, Low = s.Slow.Value, s.Sname })
                    .ToListAsync();

                try
                {
                    var stockData = stockDataRaw
                        .Select(s => new { Date = DateTime.Parse(s.Sdate), Low = s.Low, Name = s.Sname })
                        .OrderBy(s => s.Date)
                        .ToList();

                    if (stockData.Count < windowDays * 2 + 1) // التأكد من وجود بيانات كافية
                        continue;

                    // العثور على القيعان التاريخية غير المخترقة
                    var lows = new List<HistoricalLow>();
                    for (int i = windowDays; i < stockData.Count - windowDays; i++)
                    {
                        var current = stockData[i];
                        bool isLocalLow = true;
                        bool isUnbroken = true;
                        bool isNotBreached = true;

                        // التحقق من القاع المحلي (±90 يومًا)
                        for (int j = i - windowDays; j <= i + windowDays; j++)
                        {
                            if (j != i && stockData[j].Low <= current.Low)
                            {
                                isLocalLow = false;
                                break;
                            }
                        }

                        // التحقق من عدم الاختراق في النافذة (+90 يومًا)
                        if (isLocalLow)
                        {
                            for (int j = i + 1; j <= i + windowDays && j < stockData.Count; j++)
                            {
                                if (stockData[j].Low < current.Low)
                                {
                                    isUnbroken = false;
                                    break;
                                }
                            }
                        }

                        // التحقق من أن القاع لم يُخترق (لا يوجد Slow يقل عن القاع بأكثر من 5% بعد تاريخه)
                        if (isLocalLow && isUnbroken)
                        {
                            for (int j = i + 1; j < stockData.Count; j++)
                            {
                                if (stockData[j].Low < current.Low * (1 - upperThreshold))
                                {
                                    isNotBreached = false;
                                    break;
                                }
                            }
                        }

                        if (isLocalLow && isUnbroken && isNotBreached)
                        {
                            lows.Add(new HistoricalLow { Date = current.Date, LowPrice = current.Low });
                        }
                    }

                    if (!lows.Any())
                        continue;

                    // جلب السعر الحالي (آخر Slow) واسم الشركة
                    var currentPrice = stockData.Last().Low;
                    var companyName = stockData.Last().Name ?? "غير متوفر";

                    // إيجاد أحدث قاع تاريخي غير مخترق
                    var latestLow = lows.OrderByDescending(l => l.Date).FirstOrDefault();

                    // اختيار القاع المناسب
                    HistoricalLow selectedLow = null;
                    if (latestLow != null)
                    {
                        // تحقق مما إذا كان السعر الحالي في النطاق المطلوب مقارنة بأحدث قاع
                        var lowerBound = latestLow.LowPrice * (1 - breakoutThreshold); // 95% من القاع
                        var upperBound = latestLow.LowPrice * (1 + upperThreshold); // 105% من القاع
                        if (currentPrice >= lowerBound && currentPrice <= upperBound)
                        {
                            selectedLow = latestLow;
                        }
                        else
                        {
                            // إذا كان السعر الحالي أقل من الحد الأدنى، تحقق من الفرق
                            if (currentPrice < lowerBound)
                            {
                                var latestLowDiff = (currentPrice - latestLow.LowPrice) / latestLow.LowPrice;
                                if (latestLowDiff <= breakoutThreshold)
                                {
                                    // إذا كان أحدث قاع ضمن 5% فوق السعر الحالي، لا تبحث عن قاع سابق
                                    continue;
                                }
                            }

                            // ابحث عن القاع السابق غير المخترق في النطاق
                            selectedLow = lows
                                .OrderByDescending(l => l.Date)
                                .Skip(1)
                                .FirstOrDefault(l => l != null &&
                                    currentPrice >= l.LowPrice * (1 - breakoutThreshold) &&
                                    currentPrice <= l.LowPrice * (1 + upperThreshold));
                        }
                    }

                    // التحقق من وجود قاع مناسب
                    if (selectedLow != null)
                    {
                        // حساب نسبة الاختراق
                        double breakoutPercentage = currentPrice < selectedLow.LowPrice
                            ? upperThreshold * 100 // +5% إذا كان السعر الحالي أقل
                            : -breakoutThreshold * 100; // -5% إذا كان السعر الحالي أعلى أو يساوي

                        breakoutCompanies.Add(new BreakoutCompanylow
                        {
                            Sticker = stk,
                            Name = companyName,
                            CurrentPrice = currentPrice,
                            ClosestLowPrice = selectedLow.LowPrice,
                            ClosestLowDate = selectedLow.Date,
                            BreakoutPercentage = breakoutPercentage
                        });
                    }
                }
                catch (FormatException)
                {
                    _logger.LogWarning($"Invalid date format for Sticker {stk}. Skipping.");
                    continue;
                }
            }

            viewModel.BreakoutCompanies = breakoutCompanies;
            return View( viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }
     

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}