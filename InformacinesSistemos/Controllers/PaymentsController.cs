﻿using Microsoft.AspNetCore.Mvc;

namespace InformacinesSistemos.Controllers
{
    public class PaymentsController : Controller
    {
        public IActionResult Checkout() => View();
        public IActionResult Status() => View();
    }
}
