using Microsoft.AspNetCore.Mvc;

namespace InformacinesSistemos.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Edit(string id) => View();
    }
}
