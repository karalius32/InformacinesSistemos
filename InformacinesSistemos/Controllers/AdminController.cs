using InformacinesSistemos.Data;
using InformacinesSistemos.Models;
using InformacinesSistemos.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InformacinesSistemos.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly LibraryContext _db;

        public AdminController(LibraryContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(UserRole? role)
        {
            var query = _db.UserAccounts.AsNoTracking().AsQueryable();
            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            var users = await query.ToListAsync();
            ViewBag.SelectedRole = role;
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int id, UserRole role)
        {
            var user = await _db.UserAccounts.FindAsync(id);
            if (user == null)
                return NotFound();

            user.Role = role;
            await _db.SaveChangesAsync();

            TempData["AdminMessage"] = $"Vartotojo {user.Email} rolė atnaujinta į {role}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
