using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CapstoneCC.Models
{
    public class PayPalService
    {
        private readonly PayPalHttpClient _client;

        public PayPalService(IConfiguration configuration)
        {
            var paypalConfig = configuration.GetSection("PayPal");

            PayPalEnvironment environment;
            if (paypalConfig["Environment"] == "sandbox" )
            {
                environment = new SandboxEnvironment(paypalConfig["ClientId"], paypalConfig["Secret"]);
            }
            else
            {
                environment = new LiveEnvironment(paypalConfig["ClientId"], paypalConfig["Secret"]);
            }


            _client = new PayPalHttpClient(environment);

        }

        public async Task<PayPalHttp.HttpResponse> CreateOrder(string currency, string amount, string returnUrl, string cancelUrl)
        {
            var order = new OrderRequest
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = currency,
                            Value = amount
                        }
                    }
                },
                ApplicationContext = new ApplicationContext
                {
                    ReturnUrl = returnUrl,
                    CancelUrl = cancelUrl
                }
            };

            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(order);

            return await _client.Execute(request);
        }

        public async Task<PayPalHttp.HttpResponse> CaptureOrder(string orderId)
        {
            var request = new OrdersCaptureRequest(orderId);
            request.RequestBody(new OrderActionRequest());

            return await _client.Execute(request);
        }
    }
}