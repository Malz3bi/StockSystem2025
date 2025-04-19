using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StockSystem2025.Models;

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

        public IActionResult Index()
        {

            var k= _context.CompanyTables.ToList();
            return View();
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
