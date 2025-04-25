using CapstoneCC.Services;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneCC.Models
{
    public class InventoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
