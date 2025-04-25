using CapstoneCC.Models;
using CapstoneCC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CapstoneCC.Pages
{
    public class AnalyticsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public decimal TotalSales { get; set; }
        public int NewUsers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int WalkInCount { get; set; }
        public List<TopProductDto> TopProducts { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Fetch Walk in
            var analytics = await _context.Analytics.FirstOrDefaultAsync(a => a.Id == 1);
            if (analytics == null)
            {
                analytics = new Analytics
                {
                    WalkInCount = 0
                };
            }
            WalkInCount = analytics.WalkInCount;

            // Fetch Total Sales
            TotalSales = await _context.SalesTransactions.SumAsync(t => t.Amount);

            // Fetch New Users Count (last 30 days as an example)
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            NewUsers = await _context.Users.CountAsync(u => u.CreatedAt >= thirtyDaysAgo);

            // Fetch Total Orders
            TotalOrders = await _context.Orders.CountAsync();

            // Fetch Revenue
            TotalRevenue = await _context.Orders
            .Where(o => o.OrderStatus == "Approved")
            .SelectMany(o => o.OrderProducts)
            .SumAsync(op => op.Price * op.Quantity);

            // Fetch Top Products
            TopProducts = await _context.OrderProducts
            .GroupBy(op => op.Product.Id)
            .Select(g => new TopProductDto
            {
                ProductId = g.Key,
                ProductName = g.First().Product.Name,
                TotalSold = g.Sum(op => op.Quantity),
                TotalRevenue = g.Sum(op => op.Quantity * op.Price)
            })
            .OrderByDescending(tp => tp.TotalSold)
            .Take(5)
            .ToListAsync();

            return Page();
        }
        public class TopProductDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int TotalSold { get; set; }
            public decimal TotalRevenue { get; set; }
        }
    }
}
