using CapstoneCC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using CapstoneCC.Pages.Helper;
using Microsoft.AspNetCore.Identity;
using CapstoneCC.Services;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace CapstoneCC.Pages
{
    public class AddToCartModel : PageModel
    {
        public List<CartItem> Cart { get; set; }
        private readonly IConfiguration _configuration;
        public AddToCartModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public void OnGet()
        {
            Cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            foreach (var item in Cart)
            {
                item.TotalPrice = item.Price * item.Quantity;
            }
            HttpContext.Session.SetObjectAsJson("Cart", Cart);
            HttpContext.Session.CommitAsync();
        }
        public IActionResult OnPostAddToCart([FromBody] CartItem cartItem)
        {
            // Retrieve the cart from session
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Check if the item already exists in the cart
            var existingItem = cart.FirstOrDefault(c => c.ProductId == cartItem.ProductId);
            if (existingItem != null)
            {
                // Update the quantity and total price
                existingItem.Quantity += cartItem.Quantity;
                existingItem.TotalPrice += cartItem.TotalPrice;
            }
            else
            {
                // Add new item to cart
                cart.Add(cartItem);
            }

            // Save the updated cart to the session
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return new JsonResult(new { success = true });
        }
        public IActionResult OnPostRemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            var itemToRemove = cart.FirstOrDefault(c => c.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            // Force session save
            HttpContext.Session.CommitAsync();

            return RedirectToPage();
        }
        public IActionResult OnPostUpdateQuantity([FromBody] UpdateQuantityRequest request)
        {
            if (request == null || request.ProductId <= 0 || request.NewQuantity <= 0)
            {
                return BadRequest(new { message = "Invalid ProductId or Quantity" });
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var itemToUpdate = cart.FirstOrDefault(c => c.ProductId == request.ProductId);
            if (itemToUpdate == null)
            {
                return BadRequest(new { message = "Product not found in cart" });
            }

            itemToUpdate.Quantity = request.NewQuantity;
            itemToUpdate.TotalPrice = itemToUpdate.Price * request.NewQuantity;
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            var totalCartPrice = cart.Sum(c => c.TotalPrice);
            return new JsonResult(new { success = true, totalCartPrice });
        }

        public class UpdateQuantityRequest
        {
            public int ProductId { get; set; }
            public int NewQuantity { get; set; }
        }
        public IActionResult OnPostSubmitOrder(string paymentMethod)
        {
            // Retrieve the cart from the session
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                ModelState.AddModelError(string.Empty, "Your cart is empty.");
                return Page();
            }

            // Retrieve logged-in user details
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string firstName, lastName, address, phoneNumber, email;

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Fetch user details from the database
                using (var command = new SqlCommand("SELECT FirstName, LastName, Address, PhoneNumber, Email FROM AspNetUsers WHERE Id = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            firstName = reader["FirstName"].ToString();
                            lastName = reader["LastName"].ToString();
                            address = reader["Address"].ToString();
                            phoneNumber = reader["PhoneNumber"].ToString();
                            email = reader["Email"].ToString();
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "User details not found.");
                            return Page();
                        }
                    }
                }

                // Insert order into the Orders table
                int orderId;
                using (var command = new SqlCommand(@"
                INSERT INTO Orders (FirstName, LastName, Address, PhoneNumber, Email, OrderDate, PaymentMethod)
                VALUES (@FirstName, @LastName, @Address, @PhoneNumber, @Email, GETDATE(), @PaymentMethod);
                SELECT CAST(SCOPE_IDENTITY() AS INT);", connection))
                {
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@LastName", lastName);
                    command.Parameters.AddWithValue("@Address", address);
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@PaymentMethod", paymentMethod);

                    orderId = (int)command.ExecuteScalar();
                }

                // Insert products into the OrderProducts table
                foreach (var item in cart)
                {
                    using (var productCommand = new SqlCommand(@"
                INSERT INTO OrderProducts (OrderId, ProductName, Price, Quantity)
                VALUES (@OrderId, @ProductName, @Price, @Quantity);", connection))
                    {
                        productCommand.Parameters.AddWithValue("@OrderId", orderId);
                        productCommand.Parameters.AddWithValue("@ProductName", item.Name);
                        productCommand.Parameters.AddWithValue("@Price", item.Price);
                        productCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                        productCommand.ExecuteNonQuery();
                    }
                }
            }

            // Clear the cart and set success message
            HttpContext.Session.Remove("Cart");
            TempData["OrderSuccess"] = "Order submitted successfully!";
            return RedirectToPage("/Helper/OrderSuccess");
        }
        public IActionResult OnPostCheckoutSelected(List<int> SelectedProducts)
        {
            if (SelectedProducts == null || !SelectedProducts.Any())
            {
                ModelState.AddModelError("", "Please select at least one product to checkout.");
                return Page(); // Returns the current page.
            }

            // Process the selected products for checkout
            return RedirectToPage("/OrderSummary");
        }
    }
}
