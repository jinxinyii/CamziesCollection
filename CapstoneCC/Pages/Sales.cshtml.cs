using CapstoneCC.Models;
using DinkToPdf;
using DinkToPdf.Contracts;
using CapstoneCC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CapstoneCC.Pages
{
    public class SalesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public SalesModel(ApplicationDbContext context)
        {
            _context = context;
        }
        public List<SalesTransaction> Transactions { get; set; }

        public async Task OnGet()
        {
            Transactions = await _context.SalesTransactions.OrderByDescending(t => t.TransactionDate).ToListAsync();
        }
        public async Task<IActionResult> OnGetTransactionDetails(string transactionId)
        {
            Transactions = await _context.SalesTransactions
                .Where(t => t.TransactionId == transactionId)
                .ToListAsync();

            return Page();
        }
        public async Task<IActionResult> OnGetDownloadPdf()
        {
            // Ensure that Transactions are populated
            if (Transactions == null || !Transactions.Any())
            {
                // Fetch the Transactions if they are not available
                Transactions = await _context.SalesTransactions
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }

            // Check if Transactions is still empty after the fetch
            if (Transactions == null || !Transactions.Any())
            {
                return NotFound("No sales transactions available.");
            }

            // PDF generation using DinkToPdf
            var converter = new SynchronizedConverter(new PdfTools());

            // Build the HTML for the PDF
            string htmlContent = @"
            <h1>Sales Transactions</h1>
            <table border='1'>
                <thead>
                    <tr>
                        <th>Transaction ID</th>
                        <th>Date</th>
                        <th>Time</th>
                        <th>Cashier</th>
                        <th>Amount</th>
                    </tr>
                </thead>
                <tbody>";

            foreach (var transaction in Transactions)
            {
                htmlContent += $@"
            <tr>
                <td>{transaction.TransactionId}</td>
                <td>{transaction.TransactionDate:yyyy-MM-dd}</td>
                <td>{transaction.TransactionDate:HH:mm:ss}</td>
                <td>{transaction.CashierName}</td>
                <td>{transaction.Amount}</td>
            </tr>";
            }

            htmlContent += @"
            </tbody>
        </table>";

            // Configure PDF settings
            var pdfDocument = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4
                },
                Objects = { new ObjectSettings { HtmlContent = htmlContent } }
            };

            // Convert HTML to PDF
            var pdfBytes = converter.Convert(pdfDocument);

            // Return the PDF file
            return File(pdfBytes, "application/pdf", "SalesReport.pdf");
        }
    }
}
