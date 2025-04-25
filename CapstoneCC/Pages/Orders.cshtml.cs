using CapstoneCC.Models;
using CapstoneCC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CapstoneCC.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Net.Mail;
using System.Net;   
using MailKit.Security;
using MimeKit;
using SkiaSharp;


namespace CapstoneCC.Pages
{
    public class OrdersModel : PageModel
    {
        private readonly IEmailService _emailService;
        private readonly ApplicationDbContext _context;

        [BindProperty]
        public string PaymentMethod { get; set; }
        public string UserId { get; set; }
        public List<CartItem> CartItems { get; set; }
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRazorViewEngine _viewEngine;
        public OrdersModel(
            IEmailService emailService,
            IConfiguration configuration,
            IRazorViewEngine viewEngine,
            IServiceProvider serviceProvider,
            ApplicationDbContext context)
        {
            _emailService = emailService;
            _configuration = configuration;
            _viewEngine = viewEngine;
            _serviceProvider = serviceProvider;
            _context = context;
        }
        public List<OrderModel.Order> Orders { get; set; } = new List<OrderModel.Order>();
        public void OnGet()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Fetch orders
                using (var command = new SqlCommand("SELECT * FROM Orders ORDER BY OrderDate DESC", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var order = new OrderModel.Order
                            {
                                OrderId = (int)reader["OrderId"],
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Address = reader["Address"].ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString(),
                                Email = reader["Email"].ToString(),
                                OrderDate = (DateTime)reader["OrderDate"],
                                OrderStatus = reader["OrderStatus"]?.ToString() ?? "Pending", // Include OrderStatus
                                Products = new List<OrderModel.OrderProduct>()
                            };

                            Orders.Add(order);
                        }
                    }
                }

                // Fetch products for each order
                foreach (var order in Orders)
                {
                    using (var productCommand = new SqlCommand("SELECT * FROM OrderProducts WHERE OrderId = @OrderId", connection))
                    {
                        productCommand.Parameters.AddWithValue("@OrderId", order.OrderId);
                        using (var productReader = productCommand.ExecuteReader())
                        {
                            while (productReader.Read())
                            {
                                order.Products.Add(new OrderModel.OrderProduct
                                {
                                    ProductName = productReader["ProductName"].ToString(),
                                    Price = (decimal)productReader["Price"],
                                    Quantity = (int)productReader["Quantity"]
                                });
                            }
                        }
                    }
                }
            }
        }

        [HttpGet("Orders/GetOrderDetails")]
        public IActionResult OnGetOrderDetails(int orderId)
        {
            OrderModel.Order orderDetails = null;

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Orders WHERE OrderId = @OrderId";
                    command.Parameters.AddWithValue("@OrderId", orderId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            orderDetails = new OrderModel.Order
                            {
                                OrderId = (int)reader["OrderId"],
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Address = reader["Address"].ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString(),
                                Email = reader["Email"].ToString(),
                                OrderDate = (DateTime)reader["OrderDate"],
                                Products = new List<OrderModel.OrderProduct>()
                            };
                        }
                    }
                }

                if (orderDetails != null)
                {
                    using (var productCommand = connection.CreateCommand())
                    {
                        productCommand.CommandText = "SELECT * FROM OrderProducts WHERE OrderId = @OrderId";
                        productCommand.Parameters.AddWithValue("@OrderId", orderId);

                        using (var productReader = productCommand.ExecuteReader())
                        {
                            while (productReader.Read())
                            {
                                orderDetails.Products.Add(new OrderModel.OrderProduct
                                {
                                    ProductName = productReader["ProductName"].ToString(),
                                    Price = (decimal)productReader["Price"],
                                    Quantity = (int)productReader["Quantity"]
                                });
                            }
                        }
                    }
                }
            }

            if (orderDetails != null)
            {
                return new JsonResult(new { success = true, data = orderDetails });
            }

            return new JsonResult(new { success = false });
        }


        private string RenderPartialViewToString(string viewName, object model)
        {
            var actionContext = new ActionContext(HttpContext, RouteData, PageContext.ActionDescriptor);
            using (var sw = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(actionContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"View '{viewName}' was not found.");
                }

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                    {
                        Model = model
                    },
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                viewResult.View.RenderAsync(viewContext).Wait();
                return sw.GetStringBuilder().ToString();
            }
        }

        public void OnPost()
        {
            // Process the order based on payment method
            if (PaymentMethod == "paypal")
            {
                // Handle PayPal payment (e.g., save the transaction ID, etc.)
            }
            else if (PaymentMethod == "cod")
            {
                // Handle COD payment (e.g., create an order with 'COD' as the payment method)
            }
            else
            {
                // Handle invalid payment method
            }
        }

        [HttpPost]
        public IActionResult OnPostDeleteOrder(int orderId)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Delete products associated with the order
                    using (var deleteProductsCommand = new SqlCommand("DELETE FROM OrderProducts WHERE OrderId = @OrderId", connection))
                    {
                        deleteProductsCommand.Parameters.AddWithValue("@OrderId", orderId);
                        deleteProductsCommand.ExecuteNonQuery();
                    }

                    // Delete the order itself
                    using (var deleteOrderCommand = new SqlCommand("DELETE FROM Orders WHERE OrderId = @OrderId", connection))
                    {
                        deleteOrderCommand.Parameters.AddWithValue("@OrderId", orderId);
                        deleteOrderCommand.ExecuteNonQuery();
                    }
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error deleting order: {ex.Message}");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }
        public async Task<IActionResult> OnPostApproveOrder(int orderId, string cashierName)
        {
            // 1. Try to find the order using the orderId.
            var order = await _context.Orders.FindAsync(orderId);

            // 2. Check if the order exists
            if (order == null)
            {
                // If not, return an error response (e.g., redirect or show a message)
                return NotFound("Order not found.");
            }
            if (string.IsNullOrEmpty(order.OrderStatus))
            {
                // Set a default status if it's null
                order.OrderStatus = "Pending";
            }
            // 3. Approve the order
            order.OrderStatus = "Approved";
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            // 4. Calculate totalAmount from the order's products
            if (order.OrderProducts == null)
            {
                order.OrderProducts = new List<OrderModel.OrderProduct>();
            }
            decimal totalAmount = order.OrderProducts.Sum(op => op.Price * op.Quantity);

            // 5. Ensure cashierName is set to "Online" if it's null or empty
            if (string.IsNullOrEmpty(cashierName))
            {
                cashierName = "Online";
            }

            // 6. Create a new sales transaction record
            var salesTransaction = new SalesTransaction
            {
                TransactionId = Guid.NewGuid().ToString(),
                TransactionDate = DateTime.Now,
                CashierName = cashierName,
                Amount = totalAmount
            };
            _context.SalesTransactions.Add(salesTransaction);

            // 7. Save changes to the database
            await _context.SaveChangesAsync();

            // 8. Send the email notification (assuming _emailService is configured correctly)
            try
            {
                var customerEmail = order.Email;
                var customerName = order.FirstName + " " + order.LastName;
                await _emailService.SendOrderProcessedEmailAsync(customerEmail, customerName, orderId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }

            // 9. Redirect to the sales page
            return RedirectToPage("/Sales", new { transactionId = salesTransaction.TransactionId });
        }
        
        private void DeductInventory(List<OrderModel.OrderProduct> products)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var product in products)
                {
                    // Find the actual product in the inventory based on product name
                    using (var command = new SqlCommand(@"
                UPDATE Products
                SET Stock = Stock - @Quantity
                WHERE ProductName = @ProductName AND Stock >= @Quantity", connection))
                    {
                        command.Parameters.AddWithValue("@Quantity", product.Quantity);
                        command.Parameters.AddWithValue("@ProductName", product.ProductName); // Use ProductName as identifier

                        var rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"Insufficient stock for Product: {product.ProductName}");
                        }
                    }
                }
            }
        }

        public IActionResult OnPostDeclineOrder(int orderId)
        {
            // Implement logic to decline the order
            // For example, delete the order from the database
            DeleteOrder(orderId);
            return RedirectToPage();
        }
        private void UpdateOrderStatus(int orderId, string status)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(@"
                    UPDATE Orders
                    SET Status = @Status
                    WHERE OrderId = @OrderId", connection))
                    {
                        command.Parameters.AddWithValue("@Status", status);
                        command.Parameters.AddWithValue("@OrderId", orderId);

                        var rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            Console.WriteLine($"No order found with OrderId: {orderId}");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
            }
        }

        private void DeleteOrder(int orderId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Delete associated products from OrderProducts
                using (var deleteProductsCommand = new SqlCommand("DELETE FROM OrderProducts WHERE OrderId = @OrderId", connection))
                {
                    deleteProductsCommand.Parameters.AddWithValue("@OrderId", orderId);
                    deleteProductsCommand.ExecuteNonQuery();
                }

                // Delete the order itself
                using (var deleteOrderCommand = new SqlCommand("DELETE FROM Orders WHERE OrderId = @OrderId", connection))
                {
                    deleteOrderCommand.Parameters.AddWithValue("@OrderId", orderId);
                    deleteOrderCommand.ExecuteNonQuery();
                }
            }
        }
        public IActionResult OnPostMarkDelivered(int OrderId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Start a transaction to ensure data consistency
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Fetch all the products in the order
                        var orderProducts = new List<OrderModel.OrderProduct>();
                        using (var productCommand = new SqlCommand("SELECT * FROM OrderProducts WHERE OrderId = @OrderId", connection, transaction))
                        {
                            productCommand.Parameters.AddWithValue("@OrderId", OrderId);
                            using (var productReader = productCommand.ExecuteReader())
                            {
                                while (productReader.Read())
                                {
                                    orderProducts.Add(new OrderModel.OrderProduct
                                    {
                                        ProductName = productReader["ProductName"].ToString(),
                                        Price = (decimal)productReader["Price"],
                                        Quantity = (int)productReader["Quantity"]
                                    });
                                }
                            }
                        }

                        // Deduct stock for each product
                        foreach (var orderProduct in orderProducts)
                        {
                            using (var updateStockCommand = new SqlCommand("UPDATE Products SET Stock = Stock - @Quantity WHERE Name = @ProductName", connection, transaction))
                            {
                                updateStockCommand.Parameters.AddWithValue("@Quantity", orderProduct.Quantity);
                                updateStockCommand.Parameters.AddWithValue("@ProductName", orderProduct.ProductName);
                                updateStockCommand.ExecuteNonQuery();
                            }
                        }

                        // Update order status to Delivered
                        using (var updateOrderStatusCommand = new SqlCommand("UPDATE Orders SET OrderStatus = 'Delivered' WHERE OrderId = @OrderId", connection, transaction))
                        {
                            updateOrderStatusCommand.Parameters.AddWithValue("@OrderId", OrderId);
                            updateOrderStatusCommand.ExecuteNonQuery();
                        }

                        // Commit the transaction
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // Rollback in case of an error
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return RedirectToPage(); // Redirect back to the page with the updated order status
        }
    }
}
