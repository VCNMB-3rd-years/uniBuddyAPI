using Microsoft.AspNetCore.Mvc;
using uniBuddyAPI.Models;
using uniBuddyAPI.Services;

namespace uniBuddyAPI.Controllers
{
    [ApiController]
    [Route("study-sessions")]
    [Produces("application/json")]
    public class StudySessionsController : Controller
    {
        private readonly RealTimeDbService _db;
        public StudySessionsController(RealTimeDbService db)
        {
            _db = db;
        }


    }
}
