using InformacinesSistemos.Data;
using InformacinesSistemos.Models;
using InformacinesSistemos.Models.Enums;
using InformacinesSistemos.Models.Library;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Globalization;

namespace InformacinesSistemos.Controllers
{
    public class SubscriptionsController : Controller
    {
        private readonly LibraryContext _db;
        private readonly IConfiguration _cfg;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionsController(LibraryContext db, IConfiguration cfg, 
                                        UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _cfg = cfg;
            _userManager = userManager;
        }

        public IActionResult Index() => View();

        public async Task<int> CreateSubscriptionInvoice()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return -1;

            var profile = await _db.UserAccounts
                                   .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (profile == null)
                return -1;

            var existing = await _db.Invoices.AnyAsync(i =>
                            i.UserId == profile.Id &&
                            i.Status == InvoiceStatus.New);

            if (existing) return -1;

            var invoice = new Invoice
            {
                Name = "Subscription Fee",
                CreatedDate = DateTime.UtcNow,
                Amount = double.Parse(_cfg["Subscription:Price"]!, CultureInfo.InvariantCulture),
                Currency = "EUR",
                Status = InvoiceStatus.New,
                UserId = profile.Id
            };

            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            return invoice.Id;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyWithCrypto()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Account");
            }

            var invoiceId = await CreateSubscriptionInvoice();
            if (invoiceId == -1)
                return View("Error", new ErrorViewModel { RequestId = HttpContext.TraceIdentifier, Message = "Could not create invoice." });

            return RedirectToAction("Checkout", "Payments", new { invoiceId });
        }
    }
}
