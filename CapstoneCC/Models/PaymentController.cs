using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PayPalCheckoutSdk.Orders;

namespace CapstoneCC.Models
{
    public class PaymentController : Controller
    {
        private readonly PayPalService _payPalService;

        public PaymentController(PayPalService payPalService)
        {
            _payPalService = payPalService;
        }

        public async Task<IActionResult> CreatePayment()
        {
            var returnUrl = Url.Action("Success", "Payment", null, Request.Scheme);
            var cancelUrl = Url.Action("Cancel", "Payment", null, Request.Scheme);

            var response = await _payPalService.CreateOrder("PHP", "10.00", returnUrl, cancelUrl);
            var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

            var approvalLink = result.Links.FirstOrDefault(link => link.Rel == "approve")?.Href;
            if (approvalLink != null)
            {
                return Redirect(approvalLink);
            }

            return View("Error");
        }

        public async Task<IActionResult> Success(string token)
        {
            var response = await _payPalService.CaptureOrder(token);
            var result = response.Result<PayPalCheckoutSdk.Orders.Order>();

            if (result.Status == "COMPLETED")
            {
                return View("Success");
            }

            return View("Error");
        }

        public IActionResult Cancel()
        {
            return View("Cancel");
        }

        [HttpPost]
        public IActionResult WebhookHandler()
        {
            // 1. Read the webhook request body
            string requestBody;
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                requestBody = reader.ReadToEndAsync().Result;
            }

            // 2. Verify the webhook signature (IMPORTANT for security) 
            //    - See PayPal's documentation for signature verification code

            // 3. Parse the webhook event data (JSON)
            dynamic webhookEvent = JsonConvert.DeserializeObject(requestBody);

            // 4. Process the event based on event type
            string eventType = webhookEvent.event_type;
            switch (eventType)
            {
                case "PAYMENT.CAPTURE.COMPLETED":
                    // Handle successful payment capture
                    string transactionId = webhookEvent.resource.id;
                    // ... update your database, send confirmation emails, etc.
                    break;
                // Add cases for other event types
                default:
                    break;
            }

            return Ok(); // Return a 200 OK response to PayPal
        }
    }
}
