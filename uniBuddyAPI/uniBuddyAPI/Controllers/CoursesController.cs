using Microsoft.AspNetCore.Mvc;

namespace uniBuddyAPI.Controllers
{
    public class CoursesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
