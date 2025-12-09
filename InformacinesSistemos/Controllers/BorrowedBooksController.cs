using InformacinesSistemos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InformacinesSistemos.Controllers;

public class BorrowedBooksController : Controller
{
    private readonly LibraryContext _db;

    public BorrowedBooksController(LibraryContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var loans = await _db.Loans
            .Include(l => l.Book)
            .AsNoTracking()
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();

        return View(loans);
    }
}
