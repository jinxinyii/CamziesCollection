using CapstoneCC.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;

namespace CapstoneCC.Models
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ApplicationDbContext _context;

        public EmailService(IOptions<EmailSettings> emailSettings, ApplicationDbContext context)
        {
            _emailSettings = emailSettings.Value;
            _context = context;  // Assign the injected DbContext to _context
        }

        public async Task SendOrderProcessedEmailAsync(string toEmail, string customerName, int orderId)
        {
            // Fetch the order from the database
            var order = await _context.Orders
                .Where(o => o.OrderId == orderId)
                .Include(o => o.Products) // Ensure Products are loaded with the Order
                .FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception($"Order with ID {orderId} not found.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress(customerName, toEmail));
            message.Subject = $"Your Order #{orderId} is Now Processing";

            message.Body = new TextPart("html")
            {
                Text = $@"
                <p>Dear {customerName},</p>
                <p>Thank you for your order! Your order (ID: {orderId}) is now being processed.</p>
                <p><strong>Order Details:</strong></p>
                <table border='1' style='border-collapse: collapse;'>
                    <thead>
                        <tr>
                            <th>Product Name</th>
                            <th>Quantity</th>
                            <th>Price</th>
                            <th>Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        {string.Join("", order.Products.Select(product => $@"
                            <tr>
                                <td>{product.ProductName}</td>
                                <td>{product.Quantity}</td>
                                <td>₱{product.Price.ToString("F2")}</td>
                                <td>₱{(product.Price * product.Quantity).ToString("F2")}</td>
                            </tr>"))}
                    </tbody>
                </table>
                <p><strong>Overall Total: ₱{order.Products.Sum(product => product.Price * product.Quantity).ToString("F2")}</strong></p>
                <p>Tanza, Cavite, Philippines</p>
                <p>0997 761 6753</p>
                <p>camziescollections@yahoo.com</p>
                <p>Sincerely,</p>
                <p>Camzie's Collections</p>"
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }

    public interface IEmailService
    {
        Task SendOrderProcessedEmailAsync(string toEmail, string customerName, int orderId);
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
    }
}
