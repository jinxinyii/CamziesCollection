using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using CapstoneCC.Pages.Helper;
using CapstoneCC.Models;
using System.IO;
using System.Text.Json;
using CapstoneCC.Services;
using Newtonsoft.Json;

namespace CapstoneCC.Pages
{
    public class SkincareModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public SkincareModel(ApplicationDbContext context)
        {
            _context = context;
        }
        private Product GetProductById(int productId)
        {
            // Assuming you have _context injected into your PageModel
            return _context.Products.FirstOrDefault(p => p.Id == productId);
        }

        public List<Product> Products { get; set; }
        public IActionResult OnGet()
        {
            Products = _context.Products.ToList();

            // Store a value in session
            HttpContext.Session.SetString("TestKey", "Hello, World!");

            // Retrieve the value from session
            var value = HttpContext.Session.GetString("TestKey");

            // Print the value (or debug to ensure it works)
            Console.WriteLine(value);
            return Page();
        }
        public IActionResult OnPostAddToCart(int productId, int quantity, decimal discountedPrice)
        {
            // Retrieve the product from the database
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return NotFound();

            // Retrieve the cart from the session
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Check if the product already exists in the cart
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);
            if (existingItem != null)
            {
                // Update quantity and total price
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.Quantity * existingItem.Price; // Price must be set previously
            }
            else
            {
                // Add new item to the cart
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = discountedPrice, // Use discounted price
                    Quantity = quantity,
                    TotalPrice = discountedPrice * quantity,
                    ImagePath = product.ImagePath
                });
            }

            // Save the updated cart to the session
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return RedirectToPage("Skincare");
        }
    }
}
