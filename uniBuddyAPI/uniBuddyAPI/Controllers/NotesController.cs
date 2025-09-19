using Microsoft.AspNetCore.Mvc;

namespace uniBuddyAPI.Controllers
{
    public class NotesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
