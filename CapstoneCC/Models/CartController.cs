using CapstoneCC.Services;
using Microsoft.AspNetCore.Mvc;

namespace CapstoneCC.Models
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetProductByBarcode(string barcode)
        {
            var product = _context.Products.FirstOrDefault(p => p.Barcode == barcode);
            if (product == null) return NotFound();

            return Json(new { id = product.Id, name = product.Name, price = product.Price });
        }
        [HttpPost]
        public IActionResult GenerateReceipt([FromBody] List<CartItem> cartItems)
        {
            // Reduce stock as explained earlier
            foreach (var item in cartItems)
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    if (product.Stock < 0) product.Stock = 0;
                }
            }

            _context.SaveChanges();

            // Return success response
            return Ok();
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
