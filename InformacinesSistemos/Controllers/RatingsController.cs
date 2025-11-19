using Microsoft.AspNetCore.Mvc;
using System.Linq;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Controllers
{
    public class RatingsController : Controller
    {
        public IActionResult Create() => View();
    }
}