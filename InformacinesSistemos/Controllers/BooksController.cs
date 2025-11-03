using Microsoft.AspNetCore.Mvc;
using System.Linq;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Controllers
{
    public class BooksController : Controller
    {
        public IActionResult Create() => View();

        public IActionResult Edit(int? id)
        {
            var book = FakeData.Books.FirstOrDefault(b => b.Id == id);
            if (book == null) return NotFound();
            return View(book);
        }
    }
    
}
