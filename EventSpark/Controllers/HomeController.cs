using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventSpark.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // If already logged in, send them to Events (or Dashboard or whatever you prefer)
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Events");
            }

            // Not logged in? Send to Identity login page
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        // you can keep Privacy/Error if they exist, doesn't matter
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
