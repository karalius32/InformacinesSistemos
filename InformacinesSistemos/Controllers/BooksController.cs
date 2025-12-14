using Microsoft.AspNetCore.Mvc;
using System.Linq;
using InformacinesSistemos.Data;
using InformacinesSistemos.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InformacinesSistemos.Controllers
{
    public class BooksController : Controller
    {
        private readonly LibraryContext _db;
        public BooksController(LibraryContext db) { _db = db; }
        public IActionResult Create() => View();

        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? returnUrl = null)
        {
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookEditViewModel vm)
        {
            if (id != vm.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(vm);

            var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();

            // Atnaujinam visus laukus iš formos:
            book.Title = vm.Title;
            book.Identifier = vm.Identifier;
            book.PublishDate = vm.PublishDate;
            book.UpdatedDate = vm.UpdatedDate;     // jei nori auto: book.UpdatedDate = DateTime.UtcNow;
            book.Description = vm.Description;
            book.PageCount = vm.PageCount;
            book.Publisher = vm.Publisher;
            book.Language = vm.Language;
            book.Format = vm.Format;
            book.CoverUrl = vm.CoverUrl;
            book.Keywords = vm.Keywords;

            await _db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl)
        {
            // 1) Jei paskolinta – netrinam
            bool isLoaned = await _db.Loans.AnyAsync(l => l.BookId == id);
            if (isLoaned)
            {
                TempData["Error"] = "Knyga yra paskolinta, todėl jos ištrinti negalima.";
                return RedirectToAction("Edit", new { id, returnUrl });
            }

            // 2) Jei nepaskolinta – trinam (tik vaikinius ryšius, kad FK netrukdytų)
            var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) return NotFound();

            _db.Ratings.RemoveRange(_db.Ratings.Where(r => r.BookId == id));
            _db.Recommendations.RemoveRange(_db.Recommendations.Where(r => r.BookId == id));
            _db.BookAuthors.RemoveRange(_db.BookAuthors.Where(ba => ba.BookId == id));
            _db.BookCategories.RemoveRange(_db.BookCategories.Where(bc => bc.BookId == id));

            _db.Books.Remove(book);
            await _db.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
