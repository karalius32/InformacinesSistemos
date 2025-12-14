using InformacinesSistemos.Data;
using InformacinesSistemos.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InformacinesSistemos.Controllers
{
    public class SearchController : Controller
    {
        private readonly LibraryContext _db;

        public SearchController(LibraryContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q)
        {
            // Čia esminis triukas: forma visada siunčia ?q=...
            // Net jei q tuščias, query rakte "q" bus.
            bool submitted = Request.Query.ContainsKey("q");

            var vm = new SearchViewModel
            {
                Query = q,
                ShowResults = submitted
            };

            // Jei dar nepaspausta "Ieškoti" (t.y. tiesiog atėjai į /Search) – nerodom lentelės.
            if (!submitted)
                return View(vm);

            // Jei paspausta "Ieškoti" su tuščiu q – rodome visas knygas (pvz. 50).
            var booksQuery = _db.Books
                .AsNoTracking()
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                var pattern = term + "%"; // pradžia

                booksQuery = booksQuery.Where(b =>
                    (!string.IsNullOrEmpty(b.Title) && EF.Functions.ILike(b.Title, pattern)) ||
                    (!string.IsNullOrEmpty(b.Identifier) && EF.Functions.ILike(b.Identifier, pattern)) ||
                    (!string.IsNullOrEmpty(b.Publisher) && EF.Functions.ILike(b.Publisher, pattern))
                );
            }

            vm.Results = await booksQuery
                .OrderBy(b => b.Title)
                .Take(50)
                .ToListAsync();

            return View(vm);
        }
    }
}
