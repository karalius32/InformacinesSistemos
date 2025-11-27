using InformacinesSistemos.Data;
using InformacinesSistemos.Models.Enums;
using InformacinesSistemos.Models.Library;
using InformacinesSistemos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InformacinesSistemos.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly LibraryContext _db;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            LibraryContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Neteisingas el. paštas arba slaptažodis.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            var profile = await _db.UserAccounts.FirstOrDefaultAsync(p => p.IdentityUserId == user!.Id);

            if (profile != null)
            {
                profile.LastLogin = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            var profile = new UserAccount
            {
                Email = model.Email,
                RegistrationDate = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IdentityUserId = user.Id,
                Role = UserRole.Reader,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            _db.UserAccounts.Add(profile);
            await _db.SaveChangesAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var profile = await _db.UserAccounts.FirstOrDefaultAsync(p => p.IdentityUserId == user!.Id);
            if (profile == null)
                return NotFound();

            var model = new EditProfileViewModel
            {
                Email = profile.Email ?? user.Email ?? "",
                PhoneNumber = profile.PhoneNumber ?? user.PhoneNumber,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                BirthDate = profile.BirthDate,
                Address = profile.Address
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            var profile = await _db.UserAccounts
                .FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);

            if (profile == null) return NotFound();

            // Update profile table
            profile.FirstName = vm.FirstName;
            profile.LastName = vm.LastName;
            profile.BirthDate = vm.BirthDate;
            profile.Address = vm.Address;
            profile.Email = vm.Email;
            profile.PhoneNumber = vm.PhoneNumber;
            profile.UpdatedAt = DateTime.UtcNow;

            // Update Identity user too (so login/email stays in sync)
            if (user.Email != vm.Email)
            {
                user.Email = vm.Email;
                user.UserName = vm.Email; // because you use email as username
            }

            user.PhoneNumber = vm.PhoneNumber;

            var identityResult = await _userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
            {
                foreach (var e in identityResult.Errors)
                    ModelState.AddModelError("", e.Description);

                return View(vm);
            }

            await _db.SaveChangesAsync();

            return RedirectToAction("Edit"); // or Home/Index
        }
    }
}
