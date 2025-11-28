using Coinbase.Commerce;
using Coinbase.Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace InformacinesSistemos.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly CommerceApi _commerce;
        private readonly IConfiguration _cfg;

        public PaymentsController(CommerceApi commerce, IConfiguration cfg)
        {
            _commerce = commerce;
            _cfg = cfg;
        }
        // GET: /Payments/Checkout
        public async Task<IActionResult> Checkout()
        {
            // later: pull plan + price from DB
            var planName = "Basic";
            var priceEur = 9.99m;

            var charge = new CreateCharge
            {
                Name = $"Subscription {planName}",
                Description = "Demo crypto payment",
                PricingType = PricingType.FixedPrice,  // fixed EUR price
                LocalPrice = new Money { Amount = priceEur, Currency = "EUR" },

                RedirectUrl = Url.Action("Status", "Payments", null, Request.Scheme),

                Metadata =
                {
                    {"userId", User.Identity?.Name ?? "demo" },
                    {"plan", planName }
                }
            };

            var response = await _commerce.CreateChargeAsync(charge);
            
            if (response.HasError())
            {
                // handle error
                return View("Error", new { message = "Could not create charge." });
            }

            return Redirect(response.Data.HostedUrl);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Route("Payments/Webhook")]
        public async Task<IActionResult> Webhook()
        {
            // 1) Read raw body
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            // 2) Coinbase signature header
            var signature = Request.Headers["X-CC-Webhook-Signature"].ToString();
            var secret = _cfg["CoinbaseCommerce:WebhookSharedSecret"];

            // 3) Validate authenticity
            if (!WebhookHelper.IsValid(secret, signature, body))
            {
                // spoofed / invalid
                return Unauthorized();
            }

            // 4) Safe to deserialize now
            var webhook = JsonConvert.DeserializeObject<Webhook>(body);

            // The actual charge object
            var charge = webhook.Event.DataAs<Charge>();

            // Example: metadata lookup
            var userId = charge.Metadata?["userId"]?.ToString();
            var plan = charge.Metadata?["plan"]?.ToString();

            // 5) Handle events
            if (webhook.Event.IsChargePending)
            {
                // mark as pending-paid in DB
            }
            else if (webhook.Event.IsChargeConfirmed)
            {
                // mark as paid-final + activate subscription
            }
            else if (webhook.Event.IsChargeFailed)
            {
                // mark as failed/expired
            }

            return Ok();
        }
        public IActionResult Status() => View();
        public IActionResult PayDebt() => View();
        public IActionResult StatusDebt() => View();
    }
}
