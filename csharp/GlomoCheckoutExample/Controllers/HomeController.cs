using Microsoft.AspNetCore.Mvc;

namespace GlomoCheckoutExample.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
