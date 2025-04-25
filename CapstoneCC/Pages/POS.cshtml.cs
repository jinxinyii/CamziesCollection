using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CapstoneCC.Models;
using System.Drawing.Printing;
using System.Drawing;
using CapstoneCC.Services;
using Microsoft.AspNetCore.Identity;

namespace CapstoneCC.Pages
{
    public class POSModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public POSModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public string CashierName { get; private set; }
        public List<Product> Products { get; set; }
        public async Task OnGetAsync()
        {
            Products = _context.Products.ToList();

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                CashierName = $"{user.FirstName} {user.LastName}";
            }
        }
    }
}
