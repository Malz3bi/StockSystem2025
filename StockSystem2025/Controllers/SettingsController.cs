using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StockSystem2025.Controllers
{
    public enum Role
    {
        Admin = 10,
        Supervisor = 5,
        User = 0
    }

   
    public class SettingsController : Controller
    {
        private readonly StockdbContext _context;

        public SettingsController(StockdbContext context)
        {
            _context = context;
        }

        // GET: Settings
        public async Task<IActionResult> Index()
        {
            //var userRole = GetUserRole();
            //ViewBag.IsAdmin = userRole == Role.Admin;
            //ViewBag.IsSupervisor = userRole == Role.Supervisor;
            
            
            ViewBag.IsAdmin = true;
            ViewBag.IsSupervisor =true;

            var settings = await _context.Settings.ToDictionaryAsync(s => s.Name, s => s.Value ?? string.Empty);

            var model = new SettingsViewModel
            {
                SiteName = settings.GetValueOrDefault("SiteName", ""),
                RSIDays = int.TryParse(settings.GetValueOrDefault("RSIDays"), out var rsi) ? rsi : 0,
                Williams = int.TryParse(settings.GetValueOrDefault("Williams"), out var williams) ? williams : 0,
                StockDays = int.TryParse(settings.GetValueOrDefault("StockDays"), out var stockDays) ? stockDays : 1,
                ChartDays = int.TryParse(settings.GetValueOrDefault("ChartDays"), out var chartDays) ? chartDays : 0,
                ExcelSheetName = settings.GetValueOrDefault("ExcelSheetName", ""),
                StickerColNo = int.TryParse(settings.GetValueOrDefault("StickerColNo"), out var sticker) ? sticker : 1,
                SnameColNo = int.TryParse(settings.GetValueOrDefault("SnameColNo"), out var sname) ? sname : 1,
                SopenColNo = int.TryParse(settings.GetValueOrDefault("SopenColNo"), out var sopen) ? sopen : 1,
                SHighColNo = int.TryParse(settings.GetValueOrDefault("SHighColNo"), out var shigh) ? shigh : 1,
                SLowColNo = int.TryParse(settings.GetValueOrDefault("SLowColNo"), out var slow) ? slow : 1,
                SCloseColNo = int.TryParse(settings.GetValueOrDefault("SCloseColNo"), out var sclose) ? sclose : 1,
                SvolColNo = int.TryParse(settings.GetValueOrDefault("SvolColNo"), out var svol) ? svol : 1,
                ExpectedOpenColNo = int.TryParse(settings.GetValueOrDefault("ExpectedOpenColNo"), out var expected) ? expected : 1,
                SDateColNo = int.TryParse(settings.GetValueOrDefault("SDateColNo"), out var sdate) ? sdate : 1,
                InstantUpdateValue = int.TryParse(settings.GetValueOrDefault("InstantUpdateValue"), out var instant) ? instant : 0,
                ShowCompaniesCount = settings.GetValueOrDefault("ShowCompaniesCount") == "1"
            };

            return View(model);
        }

        // POST: Settings/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var userRole = GetUserRole();
            ViewBag.IsAdmin = userRole == Role.Admin;
            ViewBag.IsSupervisor = userRole == Role.Supervisor;

            var settings = await _context.Settings.ToListAsync();

            UpdateSetting(settings, "SiteName", model.SiteName);
            UpdateSetting(settings, "RSIDays", model.RSIDays.ToString());
            UpdateSetting(settings, "Williams", model.Williams.ToString());
            UpdateSetting(settings, "StockDays", model.StockDays.ToString());
            UpdateSetting(settings, "ChartDays", model.ChartDays.ToString());
            UpdateSetting(settings, "UpColor1", model.UpColor1);
            UpdateSetting(settings, "UpColor2", model.UpColor2);
            UpdateSetting(settings, "DownColor1", model.DownColor1);
            UpdateSetting(settings, "DownColor2", model.DownColor2);
            UpdateSetting(settings, "ExcelSheetName", model.ExcelSheetName);
            UpdateSetting(settings, "StickerColNo", model.StickerColNo.ToString());
            UpdateSetting(settings, "SnameColNo", model.SnameColNo.ToString());
            UpdateSetting(settings, "SopenColNo", model.SopenColNo.ToString());
            UpdateSetting(settings, "SHighColNo", model.SHighColNo.ToString());
            UpdateSetting(settings, "SLowColNo", model.SLowColNo.ToString());
            UpdateSetting(settings, "SCloseColNo", model.SCloseColNo.ToString());
            UpdateSetting(settings, "SvolColNo", model.SvolColNo.ToString());
            UpdateSetting(settings, "ExpectedOpenColNo", model.ExpectedOpenColNo.ToString());
            UpdateSetting(settings, "SDateColNo", model.SDateColNo.ToString());
            UpdateSetting(settings, "InstantUpdateValue", model.InstantUpdateValue.ToString());
            UpdateSetting(settings, "ShowCompaniesCount", model.ShowCompaniesCount ? "1" : "0");

            if (model.ClearData)
            {
                await ClearAllDataAsync();
                TempData["SuccessMessage"] = "تم مسح جميع بيانات البرنامج بنجاح";
                return RedirectToAction("Index");
            }

          

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم حفظ التغييرات بنجاح";
            return RedirectToAction("Index");
        }

        // GET: Settings/ManageCompanies
        public async Task<IActionResult> ManageCompanies(string indicatorCode = "TASI")
        {
            //var userRole = GetUserRole();
            //if (userRole != Role.Admin && userRole != Role.Supervisor)
            //{
            //    return Forbid();
            //}

            var indicators = await _context.CompanyTables
                .Where(c => c.IsIndicator)
                .ToListAsync();

            var indicator = await _context.CompanyTables
                .FirstOrDefaultAsync(c => c.CompanyCode == indicatorCode && c.IsIndicator) ?? indicators.First();

            var existingCompanies = await _context.CompanyTables
                .Where(c => c.ParentIndicator == indicatorCode)
                .ToListAsync();

            var remainingCompanies = await _context.CompanyTables
                .Where(c => c.IsIndicator == (indicatorCode == "TASI") && c.CompanyCode != "TASI" && c.ParentIndicator == null)
                .ToListAsync();

            remainingCompanies = remainingCompanies.Except(existingCompanies).ToList();

            var model = new ManageCompaniesViewModel
            {
                IndicatorId = indicator.CompanyId,
                IndicatorCode = indicator.CompanyCode,
                IndicatorName = indicator.CompanyName ?? string.Empty,
                ExistingCompanies = existingCompanies,
                RemainingCompanies = remainingCompanies
            };

            ViewBag.Indicators = indicators;
            return View(model);
        }

        // POST: Settings/SaveCompanies
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCompanies(ManageCompaniesViewModel model)
        {
            //var userRole = GetUserRole();
            //if (userRole != Role.Admin && userRole != Role.Supervisor)
            //{
            //    return Forbid();
            //}

            var existingCompanies = await _context.CompanyTables.Where(c => c.ParentIndicator == model.IndicatorCode).ToListAsync();
            if (!string.IsNullOrEmpty(model.SelectedExistingCompanies?.FirstOrDefault()))
            {
                var  oldcompanyCodes = model.SelectedExistingCompanies.FirstOrDefault().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(code => code.Trim()).ToList();

                // Remove companies not in the new list
                foreach (var company in existingCompanies)
                {
                    if (!oldcompanyCodes.Contains(company.CompanyCode))
                    {
                        company.ParentIndicator = null;
                    }
                }
            }






            if (!string.IsNullOrEmpty(model.SelectedExistingCompanies?.FirstOrDefault()))
            {
                var companyCodes = model.SelectedExistingCompanies.FirstOrDefault()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(code => code.Trim())
                    .ToList();

                foreach (var code in companyCodes)
                {
                    var company = await _context.CompanyTables
                        .FirstOrDefaultAsync(c => c.CompanyCode == code);
                    if (company != null && string.IsNullOrEmpty(company.ParentIndicator))
                    {
                        company.ParentIndicator = model.IndicatorCode;
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم حفظ التغييرات بنجاح";
            return RedirectToAction("ManageCompanies", new { indicatorCode = model.IndicatorCode });
        }

        private Role GetUserRole()
        {
            // Placeholder for actual authentication logic (e.g., using User.IsInRole)
            return User.IsInRole("Admin") ? Role.Admin : User.IsInRole("Supervisor") ? Role.Supervisor : Role.User;
        }

        private void UpdateSetting(List<Setting> settings, string name, string value)
        {
            var setting = settings.FirstOrDefault(s => s.Name == name);
            if (setting != null)
            {
                setting.Value = value;
            }
            else
            {
                _context.Settings.Add(new Setting { Name = name, Value = value });
            }
        }

        private async Task ClearAllDataAsync()
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM StockTable");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM FollowListCompanies");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM FollowList");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM RefreshedPage");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Formulas");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Criterias");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Mediums");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM CompanyTable");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM EconomicLinks");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM EconLinksTypes");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM DigitalAnalysisData");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM DigitalAnalysis");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProfessionalFibonacciData");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ProfessionalFibonacci");
        }
    }
}