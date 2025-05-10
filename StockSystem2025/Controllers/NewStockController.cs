using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockSystem2025.Models;
using StockSystem2025.Services;
using StockSystem2025.ViewModel;
using StockSystem2025.ViewModels;
using System.Threading.Tasks;

namespace StockSystem2025.Controllers
{
    public class NewStockController : Controller
    {
        private readonly INewEconomicLinksService _economicLinksService;
        private readonly StockdbContext _context;

        public NewStockController(INewEconomicLinksService economicLinksService, StockdbContext context)
        {
            _economicLinksService = economicLinksService;
            _context = context;
        }

        // Economic Links
        public async Task<IActionResult> EconomicLinks(int? editId)
        {
            var linkTypes = await _economicLinksService.GetLinkTypesAsync();
            var groupedLinks = new List<(EconLinksType Type, List<EconomicLink> Links)>();
            foreach (var type in linkTypes)
            {
                var links = await _economicLinksService.GetLinksByTypeIdAsync(type.Id);
                groupedLinks.Add((type, links));
            }

            var viewModel = new NewEconomicLinksViewModel
            {
                LinkTypes = linkTypes,
                GroupedLinks = groupedLinks
            };

            if (editId.HasValue)
            {
                var link = await _economicLinksService.GetLinkByIdAsync(editId.Value);
                if (link != null)
                {
                    viewModel.EditLinkId = editId;
                    viewModel.LinkName = link.Name;
                    viewModel.LinkUrl = link.Link;
                    viewModel.SelectedTypeId = link.TypeId;
                }
            }

            ViewData["Title"] = "روابط اقتصادية";
            return View("EconomicLinks", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLinkType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                ModelState.AddModelError("TypeName", "اسم الجدول مطلوب.");
            }

            if (ModelState.IsValid)
            {
                await _economicLinksService.AddLinkTypeAsync(typeName);
                return RedirectToAction(nameof(EconomicLinks));
            }

            var viewModel = new NewEconomicLinksViewModel
            {
                LinkTypes = await _economicLinksService.GetLinkTypesAsync(),
                GroupedLinks = new List<(EconLinksType, List<EconomicLink>)>()
            };
            ViewData["Title"] = "روابط اقتصادية";
            return View("EconomicLinks", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLink(NewEconomicLinksViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.LinkName) || string.IsNullOrWhiteSpace(model.LinkUrl) || !model.SelectedTypeId.HasValue)
            {
                ModelState.AddModelError("", "جميع الحقول مطلوبة.");
            }

            if (ModelState.IsValid)
            {
                await _economicLinksService.AddLinkAsync(model.LinkName, model.LinkUrl, model.SelectedTypeId.Value);
                return RedirectToAction(nameof(EconomicLinks));
            }

            model.LinkTypes = await _economicLinksService.GetLinkTypesAsync();
            model.GroupedLinks = new List<(EconLinksType, List<EconomicLink>)>();
            ViewData["Title"] = "روابط اقتصادية";
            return View("EconomicLinks", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLink(NewEconomicLinksViewModel model)
        {
            if (!model.EditLinkId.HasValue || string.IsNullOrWhiteSpace(model.LinkName) || string.IsNullOrWhiteSpace(model.LinkUrl) || !model.SelectedTypeId.HasValue)
            {
                ModelState.AddModelError("", "جميع الحقول مطلوبة.");
            }

            if (ModelState.IsValid)
            {
                await _economicLinksService.UpdateLinkAsync(model.EditLinkId.Value, model.LinkName, model.LinkUrl, model.SelectedTypeId.Value);
                return RedirectToAction(nameof(EconomicLinks));
            }

            model.LinkTypes = await _economicLinksService.GetLinkTypesAsync();
            model.GroupedLinks = new List<(EconLinksType, List<EconomicLink>)>();
            ViewData["Title"] = "روابط اقتصادية";
            return View("EconomicLinks", model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteLink(int id)
        {
            await _economicLinksService.DeleteLinkAsync(id);
            return RedirectToAction(nameof(EconomicLinks));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteLinkType(int id)
        {
            await _economicLinksService.DeleteLinkTypeAsync(id);
            return RedirectToAction(nameof(EconomicLinks));
        }

        // About
        public IActionResult About()
        {
            ViewData["Title"] = "عن البرنامج";
            return View();
        }

        // Instructions
        public IActionResult Instructions()
        {
            ViewData["Title"] = "تعليمات البرنامج";
            return View();
        }

        // Strategic Analysis
        public async Task<IActionResult> StrategicAnalysis(int tabIndex = 0)
        {
            var digitalAnalyses = await _context.DigitalAnalyses.Take(10).ToListAsync();
            var fibonacciAnalyses = await _context.ProfessionalFibonaccis.Take(10).ToListAsync();

            var viewModel = new NewStrategicAnalysisViewModel
            {
                SelectedTabIndex = tabIndex,
                DigitalAnalyses = digitalAnalyses,
                FibonacciAnalyses = fibonacciAnalyses
            };

            ViewData["Title"] = "التحليل الاستراتيجي";
            return View("StrategicAnalysis", viewModel);
        }
    }
}