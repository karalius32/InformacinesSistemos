using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InformacinesSistemos.ViewModels;
using InformacinesSistemos.Data;
using Microsoft.EntityFrameworkCore;

namespace InformacinesSistemos.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly LibraryContext _db;

    public HomeController(ILogger<HomeController> logger, LibraryContext db)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var books = await _db.Books
            .AsNoTracking()
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
            .OrderBy(b => b.Title)
            .ToListAsync();

        return View(books);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
