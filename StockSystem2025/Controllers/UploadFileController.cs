using System.Diagnostics;
using System.Drawing.Printing;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models;
using StockSystem2025.ViewModel;

namespace StockSystem2025.Controllers
{
    public class UploadFileController : Controller
    {
        private readonly IHubContext<ProgressHub> _progressHub;
        private readonly StockdbContext _context;
        private readonly IWebHostEnvironment _environment; // Add this field
        private readonly ILogger<UploadFileController> _logger;

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
            int counter = 0;
            int totalLines = lines.Length;
            for (int i = 0; i < totalLines; i++)
            {

                try
                {
                    var line = lines[i];
                    var words = line.Split(',');
                    string sticker = words[0];
                    string sdate = words[3];
                    int year = int.Parse(sdate.Substring(0, 4));
                    int month = int.Parse(sdate.Substring(4, 2));
                    int day = int.Parse(sdate.Substring(6, 2));
                    sdate = $"{year}/{month:D2}/{day:D2}";
                    DateTime createdDate = new DateTime(year, month, day);

                    var stock = await _context.StockTables
                        .FirstOrDefaultAsync(x => x.Sticker == sticker && x.Sdate == sdate);

                    if (stock == null)
                    {
                        var company = await _context.CompanyTables
                            .FirstOrDefaultAsync(x => x.CompanyCode == sticker);
                        string companyName = string.IsNullOrEmpty(words[1]) && company != null
                            ? company.CompanyName ?? string.Empty
                            : words[1];

                        _context.StockTables.Add(new StockTable
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
                    }
                    else
                    {
                        stock.Sname = words[1];
                        stock.Sopen = Convert.ToDouble(words[4]);
                        stock.Shigh = Convert.ToDouble(words[5]);
                        stock.Slow = Convert.ToDouble(words[6]);
                        stock.Sclose = Convert.ToDouble(words[7]);
                        stock.Svol = Convert.ToDouble(words[8]);
                    }

                    if (!await _context.CompanyTables.AnyAsync(x => x.CompanyCode == sticker) && !string.IsNullOrEmpty(words[1]))
                    {
                        bool isNumeric = int.TryParse(sticker, out _);
                        _context.StockTables.Add(new StockTable
                        {
                            DayNo = 1,
                            Sticker = sticker,
                            Sname = words[1],
                            Sdate = sdate,
                            Sopen = Convert.ToDouble(words[4]),
                            Shigh = Convert.ToDouble(words[5]),
                            Slow = Convert.ToDouble(words[6]),
                            Sclose = Convert.ToDouble(words[7]),
                            Svol = Convert.ToDouble(words[8]),
                            Createddate = createdDate
                        });
                    }
                    else if (await _context.CompanyTables.AnyAsync(x => x.CompanyCode == sticker && string.IsNullOrEmpty(x.CompanyName)))
                    {
                        var company = await _context.CompanyTables.FirstAsync(x => x.CompanyCode == sticker);
                        company.CompanyName = words[1];
                    }

                    await _context.SaveChangesAsync();
                    counter++;

                    // Send progress update
                    double progress = (double)(i + 1) / totalLines * 100;
                    await _progressHub.Clients.All.SendAsync("ReceiveProgress", processId, progress, $"جاري معالجة الملف {fileName}: {i + 1} من {totalLines} سطر");
                }
                catch (Exception ex)
                {
                    var line = lines[i];
                    _logger.LogWarning(ex, $"Error processing line: {line}");
                }
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
            await _context.SaveChangesAsync();
        }
        public class ProgressHub : Hub
        {
            public async Task SendProgress(string processId, double percentage, string message)
            {
                await Clients.Caller.SendAsync("ReceiveProgress", processId, percentage, message);
            }
        }
    }
  

}
