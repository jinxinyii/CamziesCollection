using CapstoneCC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneCC.Models
{
    [Route("Sales")]
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SalesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet("GetProducts")]
        public IActionResult GetProducts()
        {
            var products = _context.Products
                .Select(p => new
                {
                    Id = p.Id,
                    Barcode = p.Barcode,
                    Name = p.Name,
                    Price = p.Price,
                    Discount = p.Discount,
                    RetailPrice = p.RetailPrice,
                    RetailQuantity = p.RetailQuantity
                })
                .ToList();

            return Ok(products);
        }
        [HttpPost("SaveTransactionId")]
        public async Task<IActionResult> SaveTransactionId([FromBody] SalesTransaction model)
        {
            if (model != null && !string.IsNullOrEmpty(model.TransactionId))
            {
                // Retrieve the currently logged-in user
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(); // User is not logged in
                }

                // Combine FirstName and LastName
                var fullName = $"{user.FirstName} {user.LastName}";

                var transaction = new SalesTransaction
                {
                    TransactionId = model.TransactionId,
                    CashierName = fullName,
                    Amount = model.Amount,
                    TransactionDate = DateTime.Now
                };

                _context.SalesTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Transaction saved successfully." });
            }

            return Json(new { success = false, message = "Invalid transaction data." });
        }
        [HttpPost("DeductStock")]
        public IActionResult DeductStock([FromBody] List<CartItem> purchases)
        {
            var insufficientStockItems = new List<string>();

            // Validate stock first without making any changes
            foreach (var item in purchases)
            {
                Console.WriteLine($"Checking ProductId: {item.ProductId}, Quantity: {item.Quantity}");

                var product = _context.Products.Find(item.ProductId);
                if (product == null)
                {
                    return BadRequest(new { message = $"Product not found: {item.ProductId}" });
                }

                if (product.Stock < item.Quantity)
                {
                    insufficientStockItems.Add($"{product.Name} (Requested: {item.Quantity}, Available: {product.Stock})");
                }
            }

            // If any product has insufficient stock, return an error
            if (insufficientStockItems.Any())
            {
                return BadRequest(new
                {
                    message = "Transaction declined. Insufficient stock for the following products:",
                    insufficientItems = insufficientStockItems
                });
            }

            // Deduct stock only if all validations pass
            foreach (var item in purchases)
            {
                var product = _context.Products.Find(item.ProductId);
                product.Stock -= item.Quantity;
                Console.WriteLine($"Deducted Stock for {product.Name}: {product.Stock}");
            }

            _context.SaveChanges();
            return Ok(new { message = "Stock updated successfully!" });
        }

        [HttpGet("Index")] // Route: /Sales/Index
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("Sales")]
        public IActionResult Sales()
        {
            var transactions = _context.SalesTransactions.ToList();
            return View(transactions);
        }
    }
}
