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
        public async Task<IActionResult> Index(
            string? q,
            int[] categoryIds,
            int[] authorIds,
            int? yearFrom,
            int? yearTo)
        {
            bool submitted =
                Request.Query.ContainsKey("q") ||
                Request.Query.ContainsKey("categoryIds") ||
                Request.Query.ContainsKey("authorIds") ||
                Request.Query.ContainsKey("yearFrom") ||
                Request.Query.ContainsKey("yearTo");

            var vm = new SearchViewModel
            {
                Query = q,
                ShowResults = submitted,
                SelectedCategoryIds = categoryIds ?? Array.Empty<int>(),
                SelectedAuthorIds = authorIds ?? Array.Empty<int>(),
                YearFrom = yearFrom,
                YearTo = yearTo
            };

            vm.CategoryOptions = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new FilterOption
                {
                    Id = c.Id,
                    Name = c.Name ?? ""
                })
                .ToListAsync();

            vm.AuthorOptions = await _db.Authors
                .AsNoTracking()
                .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
                .Select(a => new FilterOption
                {
                    Id = a.Id,
                    Name = ((a.FirstName ?? "") + " " + (a.LastName ?? "")).Trim()
                })
                .ToListAsync();

            if (!submitted)
                return View(vm);

            var booksQuery = _db.Books
                .AsNoTracking()
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .Include(b => b.BookCategories).ThenInclude(bc => bc.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                var pattern = term + "%";

                booksQuery = booksQuery.Where(b =>
                    !string.IsNullOrEmpty(b.Title) && EF.Functions.ILike(b.Title, pattern)
                );
            }

            if (categoryIds != null && categoryIds.Length > 0)
            {
                booksQuery = booksQuery.Where(b =>
                    b.BookCategories.Any(bc => categoryIds.Contains(bc.CategoryId)));
            }

            if (authorIds != null && authorIds.Length > 0)
            {
                booksQuery = booksQuery.Where(b =>
                    b.BookAuthors.Any(ba => authorIds.Contains(ba.AuthorId)));
            }

            if (yearFrom.HasValue && yearFrom.Value >= 1)
            {
                var from = new DateOnly(yearFrom.Value, 1, 1);
                booksQuery = booksQuery.Where(b => b.PublishDate != null && b.PublishDate.Value >= from);
            }

            if (yearTo.HasValue && yearTo.Value >= 1)
            {
                var to = new DateOnly(yearTo.Value, 12, 31);
                booksQuery = booksQuery.Where(b => b.PublishDate != null && b.PublishDate.Value <= to);
            }

            vm.Results = await booksQuery
                .OrderBy(b => b.Title)
                .Take(50)
                .ToListAsync();

            var bookIds = vm.Results.Select(b => b.Id).ToList();

            var ratingRows = await _db.Ratings
                .AsNoTracking()
                .Where(r => bookIds.Contains(r.BookId))
                .GroupBy(r => r.BookId)
                .Select(g => new
                {
                    BookId = g.Key,
                    Avg = g.Where(x => x.RatingValue != null).Average(x => (double)x.RatingValue!.Value),
                    Count = g.Count(x => x.RatingValue != null)
                })
                .ToListAsync();

            vm.RatingByBookId = ratingRows.ToDictionary(
                x => x.BookId,
                x => new SearchViewModel.RatingSummaryVm
                {
                    AvgRounded = x.Count == 0 ? 0 : (int)Math.Round(x.Avg, 0, MidpointRounding.AwayFromZero),
                    Count = x.Count
                });
            
            foreach (var id in bookIds)
            {
                if (!vm.RatingByBookId.ContainsKey(id))
                    vm.RatingByBookId[id] = new SearchViewModel.RatingSummaryVm { AvgRounded = 0, Count = 0 };
            }

            return View(vm);
        }
    }
}
