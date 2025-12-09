using InformacinesSistemos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InformacinesSistemos.Controllers
{
    public class LendingController : Controller
    {
        private readonly LibraryContext _db;

        public LendingController(LibraryContext db)
        {
            _db = db;
        }

        public IActionResult LendBook() => View();

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
                showReturnConfirmation = true
            });
        }
    }
}
