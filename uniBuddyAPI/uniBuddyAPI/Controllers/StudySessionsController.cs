using Microsoft.AspNetCore.Mvc;

namespace uniBuddyAPI.Controllers
{
    public class StudySessionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
