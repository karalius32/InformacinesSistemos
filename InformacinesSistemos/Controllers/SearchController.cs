using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using InformacinesSistemos.Models;
using System.Collections.Generic;

namespace InformacinesSistemos.Controllers
{
    public class SearchController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult Results(string? q)
        {
            IEnumerable<BookSimple> items = FakeData.Books;

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();

                items = FakeData.Books.Where(b =>
                    (!string.IsNullOrEmpty(b.Title) && b.Title.StartsWith(term, StringComparison.OrdinalIgnoreCase))
                );
            }

            ViewBag.Query = q;
            return View(items.ToList());
        }
    }
}
