using Coinbase.Commerce;
using Coinbase.Commerce.Models;
using InformacinesSistemos.Data;
using InformacinesSistemos.Models;
using InformacinesSistemos.Models.Enums;
using InformacinesSistemos.Services;
using InformacinesSistemos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InformacinesSistemos.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly CommerceApi _commerce;
        private readonly IConfiguration _cfg;
        private readonly LibraryContext _db;
        private readonly IPaymentService _paymentService;

        public PaymentsController(CommerceApi commerce, IConfiguration cfg, LibraryContext db, IPaymentService paymentService)
        {
            _commerce = commerce;
            _cfg = cfg;
            _db = db;
            _paymentService = paymentService;
        }
        // GET: /Payments/Checkout
        public async Task<IActionResult> Checkout(int invoiceId)
        {
            var invoice = await _db.Invoices.FindAsync(invoiceId);

            if (invoice == null)
            {
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier, Message = "Invoice not found." });
            }

            if (invoice.CoinbaseHostedUrl != null)
            {
                return View("Waiting", new WaitingViewModel
                {
                    InvoiceId = invoiceId,
                    HostedUrl = invoice.CoinbaseHostedUrl
                });
            }

            var charge = new CreateCharge
            {
                Name = invoice.Name,
                PricingType = PricingType.FixedPrice,  // fixed EUR price
                LocalPrice = new Money { Amount = (decimal)invoice.Amount, Currency = "EUR" },
                Metadata =
                {
                    {"userId", User.Identity?.Name ?? "demo" },
                    {"invoiceId", invoice.Id.ToString() }
                }
            };


            var response = await _commerce.CreateChargeAsync(charge);
            
            if (response.HasError())
            {
                // handle error
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier, Message = "Could not create charge." });
            }

            invoice.CoinbaseChargeCode = response.Data.Code;
            invoice.CoinbaseHostedUrl = response.Data.HostedUrl;

            await _db.SaveChangesAsync();

            return View("Waiting", new WaitingViewModel
            {
                InvoiceId = invoiceId,
                HostedUrl = response.Data.HostedUrl
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Route("Payments/Webhook")]
        public async Task<IActionResult> Webhook()
        {
            Console.WriteLine("Webhook hit");
            // 1) Read raw body
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            string pretty;
            try
            {
                pretty = JObject.Parse(body).ToString(Formatting.Indented);
            }
            catch
            {
                pretty = body; // fallback if it's not valid JSON for some reason
            }
            System.IO.File.AppendAllText("webhook_raw.txt", $"----- {DateTime.UtcNow:o} -----\n{pretty}\n\n");
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

            if (webhook == null)
            {
                return BadRequest();
            }

            Console.WriteLine($"Webhook event: {webhook.Event.Type}");

            // The actual charge object
            var charge = webhook.Event.DataAs<Charge>();

            // Example: metadata lookup
            var userId = charge.Metadata?["userId"]?.ToString();

            if (!int.TryParse(charge.Metadata?["invoiceId"]?.ToString(), out var invoiceId))
                return BadRequest("Missing/invalid invoiceId metadata.");

            Console.WriteLine("Invoice ID from metadata: " + invoiceId);

            var invoice = await _db.Invoices.FindAsync(invoiceId);

            if (invoice == null)
            {
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier, Message = "Invoice not found when processing webhook" });
            }

            if (webhook.Event.IsChargeCreated && invoice.Status == InvoiceStatus.New)
            {
                invoice.Status = InvoiceStatus.ChargeCreated;
            }
            else if (webhook.Event.IsChargePending)
            {
                invoice.Status = InvoiceStatus.Pending;
            }
            else if (webhook.Event.IsChargeConfirmed)
            {
                await _paymentService.HandleInvoicePaidAsync(invoice.Id);
            }
            else if (webhook.Event.IsChargeFailed)
            {
                await _paymentService.HandleInvoiceFailedAsync(invoice.Id);
            }

            await _db.SaveChangesAsync();

            return Ok();
        }
        public IActionResult Status()
        {
                return Content($"OK Status hit");
        }

        [HttpGet]
        public IActionResult Waiting(int invoiceId)
        {
            return View(invoiceId);
        }

        [HttpGet]
        [AllowAnonymous] // polling doesn’t need auth, but you can keep it if you want
        public async Task<IActionResult> CheckStatus(int invoiceId)
        {
            var invoice = await _db.Invoices.FindAsync(invoiceId);
            if (invoice == null)
                return NotFound();

            return Json(new
            {
                status = invoice.Status.ToString()
            });
        }


        public IActionResult PayDebt() => View();
        public IActionResult StatusDebt() => View();
    }
}
