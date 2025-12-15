using InformacinesSistemos.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;

namespace InformacinesSistemos.Controllers
{
    public class RecommendationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _cfg;
        private readonly LibraryContext _db;

        public RecommendationController(IHttpClientFactory httpClientFactory, IConfiguration cfg, LibraryContext db)
        {
            _httpClientFactory = httpClientFactory;
            _cfg = cfg;
            _db = db;
        }

        public async Task<IActionResult> ViewRecommendation(string? bookTitle, bool showReturnConfirmation = false, int? returnedBookId = null, int? loanId = null)
        {
            ViewBag.BookTitle = bookTitle;
            ViewBag.ShowReturnConfirmation = showReturnConfirmation;
            ViewBag.ReturnedBookId = returnedBookId;
            var catalogContext = await BuildCatalogContextAsync();
            ViewBag.RecommendedBookId = null;
            ViewBag.RecommendedBookTitle = null;
            ViewBag.RecommendedBookAuthor = null;
            ViewBag.RecommendedBookYear = null;
            ViewBag.Recommendation = null;
            ViewBag.RecommendationError = null;
            int? recommendedBookId = null;
            string? recommendedBookTitle = null;

            // Try existing recommendation for this loan
            if (loanId.HasValue)
            {
                var existingRec = await _db.Recommendations
                    .Include(r => r.Book)
                        .ThenInclude(b => b.BookAuthors)
                            .ThenInclude(ba => ba.Author)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.LoanId == loanId.Value);

                if (existingRec != null)
                {
                    ViewBag.Recommendation = existingRec.RecommendationText;
                    recommendedBookId = existingRec.BookId;
                    recommendedBookTitle = existingRec.Book?.Title;
                    ViewBag.RecommendedBookAuthor = existingRec.Book?.BookAuthors?.Select(ba => $"{ba.Author?.FirstName} {ba.Author?.LastName}".Trim()).FirstOrDefault();
                    ViewBag.RecommendedBookYear = existingRec.Book?.PublishDate?.Year.ToString();
                }
            }

            // If no existing recommendation, generate and save
            if (ViewBag.Recommendation == null)
            {
                var result = await FetchRecommendationAsync(bookTitle, catalogContext);
                if (!string.IsNullOrWhiteSpace(result.Error))
                {
                    ViewBag.RecommendationError = result.Error;
                }
                else
                {
                    var parsed = TryParseRecommendationJson(result.Recommendation);
                    var recommendationText = parsed.ReasonText ?? result.Recommendation;

                    if (loanId.HasValue && !string.IsNullOrWhiteSpace(recommendationText))
                    {
                        var parsedId = parsed.BookId ?? returnedBookId ?? 0;
                        if (parsedId > 0)
                        {
                            var rec = new Models.Recommendation
                            {
                                RecommendationDate = DateTime.UtcNow,
                                RecommendationText = recommendationText,
                                LoanId = loanId.Value,
                                BookId = parsedId
                            };

                            _db.Recommendations.Add(rec);
                            await _db.SaveChangesAsync();

                            // ensure we use the persisted values
                            recommendedBookId = parsedId;
                        }
                    }

                    ViewBag.Recommendation = recommendationText;
                    recommendedBookId ??= parsed.BookId;
                    recommendedBookTitle = parsed.BookTitle;
                    ViewBag.RecommendedBookAuthor = null;
                    ViewBag.RecommendedBookYear = null;
                    ViewBag.RecommendationError = result.Error;
                }
            }

            // Fetch returned book details
            if (returnedBookId.HasValue)
            {
                var returnedBook = await LoadBookDetailsAsync(returnedBookId.Value);
                ViewBag.ReturnedBookTitle = returnedBook.Title ?? bookTitle;
                ViewBag.ReturnedBookAuthor = returnedBook.Author ?? "-";
                ViewBag.ReturnedBookYear = returnedBook.Year ?? "-";
            }
            else
            {
                ViewBag.ReturnedBookTitle = bookTitle;
            }

            // Fetch recommended book details if possible
            if (recommendedBookId.HasValue)
            {
                var recBook = await LoadBookDetailsAsync(recommendedBookId.Value);
                ViewBag.RecommendedBookId = recommendedBookId;
                ViewBag.RecommendedBookTitle = recBook.Title ?? recommendedBookTitle;
                ViewBag.RecommendedBookAuthor = recBook.Author ?? "-";
                ViewBag.RecommendedBookYear = recBook.Year ?? "-";
            }
            else
            {
                ViewBag.RecommendedBookId = recommendedBookId;
                ViewBag.RecommendedBookTitle = recommendedBookTitle;
            }

            return View();
        }

        private async Task<string> BuildCatalogContextAsync()
        {
            var books = await _db.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .AsNoTracking()
                .Take(25)
                .ToListAsync();

            var lines = books.Select(b =>
            {
                var authors = b.BookAuthors.Select(ba => $"{ba.Author?.FirstName} {ba.Author?.LastName}".Trim()).Where(a => !string.IsNullOrWhiteSpace(a));
                var authorsJoined = string.Join(", ", authors);
                return $"{b.Id} | {b.Title ?? "Be pavadinimo"} by {authorsJoined}: {b.Description}";
            });

            return string.Join("\n", lines);
        }

        private async Task<(string? Title, string? Author, string? Year)> LoadBookDetailsAsync(int id)
        {
            var book = await _db.Books
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return (null, null, null);

            var authorNames = book.BookAuthors
                .Select(ba => $"{ba.Author?.FirstName} {ba.Author?.LastName}".Trim())
                .Where(a => !string.IsNullOrWhiteSpace(a));

            var year = book.PublishDate?.Year.ToString();

            return (book.Title, string.Join(", ", authorNames), string.IsNullOrWhiteSpace(year) ? null : year);
        }

        private async Task<(string Recommendation, string? Error)> FetchRecommendationAsync(string? bookTitle, string catalogContext)
        {
            var apiKey = _cfg["OpenAI:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return (string.Empty, "OpenAI API key not configured.");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://api.openai.com");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                model = "gpt-5-mini",
                messages = new[]
                {
                    new { role = "system", content = "Tu esi knygų rekomendacijų asistentas. Visada atsakyk lietuvių kalba ir pasirink tik vieną konkrečią knygą iš pateikto katalogo." },
                    new { role = "user", content = $"Galimos knygos (formatas: ID | Pavadinimas by autoriai: aprašymas):\n{catalogContext}" },
                    new { role = "user", content = $"Parink vieną knygą iš bibliotekos katalogo, panašią į: {bookTitle ?? "knyga"}. Atsakyk lietuvių kalba JSON formatu: {{\"recommended_book_id\": <id>, \"recommended_book_title\": \"<pavadinimas>\", \"reason\": \"<išsamus paaiškinimas (≥200 žodžių), paminint, kad tai mūsų bibliotekos knyga>\"}}." }
                },
                max_completion_tokens = 10000
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync("/v1/chat/completions", content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return (string.Empty, $"OpenAI request failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {errorBody}");
                }

                var body = await response.Content.ReadAsStringAsync();
                dynamic? parsed = JsonConvert.DeserializeObject(body);
                var rec = parsed?.choices?[0]?.message?.content?.ToString() ?? string.Empty;
                return (rec, string.IsNullOrWhiteSpace(rec) ? "OpenAI returned empty content." : null);
            }
            catch (Exception ex)
            {
                return (string.Empty, $"OpenAI call error: {ex.Message}");
            }
        }

        private (int? BookId, string? BookTitle, string? ReasonText) TryParseRecommendationJson(string? rec)
        {
            if (string.IsNullOrWhiteSpace(rec))
                return (null, null, null);

            try
            {
                var obj = JsonConvert.DeserializeObject<dynamic>(rec);
                int? id = null;
                try { id = (int?)obj?.recommended_book_id; } catch { }
                string? title = obj?.recommended_book_title;
                string? reason = obj?.reason;
                return (id, title, reason);
            }
            catch
            {
                return (null, null, rec);
            }
        }
    }
}
