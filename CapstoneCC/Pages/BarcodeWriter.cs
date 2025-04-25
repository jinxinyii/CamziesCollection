using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing;
using System.IO;

namespace CapstoneCC.Pages
{
    internal class BarcodeWriter : PageModel
    {
        public IActionResult OnGetGenerateBarcode(string productId)
        {
            var barcodeImage = BarcodeHelper.GenerateBarcode(productId);

            return Page();
        }
    }
}