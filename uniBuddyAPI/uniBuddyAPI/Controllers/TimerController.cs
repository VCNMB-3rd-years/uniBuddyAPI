using Microsoft.AspNetCore.Mvc;

namespace uniBuddyAPI.Controllers
{
    public class TimerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
