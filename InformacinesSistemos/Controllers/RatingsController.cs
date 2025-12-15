using InformacinesSistemos.Data;
using InformacinesSistemos.Models;
using InformacinesSistemos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InformacinesSistemos.Models.Enums;

namespace InformacinesSistemos.Controllers
{
    [Authorize]
    public class RatingsController : Controller
    {
        private readonly LibraryContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public RatingsController(LibraryContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int bookId, string? returnUrl = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _db.UserAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (profile == null) return Forbid();
            if (profile.Role != UserRole.Reader)
            {
                TempData["Error"] = "Tik skaitytojai gali palikti įvertinimus.";
                return RedirectToAction("Index", "Home");
            }
            if (profile.Role != UserRole.Reader)
                return Forbid();
            var book = await _db.Books
                .AsNoTracking()
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(b => b.Id == bookId);

            if (book == null) return NotFound();

            var existing = await _db.Ratings
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == profile.Id);

            var authors = (book.BookAuthors == null || !book.BookAuthors.Any())
                ? "-"
                : string.Join(", ",
                    book.BookAuthors
                        .Select(ba => $"{ba.Author?.FirstName} {ba.Author?.LastName}".Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s)));

            var vm = new RatingCreateViewModel
            {
                BookId = book.Id,
                BookTitle = book.Title,
                AuthorsDisplay = authors,
                RatingValue = existing?.RatingValue ?? 5,
                Comment = existing?.Comment,
                ReturnUrl = returnUrl
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RatingCreateViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var profile = await _db.UserAccounts
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (profile == null) return Forbid();
            if (profile.Role != UserRole.Reader)
            {
                TempData["Error"] = "Tik skaitytojai gali palikti įvertinimus.";
                return RedirectToAction("Index", "Home");
            }
            if (profile.Role != UserRole.Reader)
                return Forbid();
            if (!ModelState.IsValid)
            {
                var book = await _db.Books
                    .AsNoTracking()
                    .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                    .FirstOrDefaultAsync(b => b.Id == vm.BookId);

                if (book != null)
                {
                    vm.BookTitle = book.Title;
                    vm.AuthorsDisplay = (book.BookAuthors == null || !book.BookAuthors.Any())
                        ? "-"
                        : string.Join(", ",
                            book.BookAuthors
                                .Select(ba => $"{ba.Author?.FirstName} {ba.Author?.LastName}".Trim())
                                .Where(s => !string.IsNullOrWhiteSpace(s)));
                }

                return View(vm);
            }

            var bookExists = await _db.Books.AnyAsync(b => b.Id == vm.BookId);
            if (!bookExists) return NotFound();

            var rating = await _db.Ratings
                .FirstOrDefaultAsync(r => r.BookId == vm.BookId && r.UserId == profile.Id);

            if (rating == null)
            {
                rating = new Rating
                {
                    BookId = vm.BookId,
                    UserId = profile.Id
                };
                _db.Ratings.Add(rating);
            }

            rating.RatingValue = vm.RatingValue;
            rating.Comment = string.IsNullOrWhiteSpace(vm.Comment) ? null : vm.Comment.Trim();
            rating.RatingDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
