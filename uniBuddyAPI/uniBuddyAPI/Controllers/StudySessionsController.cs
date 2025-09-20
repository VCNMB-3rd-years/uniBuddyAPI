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

        [HttpPost("{userId}")]
        public async Task<IActionResult> AddSession(string userId, [FromBody] StudySession body)
        {
            var session = new StudySession
            {
                UserId = userId,
                StartTime = body.StartTime == default ? DateTime.UtcNow : body.StartTime,
                EndTime = body.EndTime,
                Duration = body.Duration,
            };

            var response = await _db.Client.PostAsJsonAsync($"/studySession/{userId}.json", session);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var resultContent = await response.Content.ReadAsStringAsync();
                return Ok(new
                {
                    message = "Study session saved successfully",
                    data = resultContent
                });
            }
            else
            {
                return BadRequest(new { message = "Your study session did not save..."});
            }
        }
    }
}

