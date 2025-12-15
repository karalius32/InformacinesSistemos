using InformacinesSistemos.Data;
using InformacinesSistemos.Models;
using InformacinesSistemos.Models.Enums;
using InformacinesSistemos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InformacinesSistemos.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(LibraryContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private async Task<UserAccount?> GetCurrentProfileAsync(bool asNoTracking = true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var q = _db.UserAccounts.AsQueryable();
            if (asNoTracking) q = q.AsNoTracking();

            return await q.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
        }

        // Tik Librarian gali kurti/redaguoti/trinti (Admin - NEGALI).
        private static bool CanManageBooks(UserAccount profile)
            => profile.Role == UserRole.Librarian;

        // --- CREATE ---
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create(string? returnUrl = null)
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Account");
            if (!CanManageBooks(profile)) return Forbid();

            return View(new BookCreateViewModel { ReturnUrl = returnUrl });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCreateViewModel vm)
        {
            var profile = await GetCurrentProfileAsync(asNoTracking: true);
            if (profile == null) return RedirectToAction("Login", "Account");
            if (!CanManageBooks(profile)) return Forbid();

            if (!ModelState.IsValid)
                return View(vm);

            var book = new Book
            {
                Title = vm.Title,
                Identifier = vm.Identifier,
                PublishDate = vm.PublishDate,
                Description = vm.Description,
                PageCount = vm.PageCount,
                Publisher = vm.Publisher,
                Language = vm.Language,
                Format = vm.Format,
                CoverUrl = vm.CoverUrl,
                Keywords = vm.Keywords,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = null
            };

            _db.Books.Add(book);
            await _db.SaveChangesAsync();

            // Jei įvesta autoriaus info – susiejam su Author ir BookAuthor
            var fn = vm.AuthorFirstName?.Trim();
            var ln = vm.AuthorLastName?.Trim();

            if (!string.IsNullOrWhiteSpace(fn) || !string.IsNullOrWhiteSpace(ln))
            {
                var author = await _db.Authors.FirstOrDefaultAsync(a =>
                    (a.FirstName ?? "").ToLower() == (fn ?? "").ToLower() &&
                    (a.LastName ?? "").ToLower() == (ln ?? "").ToLower());

                if (author == null)
                {
                    author = new Author
                    {
                        FirstName = fn,
                        LastName = ln
                    };
                    _db.Authors.Add(author);
                    await _db.SaveChangesAsync();
                }

                var linkExists = await _db.BookAuthors.AnyAsync(ba => ba.BookId == book.Id && ba.AuthorId == author.Id);
                if (!linkExists)
                {
                    _db.BookAuthors.Add(new BookAuthor
                    {
                        BookId = book.Id,
                        AuthorId = author.Id,
                        Role = AuthorRole.Author
                    });
                    await _db.SaveChangesAsync();
                }
            }

            TempData["Success"] = "Knyga sėkmingai sukurta.";

            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectToAction("Edit", new { id = book.Id });
        }

        // --- EDIT ---
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? returnUrl = null)
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Account");
            if (!CanManageBooks(profile)) return Forbid();

            var book = await _db.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            var vm = new BookEditViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Identifier = book.Identifier,
                PublishDate = book.PublishDate,
                UpdatedDate = book.UpdatedDate,
                Description = book.Description,
                PageCount = book.PageCount,
                Publisher = book.Publisher,
                Language = book.Language,
                Format = book.Format,
                CreatedDate = book.CreatedDate,
                CoverUrl = book.CoverUrl,
                Keywords = book.Keywords,
                AuthorsDisplay = (book.BookAuthors == null || !book.BookAuthors.Any())
                    ? "-"
                    : string.Join(", ",
                        book.BookAuthors
                            .Select(ba => $"{ba.Author?.FirstName} {ba.Author?.LastName}".Trim())
                            .Where(s => !string.IsNullOrWhiteSpace(s))),
                ReturnUrl = returnUrl
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookEditViewModel vm)
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Account");
            if (!CanManageBooks(profile)) return Forbid();

            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(vm);

            var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();

            book.Title = vm.Title;
            book.Identifier = vm.Identifier;
            book.PublishDate = vm.PublishDate;
            book.Description = vm.Description;
            book.PageCount = vm.PageCount;
            book.Publisher = vm.Publisher;
            book.Language = vm.Language;
            book.Format = vm.Format;
            book.CoverUrl = vm.CoverUrl;
            book.Keywords = vm.Keywords;
            book.UpdatedDate = DateOnly.FromDateTime(DateTime.UtcNow);

            await _db.SaveChangesAsync();

            TempData["Success"] = "Knyga sėkmingai atnaujinta.";

            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        // --- DELETE ---
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl)
        {
            var profile = await GetCurrentProfileAsync();
            if (profile == null) return RedirectToAction("Login", "Account");
            if (!CanManageBooks(profile)) return Forbid();

            bool isLoaned = await _db.Loans.AnyAsync(l => l.BookId == id);
            if (isLoaned)
            {
                TempData["Error"] = "Knyga yra paskolinta, todėl jos ištrinti negalima.";
                return RedirectToAction("Edit", new { id, returnUrl });
            }

            var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();

            _db.Ratings.RemoveRange(_db.Ratings.Where(r => r.BookId == id));
            _db.Recommendations.RemoveRange(_db.Recommendations.Where(r => r.BookId == id));
            _db.BookAuthors.RemoveRange(_db.BookAuthors.Where(ba => ba.BookId == id));
            _db.BookCategories.RemoveRange(_db.BookCategories.Where(bc => bc.BookId == id));

            _db.Books.Remove(book);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Knyga ištrinta.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
