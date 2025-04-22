using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using StockSystem2025.Hubs;
using StockSystem2025.Models;
using StockSystem2025.ViewModel;

using System.Text;


namespace StockSystem2025.Controllers
{
    public class UploadFileController : Controller
    {
        private readonly StockdbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadFileController> _logger;
        private readonly IHubContext<ProgressHub> _progressHub;

        public UploadFileController(StockdbContext context, IWebHostEnvironment environment, ILogger<UploadFileController> logger, IHubContext<ProgressHub> progressHub)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _progressHub = progressHub;
        }

        public IActionResult UploadFileIndex()
        {
            var model = new UploadFileViewModel
            {
                ExistingDates = _context.StockTables.Select(x => x.Createddate).Distinct().ToList(),
                StockData = _context.StockTables.Select(x => x.Sdate).Distinct().OrderByDescending(x => x).Take(10).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFiles(List<IFormFile> files, string processId)
        {
            if (files == null || files.Count == 0 || files.All(f => f.Length == 0))
            {
                _logger.LogWarning("No valid files received in UploadFiles");
                return BadRequest(new { success = false, message = "يرجى اختيار ملف صالح (.txt)." });
            }

            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadPath);

            int counter = 0;
            try
            {
                foreach (var file in files)
                {
                    if (file.Length > 0 && Path.GetExtension(file.FileName).ToLower() == ".txt")
                    {
                        var filePath = Path.Combine(uploadPath, file.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var lines = await System.IO.File.ReadAllLinesAsync(filePath, Encoding.GetEncoding("windows-1256"));
                        counter += await ProcessFileLines(lines, processId, file.FileName);
                        System.IO.File.Delete(filePath);
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid file: {file.FileName}");
                    }
                }

                if (counter == 0)
                {
                    return BadRequest(new { success = false, message = "لم يتم معالجة أي ملفات. تأكد من اختيار ملفات .txt صالحة." });
                }

                await SortStockTables();
                return Ok(new { success = true, message = $"تم تحميل {files.Count(f => f.Length > 0)} ملفات و {counter} أيام تداول بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing uploaded files");
                return StatusCode(500, new { success = false, message = $"حدث خطأ أثناء معالجة الملفات: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearAllData()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM StockTable");
            TempData["ClearMessage"] = "تم مسح جميع بيانات التداول بنجاح";
            HttpContext.Session.Clear();
            return RedirectToAction("UploadFileIndex");
        }

        [HttpPost]
        public async Task<IActionResult> ClearDataBetweenDates([FromForm] UploadFileViewModel model)
        {
            if (!ModelState.IsValid || model.FromDate == null || model.ToDate == null)
            {
                TempData["ClearMessage"] = "يرجى إدخال تواريخ صالحة";
                return RedirectToAction("UploadFileIndex");
            }

            if (model.FromDate > model.ToDate)
            {
                TempData["ClearMessage"] = "تاريخ البدء يجب أن يكون قبل تاريخ الانتهاء";
                return RedirectToAction("UploadFileIndex");
            }

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM StockTable WHERE Createddate >= {0} AND Createddate <= {1}",
                    model.FromDate, model.ToDate);
                await SortStockTables();
                TempData["ClearMessage"] = "تم مسح بيانات التداول بين التاريخين بنجاح";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing data between dates");
                TempData["ClearMessage"] = "حدث خطأ أثناء مسح بيانات التداول";
            }

            return RedirectToAction("UploadFileIndex");
        }

        private async Task<int> ProcessFileLines(string[] lines, string processId, string fileName)
        {
           

            // Fetch all existing stocks and companies once
            var existingStocks = await _context.StockTables
                .Select(x => new { x.Sticker, x.Sdate })
                .ToListAsync();
            var stockLookup = new HashSet<string>(existingStocks.Select(x => $"{x.Sticker}|{x.Sdate}"));

            var existingCompanies = await _context.CompanyTables
                .ToDictionaryAsync(x => x.CompanyCode, x => x);

            var stockRecordsToAdd = new List<StockTable>();
            var companyRecordsToAdd = new List<CompanyTable>();
            var companiesToUpdate = new List<CompanyTable>();
            var processedCompanyCodes = new HashSet<string>(); // Track processed CompanyCodes
            int counter = 0;
            int totalLines = lines.Length;

            // Phase 1: Processing lines (80% of progress)
            const double processingPhasePercentage = 80.0;
            for (int i = 0; i < totalLines; i++)
            {
                try
                {
                    var line = lines[i];
                    var words = line.Split(',');

                    // Parse date (words[3] = yyyymmdd)
                    string sdate = words[3];
                    if (!int.TryParse(sdate.Substring(0, 4), out int year) ||
                        !int.TryParse(sdate.Substring(4, 2), out int month) ||
                        !int.TryParse(sdate.Substring(6, 2), out int day))
                    {
                        _logger.LogWarning($"تنسيق تاريخ غير صالح في السطر {i + 1}: {line}");
                        continue;
                    }
                    var createdDate = new DateTime(year, month, day);

                  

                    string sticker = words[0];
                    sdate = $"{year}/{month:D2}/{day:D2}";

                    // Check if stock exists; skip if it does
                    if (stockLookup.Contains($"{sticker}|{sdate}"))
                    {
                        continue;
                    }

                    // Create new StockTable record for non-existing stock
                    string companyName = words[1];
                    if (existingCompanies.TryGetValue(sticker, out var company))
                    {
                        companyName = string.IsNullOrEmpty(words[1]) ? company.CompanyName ?? string.Empty : words[1];
                    }

                    stockRecordsToAdd.Add(new StockTable
                    {
                        DayNo = 1,
                        Sticker = sticker,
                        Sname = companyName,
                        Sdate = sdate,
                        Sopen = Convert.ToDouble(words[4]),
                        Shigh = Convert.ToDouble(words[5]),
                        Slow = Convert.ToDouble(words[6]),
                        Sclose = Convert.ToDouble(words[7]),
                        Svol = Convert.ToDouble(words[8]),
                        Createddate = createdDate
                    });

                    // Handle CompanyTables (only if necessary)
                    if (!existingCompanies.ContainsKey(sticker) && !string.IsNullOrEmpty(words[1]) && processedCompanyCodes.Add(sticker))
                    {
                        bool isNumeric = int.TryParse(sticker, out _);
                        companyRecordsToAdd.Add(new CompanyTable
                        {
                            CompanyCode = sticker,
                            CompanyName = words[1],
                            Follow = true,
                            IsIndicator = !isNumeric
                        });
                    }
                    else if (existingCompanies.TryGetValue(sticker, out var existingCompany) && string.IsNullOrEmpty(existingCompany.CompanyName))
                    {
                        existingCompany.CompanyName = words[1];
                        if (!companiesToUpdate.Any(c => c.CompanyCode == sticker))
                        {
                            companiesToUpdate.Add(existingCompany);
                        }
                    }

                    counter++;

                    // Send progress update for processing phase (up to 80%)
                    if ((i + 1) % Math.Max(totalLines / 100, 10000) == 0 || i + 1 == totalLines)
                    {
                        double processingProgress = (double)(i + 1) / totalLines * processingPhasePercentage;
                        await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, processingProgress, $"جاري معالجة الملف {fileName}: {i + 1} من {totalLines} سطر");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"خطأ في معالجة السطر {i + 1}: {lines[i]}");
                }
            }

            // Phase 2: Saving to database (20% of progress)
            const double savingPhasePercentage = 20.0;
            try
            {
                // Send initial saving progress (80%)
                await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, processingPhasePercentage, $"جاري حفظ البيانات في قاعدة البيانات...");

                // Increase command timeout to 120 seconds
                _context.Database.SetCommandTimeout(120);

                // Batch processing for BulkInsert
                const int batchSize = 5000; // Adjust based on testing
                int totalSaveOperations = 0;
                int completedOperations = 0;

                // Calculate total operations
                totalSaveOperations += (stockRecordsToAdd.Any() ? (stockRecordsToAdd.Count + batchSize - 1) / batchSize : 0);
                totalSaveOperations += (companyRecordsToAdd.Any() ? (companyRecordsToAdd.Count + batchSize - 1) / batchSize : 0);
                totalSaveOperations += (companiesToUpdate.Any() ? (companiesToUpdate.Count + batchSize - 1) / batchSize : 0);

                // Save stockRecordsToAdd in batches
                if (stockRecordsToAdd.Any())
                {
                    _logger.LogInformation($"Saving {stockRecordsToAdd.Count} stock records in batches...");
                    for (int i = 0; i < stockRecordsToAdd.Count; i += batchSize)
                    {
                        var batch = stockRecordsToAdd.Skip(i).Take(batchSize).ToList();
                        await _context.BulkInsertAsync(batch);
                        completedOperations++;
                        double saveProgress = processingPhasePercentage + (double)completedOperations / totalSaveOperations * savingPhasePercentage;
                        await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, saveProgress, $"تم حفظ دفعة من الأسهم ({completedOperations}/{totalSaveOperations})...");
                    }
                }

                // Save companyRecordsToAdd in batches
                if (companyRecordsToAdd.Any())
                {
                    _logger.LogInformation($"Saving {companyRecordsToAdd.Count} company records in batches...");
                    for (int i = 0; i < companyRecordsToAdd.Count; i += batchSize)
                    {
                        var batch = companyRecordsToAdd.Skip(i).Take(batchSize).ToList();
                        await _context.BulkInsertAsync(batch);
                        completedOperations++;
                        double saveProgress = processingPhasePercentage + (double)completedOperations / totalSaveOperations * savingPhasePercentage;
                        await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, saveProgress, $"تم حفظ دفعة من الشركات الجديدة ({completedOperations}/{totalSaveOperations})...");
                    }
                }

                // Save companiesToUpdate in batches
                if (companiesToUpdate.Any())
                {
                    _logger.LogInformation($"Updating {companiesToUpdate.Count} company records in batches...");
                    for (int i = 0; i < companiesToUpdate.Count; i += batchSize)
                    {
                        var batch = companiesToUpdate.Skip(i).Take(batchSize).ToList();
                        await _context.BulkUpdateAsync(batch);
                        completedOperations++;
                        double saveProgress = processingPhasePercentage + (double)completedOperations / totalSaveOperations * savingPhasePercentage;
                        await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, saveProgress, $"تم تحديث دفعة من الشركات ({completedOperations}/{totalSaveOperations})...");
                    }
                }

                // Ensure final progress is 100%
                await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, 100.0, $"اكتمل حفظ الملف {fileName} بنجاح!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ أثناء الحفظ الجماعي");
                await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, processingPhasePercentage, $"حدث خطأ أثناء حفظ البيانات: {ex.Message}");
                throw;
            }

            return counter;
        }
        private async Task SortStockTables()
        {
            var companies = await _context.StockTables.Select(x => x.Sticker).Distinct().ToListAsync();
            foreach (var company in companies)
            {
                var stocks = await _context.StockTables
                    .Where(x => x.Sticker == company)
                    .OrderByDescending(x => x.Sdate)
                    .ToListAsync();

                int dayNo = 1;
                foreach (var stock in stocks)
                {
                    stock.DayNo = dayNo++;
                }
            }
            await _context.BulkUpdateAsync(_context.StockTables.ToList());
        }

       
    }
}