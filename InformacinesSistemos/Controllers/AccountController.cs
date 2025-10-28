using Microsoft.AspNetCore.Mvc;

namespace InformacinesSistemos.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login() => View();
        public IActionResult Register() => View();
        public IActionResult Logout() => View();
        public IActionResult Edit() => View();
    }
}
