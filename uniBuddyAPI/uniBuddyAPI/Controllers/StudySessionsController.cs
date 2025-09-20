using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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
                StartTime = body.StartTime == default ? DateTime.UtcNow : body.StartTime, //adds a start time of right now
                EndTime = body.EndTime, //need to still figure out how to automatically add an end time
                Duration = body.Duration, //end time minus start time (figure out still)
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
                //error message if something goes wrong, app wont crash
                return BadRequest(new { message = "Your study session did not save..." });
            }
        }

        [HttpGet("{userId}")] //getting the logged in users study sessions
        public async Task<IActionResult> GetSessions(string userId)
        {
            var response = await _db.Client.GetAsync($"/studySession/{userId}.json");
            if (!response.IsSuccessStatusCode) return BadRequest(new { message = "Could not load your study sessions" });

            var json = await response.Content.ReadAsStringAsync();
            //returning a message to the user if they have no recorded study sessions
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return NotFound(new { message = "You have no recorded study sessions" });

            Dictionary<string, StudySession>? map;
            try
            {
                //Code Attribution
                //The PropertyNameCaseInsensitive option has been created with the help of StackOverflow
                //https://stackoverflow.com/questions/45782127/json-net-case-insensitive-deserialization-not-working
                //Ziaullah Khan
                //https://stackoverflow.com/users/3312570/ziaullah-khan
                map = JsonSerializer.Deserialize<Dictionary<string, StudySession>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return Ok(new List<StudySession>());
            }

            var list = new List<StudySession>();
            if (map != null)
            {
                foreach (var (key, value) in map)
                {
                    //firebase key is the session id
                    value.SessionId = string.IsNullOrWhiteSpace(value.SessionId) ? key : value.SessionId; //session id is the firebase key
                    list.Add(value);
                }
            }

            //returning the most recent start time first in the list of study sessions
            return Ok(list.OrderByDescending(s => s.StartTime).ToList());
        }
    }
}

