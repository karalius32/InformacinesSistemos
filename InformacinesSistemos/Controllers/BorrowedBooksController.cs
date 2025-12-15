using InformacinesSistemos.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InformacinesSistemos.Controllers;

public class BorrowedBooksController : Controller
{
    private readonly LibraryContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public BorrowedBooksController(LibraryContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var profile = await _db.UserAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

        if (profile == null)
        {
            return Unauthorized();
        }

        var loans = await _db.Loans
            .Where(l => l.UserId == profile.Id)
            .Include(l => l.Book)
            .AsNoTracking()
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();

        return View(loans);
    }
}
