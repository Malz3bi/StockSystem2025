using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models;
using StockSystem2025.Models.AccountModels;
using StockSystem2025.SFLModels;
using StockSystem2025.SFLServices;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StockSystem2025.Controllers
{
    public class SFLSpecialFollowListController : Controller
    {
        private readonly SFLIFollowListService _followListService;
        private readonly SFLIStockService _stockService;
        private readonly SFLISettingsService _settingsService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StockdbContext _context;




        public SFLSpecialFollowListController(
            SFLIFollowListService followListService,
            SFLIStockService stockService,
            SFLISettingsService settingsService,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        StockdbContext context)
        {
            _followListService = followListService;
            _stockService = stockService;
            _settingsService = settingsService;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _context = context;



        }

        public async Task<IActionResult> SFLIndex(int? followListId, DateTime? recommendationDate, string sortColumn = "", string sortOrder = "asc")
        {
            var user = await GetCurrentUserId();
            var model = new SFLSpecialFollowListViewModel
            {
                SFLFollowLists = await _followListService.SFLGetFollowListsAsync(user.Id),
                SFLRecommendationDate = recommendationDate ?? await _stockService.SFLGetLastDateAsync(),
                SFLMinDate = await _stockService.SFLGetMinDateAsync(),
                SFLMaxDate = await _stockService.SFLGetMaxDateAsync(),
                SFLSortColumn = sortColumn,
                SFLSortOrder = sortOrder
            };

            if (model.SFLFollowLists.Count == 0)
            {
                return View(model);
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

   
        public async Task<ApplicationUser?> GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return null;
            }
            return await _userManager.GetUserAsync(user);
        }


        // GET: FollowLists
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var followLists = await _context.FollowLists
                .Where(f => f.UserId == userId)
                .ToListAsync();
            return View(followLists);
        }

        // GET: FollowLists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FollowLists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FollowList followList)
        {
        
                followList.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Add(followList);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
           
        }

        // GET: FollowLists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followList = await _context.FollowLists.FindAsync(id);
            if (followList == null || followList.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return NotFound();
            }
            return View(followList);
        }

        // POST: FollowLists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FollowList followList)
        {
            if (id != followList.Id)
            {
                return NotFound();
            }

          
                try
                {
                    var existingFollowList = await _context.FollowLists.FindAsync(id);
                    if (existingFollowList == null || existingFollowList.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                    {
                        return NotFound();
                    }

                    existingFollowList.Name = followList.Name;
                    existingFollowList.Color = followList.Color;
                    existingFollowList.StopLoss = followList.StopLoss;
                    existingFollowList.FirstSupport = followList.FirstSupport;
                    existingFollowList.SecondSupport = followList.SecondSupport;
                    existingFollowList.FirstTarget = followList.FirstTarget;
                    existingFollowList.SecondTarget = followList.SecondTarget;
                    existingFollowList.ThirdTarget = followList.ThirdTarget;

                    _context.Update(existingFollowList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FollowListExists(followList.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction("Index");
          
        }

        // GET: FollowLists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var followList = await _context.FollowLists
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (followList == null)
            {
                return NotFound();
            }

            return View(followList);
        }

        // POST: FollowLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var followList = await _context.FollowLists.FindAsync(id);
            if (followList != null && followList.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                _context.FollowLists.Remove(followList);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        private bool FollowListExists(int id)
        {
            return _context.FollowLists.Any(e => e.Id == id);
        }
    }
}




