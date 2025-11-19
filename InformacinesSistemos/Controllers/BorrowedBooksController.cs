using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace InformacinesSistemos.Controllers;

public class BorrowedBooksController : Controller
{
    public IActionResult Index() => View();
}