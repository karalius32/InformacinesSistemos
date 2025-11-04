using Microsoft.AspNetCore.Mvc;

namespace InformacinesSistemos.Controllers
{
    public class LendingController : Controller
    {
        public IActionResult LendBook() => View();

        public IActionResult ReturnBook() => View();
    }
}
