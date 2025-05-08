using CapstoneCC.Models;
using CapstoneCC.Services;
using DinkToPdf;
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
            // Load the libwkhtmltox.dll
            var context = new CustomAssemblyLoadContext();
            context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/lib/libwkhtmltox.dll"));

            // Ensure that Transactions are populated
            if (Transactions == null || !Transactions.Any())
            {
                Transactions = await _context.SalesTransactions
                    .OrderByDescending(t => t.TransactionDate)
                    .ToListAsync();
            }

            if (Transactions == null || !Transactions.Any())
            {
                return NotFound("No sales transactions available.");
            }

            var converter = new SynchronizedConverter(new PdfTools());

            // Build the HTML for the PDF
            string htmlContent = @"
            <style>
                table {
                    border-collapse: collapse;
                    width: 80%;
                    text-align: center;
                    margin: 0 auto;
                    page-break-inside: avoid;
                }
                th, td {
                    border: 1px solid #000;
                    padding: 4px 8px;
                }
                thead {
                    display: table-header-group;
                }
                tr {
                    page-break-inside: avoid;
                    page-break-after: auto;
                }
            </style>
            <div style='text-align: center;'>
                <h1>Sales Transactions</h1>
            </div>
            <table>
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

            var pdfBytes = converter.Convert(pdfDocument);

            return File(pdfBytes, "application/pdf", "SalesReport.pdf");
        }
    }
}
