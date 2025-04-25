using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using CapstoneCC.Models;
using CapstoneCC.Services;
using ZXing;
using ZXing.Common;
using System.Drawing.Imaging;
using System.Drawing;

namespace CapstoneCC.Pages
{
    public class InventoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly string _jsonFilePath;
        public InventoryModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public void OnGet()
        {
            Products = _context.Products.ToList();
        }
        public class Inventory
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
        }
        [BindProperty]
        public IFormFile ProductImage { get; set; }

        [BindProperty]
        public string ProductName { get; set; }

        [BindProperty]
        public int ProductDiscount { get; set; }

        [BindProperty]
        public decimal ProductPrice { get; set; }
        [BindProperty]
        public decimal RetailPrice { get; set; }

        [BindProperty]
        public int RetailQuantity { get; set; }

        [BindProperty]
        public int ProductStock { get; set; }

        [BindProperty]
        public string ProductDescription { get; set; }

        [BindProperty]
        public string ProductBadge { get; set; }

        [BindProperty]
        public string ProductBarcode { get; set; }

        public List<Product> Products { get; set; }

        public IActionResult OnPostUploadProduct()
        {
            if (string.IsNullOrEmpty(ProductName) || ProductPrice <= 0 || ProductStock < 0)
            {
                ModelState.AddModelError(string.Empty, "Please fill in all required fields with valid data.");
                return Page();
            }
            if (ProductImage != null && ProductImage.Length > 0)
            {
                // Define the file path
                var filePath = Path.Combine("wwwroot/images/products", ProductImage.FileName);

                // Save the image to the server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    ProductImage.CopyTo(stream);
                }

                // Determine which barcode to use
                string barcodeToSave;
                if (!string.IsNullOrEmpty(ProductBarcode))
                {
                    // Use the manually entered barcode if valid
                    barcodeToSave = ProductBarcode.Length <= 13 ? ProductBarcode : ProductBarcode.Substring(0, 13);
                }
                else
                {
                    // Otherwise, generate a 13-digit EAN-13 barcode
                    string baseBarcode = GenerateBaseBarcode();
                    int checkDigit = CalculateEAN13CheckDigit(baseBarcode);
                    barcodeToSave = baseBarcode + checkDigit.ToString();
                }

                // Store the product in the database
                var product = new Product
                {
                    Name = ProductName,
                    Discount = ProductDiscount,
                    Price = ProductPrice,
                    RetailPrice = RetailPrice > 0 ? RetailPrice : ProductPrice,
                    RetailQuantity = RetailQuantity > 0 ? RetailQuantity : 1,
                    Description = ProductDescription,
                    Badge = ProductBadge,
                    Stock = ProductStock,
                    ImagePath = "/images/products/" + ProductImage.FileName,
                    Barcode = barcodeToSave 
                };

                _context.Products.Add(product);
                _context.SaveChanges();
            }

            return RedirectToPage("Inventory");
        }

        private string GenerateBaseBarcode()
        {
            Random random = new Random();
            string baseBarcode = string.Empty;

            // Generate 12 random digits
            for (int i = 0; i < 12; i++)
            {
                baseBarcode += random.Next(0, 10); 
            }

            return baseBarcode;
        }

        private int CalculateEAN13CheckDigit(string baseBarcode)
        {
            int sum = 0;
            for (int i = 0; i < baseBarcode.Length; i++)
            {
                int digit = int.Parse(baseBarcode[i].ToString());
                sum += (i % 2 == 0) ? digit : digit * 3;
            }
            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit;
        }

        public IActionResult OnPostDeleteProduct(int productId)
        {
            var productToDelete = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (productToDelete != null)
            {
                _context.Products.Remove(productToDelete);
                _context.SaveChanges();
            }
            return RedirectToPage();
        }

        public IActionResult OnPostUpdateProduct(int id, string name, int discount, decimal price, decimal retailPrice, int retailQuantity, string description, string badge, int stock, string barcode)
        {
            var existingProduct = _context.Products.FirstOrDefault(p => p.Id == id);
            if (existingProduct != null)
            {
                existingProduct.Name = name;
                existingProduct.Discount = discount;
                existingProduct.Price = price;
                existingProduct.RetailPrice = retailPrice;
                existingProduct.RetailQuantity = retailQuantity;
                existingProduct.Description = description;
                existingProduct.Badge = badge;
                existingProduct.Stock = stock;
                existingProduct.Barcode = barcode;
                _context.SaveChanges();
            }
            return RedirectToPage();
        }

        private string SaveProductImage(IFormFile productImage)
        {
            var filePath = Path.Combine("wwwroot/images", productImage.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                productImage.CopyTo(stream);
            }

            return $"/images/{productImage.FileName}";
        }

        private int GetNewProductId()
        {
            var products = LoadProductsFromFile();
            return products.Any() ? products.Max(p => p.Id) + 1 : 1;
        }

        private List<Product> LoadProductsFromFile()
        {
            if (System.IO.File.Exists(_jsonFilePath))
            {
                var json = System.IO.File.ReadAllText(_jsonFilePath);
                return JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
            }
            return new List<Product>();
        }

        private void SaveProductsToFile(List<Product> products)
        {
            var json = JsonSerializer.Serialize(products);
            System.IO.File.WriteAllText(_jsonFilePath, json);
        }

        public IActionResult DeductInventory(Dictionary<int, int> itemsToDeduct)
        {
            var products = LoadProductsFromFile();
            bool inventoryUpdated = false;

            foreach (var item in itemsToDeduct)
            {
                var product = products.FirstOrDefault(p => p.Id == item.Key);
                if (product != null && product.Stock >= item.Value)
                {
                    product.Stock -= item.Value;
                    inventoryUpdated = true;
                }
            }

            if (inventoryUpdated)
            {
                SaveProductsToFile(products);
            }

            return new JsonResult(new { success = inventoryUpdated });
        }

        public IActionResult GetProductById(string productId)
        {
            if (!int.TryParse(productId, out int productIdInt))
            {
                return NotFound(); 
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == productIdInt); 
            return product != null ? new JsonResult(product) : NotFound();
        }

        public IActionResult OnGetSearchProducts(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new JsonResult(new List<Product>());
            }

            var matchingProducts = _context.Products
                .Where(p => p.Barcode.Contains(query) || p.Name.Contains(query))
                .Select(p => new { p.Name, p.Barcode, p.Price })
                .Take(10)
                .ToList();

            return new JsonResult(matchingProducts);
        }
    }
}