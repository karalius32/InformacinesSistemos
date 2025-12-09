using Microsoft.AspNetCore.Mvc;

namespace InformacinesSistemos.Controllers
{
    public class RecommendationController : Controller
    {
        public IActionResult ViewRecommendation(string? bookTitle, bool showReturnConfirmation = false)
        {
            ViewBag.BookTitle = bookTitle;
            ViewBag.ShowReturnConfirmation = showReturnConfirmation;
            return View();
        }
    }
}
