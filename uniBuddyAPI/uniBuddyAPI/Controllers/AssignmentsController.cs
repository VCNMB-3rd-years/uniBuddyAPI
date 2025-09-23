using Microsoft.AspNetCore.Mvc;

namespace uniBuddyAPI.Controllers
{
    public class AssignmentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
