using CapstoneCC.Services;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneCC.Models
{
    public class POSController : Controller
    {
        private readonly ApplicationDbContext _context;

        public POSController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View();
        }
    }
}