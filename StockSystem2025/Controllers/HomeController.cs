using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models;
using StockSystem2025.ViewModel;

namespace StockSystem2025.Controllers
{
    public class HomeController : Controller
    {
        private readonly StockdbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, StockdbContext context)
        {
            _logger = logger;
            _context = context;
        }
     public IActionResult IndexWep()
        {
            return View();
        }
        
        public IActionResult Index()
        {
            return View();
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