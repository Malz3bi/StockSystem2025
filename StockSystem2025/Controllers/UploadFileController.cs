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

        public async Task<IActionResult> UploadFiles(List<IFormFile> files, string processId)
        {
            if (files == null || files.Count == 0 || files.All(f => f.Length == 0))
            {
                _logger.LogWarning("No valid files received in UploadFiles");
                return BadRequest(new { success = false, message = "يرجى اختيار ملف صالح (.txt)." });
            }

            var uploadPath = Path.Combine(_environment.WebRootPath, "Uploads");
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

                // Call SortStockTables for each file to ensure progress is tracked
                foreach (var file in files.Where(f => f.Length > 0 && Path.GetExtension(f.FileName).ToLower() == ".txt"))
                {
                    await SortStockTables(processId, file.FileName);
                }

                return Ok(new { success = true, message = $"تم تحميل {files.Count(f => f.Length > 0)} ملفات و {counter} أيام تداول بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing uploaded files");
                return StatusCode(500, new { success = false, message = $"حدث خطأ أثناء معالجة الملفات: {ex.Message}" });
            }
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

            int counter = 0;
            int totalLines = lines.Length;
            const int chunkSize = 5000; // Process 200 lines per chunk
            const double processingPhasePercentage = 20.0;

            // Process lines in chunks
            for (int chunkStart = 0; chunkStart < totalLines; chunkStart += chunkSize)
            {
                var chunkLines = lines.Skip(chunkStart).Take(chunkSize).ToArray();
                var stockRecordsToAdd = new List<StockTable>();
                var companyRecordsToAdd = new List<CompanyTable>();
                var companiesToUpdate = new List<CompanyTable>();
                var processedCompanyCodes = new HashSet<string>();

                // Phase 1: Processing lines in chunk
                for (int i = 0; i < chunkLines.Length; i++)
                {
                    try
                    {
                        var line = chunkLines[i];
                        var words = line.Split(',');

                        // Parse date (words[3] = yyyymmdd)
                        string sdate = words[3];
                        if (!int.TryParse(sdate.Substring(0, 4), out int year) ||
                            !int.TryParse(sdate.Substring(4, 2), out int month) ||
                            !int.TryParse(sdate.Substring(6, 2), out int day))
                        {
                            _logger.LogWarning($"تنسيق تاريخ غير صالح في السطر {chunkStart + i + 1}: {line}");
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
                            DayNo = 0, // Will be updated in SortStockTables
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
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"خطأ في معالجة السطر {chunkStart + i + 1}: {chunkLines[i]}");
                    }
                }

                // Phase 2: Saving to database for this chunk
                const double savingPhasePercentage = 40.0;
                try
                {
                    // Send initial saving progress for chunk
                    double chunkProgress = (double)(chunkStart + chunkLines.Length) / totalLines * processingPhasePercentage;
                    await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, chunkProgress, $"جاري معالجة الجزء {chunkStart / chunkSize + 1} من الملف {fileName}: {chunkStart + chunkLines.Length} من {totalLines} سطر");

                    // Increase command timeout to 120 seconds
                    _context.Database.SetCommandTimeout(120);

                    // Save stockRecordsToAdd
                    if (stockRecordsToAdd.Any())
                    {
                        _logger.LogInformation($"Saving {stockRecordsToAdd.Count} stock records for chunk {chunkStart / chunkSize + 1}...");
                        await _context.BulkInsertAsync(stockRecordsToAdd);
                    }

                    // Save companyRecordsToAdd
                    if (companyRecordsToAdd.Any())
                    {
                        _logger.LogInformation($"Saving {companyRecordsToAdd.Count} company records for chunk {chunkStart / chunkSize + 1}...");
                        await _context.BulkInsertAsync(companyRecordsToAdd);
                    }

                    // Save companiesToUpdate
                    if (companiesToUpdate.Any())
                    {
                        _logger.LogInformation($"Updating {companiesToUpdate.Count} company records for chunk {chunkStart / chunkSize + 1}...");
                        await _context.BulkUpdateAsync(companiesToUpdate);
                    }

                    // Update progress after saving
                    double saveProgress = processingPhasePercentage + ((double)(chunkStart + chunkLines.Length) / totalLines) * (savingPhasePercentage / 2);
                    await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, saveProgress, $"تم حفظ الجزء {chunkStart / chunkSize + 1} من الملف {fileName}");

                    // Clear memory
                    stockRecordsToAdd.Clear();
                    companyRecordsToAdd.Clear();
                    companiesToUpdate.Clear();
                    processedCompanyCodes.Clear();
                    GC.Collect(); // Force garbage collection to free memory
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"خطأ أثناء حفظ الجزء {chunkStart / chunkSize + 1} من الملف {fileName}");
                    await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, processingPhasePercentage, $"حدث خطأ أثناء حفظ الجزء {chunkStart / chunkSize + 1}: {ex.Message}");
                    throw;
                }
            }

            // Final progress update for processing and saving
            await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, processingPhasePercentage + 40.0, $"اكتمل حفظ البيانات للملف {fileName}!");
            return counter;
        }
        private async Task SortStockTables(string processId, string fileName)
        {
            const double sortingPhasePercentage = 40.0;
            const double sortingStartPercentage = 60.0; // 20% processing + 40% saving

            var companies = await _context.StockTables.Select(x => x.Sticker).Distinct().ToListAsync();
            var stocksToUpdate = new List<StockTable>();
            int totalCompanies = companies.Count;
            int processedCompanies = 0;

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
                    stocksToUpdate.Add(stock);
                }

                // Send progress update for sorting phase (from 60% to 100%)
                processedCompanies++;
                if (processedCompanies % Math.Max(totalCompanies / 100, 10) == 0 || processedCompanies == totalCompanies)
                {
                    double sortingProgress = sortingStartPercentage + (double)processedCompanies / totalCompanies * sortingPhasePercentage;
                    await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, sortingProgress, $"جاري إعادة ترتيب DayNo للشركة {processedCompanies} من {totalCompanies} ({fileName})...");
                }
            }

            if (stocksToUpdate.Any())
            {
                _logger.LogInformation($"Updating {stocksToUpdate.Count} stock records with new DayNo values...");
                await _context.BulkUpdateAsync(stocksToUpdate);

                // Send final sorting progress (100%)
                await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, 100.0, $"اكتمل إعادة ترتيب DayNo للملف {fileName}!");
            }
            else
            {
                _logger.LogInformation("No stock records to update for DayNo.");
                await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, 100.0, $"لم يتم العثور على سجلات لإعادة ترتيب DayNo للملف {fileName}.");
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
                await SortStockTables("999","999");
                TempData["ClearMessage"] = "تم مسح بيانات التداول بين التاريخين بنجاح";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing data between dates");
                TempData["ClearMessage"] = "حدث خطأ أثناء مسح بيانات التداول";
            }

            return RedirectToAction("UploadFileIndex");
        }

       
    }
}