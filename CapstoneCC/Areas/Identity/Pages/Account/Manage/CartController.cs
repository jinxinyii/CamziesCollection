using CapstoneCC.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CapstoneCC.Areas.Identity.Pages.Account.Manage
{
    public class CartController : Controller
    {
        

        public IActionResult Index()
        {
            return View();
        }
    }
}
