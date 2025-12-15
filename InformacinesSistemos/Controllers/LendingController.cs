using InformacinesSistemos.Data;
using InformacinesSistemos.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InformacinesSistemos.Controllers
{
    public class LendingController : Controller
    {
        private readonly LibraryContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public LendingController(LibraryContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> LendBook(int bookId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var profile = await _db.UserAccounts.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (profile == null)
                return Unauthorized();

            var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == bookId);
            if (book == null)
                return NotFound();

            var isBookTaken = await _db.Loans.AnyAsync(l => l.BookId == bookId && l.ReturnDate == null);
            if (isBookTaken)
            {
                TempData["LendMessage"] = "Knyga šiuo metu paimta.";
                TempData["LendStatus"] = "error";
                return RedirectToAction("Index", "Home");
            }

            var loan = new Loan
            {
                BookId = bookId,
                UserId = profile.Id,
                LoanDate = DateOnly.FromDateTime(DateTime.UtcNow),
                AccumulatedPenalties = 0,
                ExtensionCount = 0
            };

            _db.Loans.Add(loan);
            await _db.SaveChangesAsync();

            TempData["LendMessage"] = "Knyga sėkmingai paimta.";
            TempData["LendStatus"] = "success";

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ReturnBook(int id)
        {
            var loan = await _db.Loans
                .Include(l => l.Book)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loan == null)
            {
                return NotFound();
            }

            return View(loan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReturn(int id)
        {
            var loan = await _db.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (loan == null)
            {
                return NotFound();
            }

            if (loan.ReturnDate == null)
            {
                loan.ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("ViewRecommendation", "Recommendation", new
            {
                bookTitle = loan.Book?.Title,
                showReturnConfirmation = true,
                returnedBookId = loan.BookId,
                loanId = loan.Id
            });
        }
    }
}
