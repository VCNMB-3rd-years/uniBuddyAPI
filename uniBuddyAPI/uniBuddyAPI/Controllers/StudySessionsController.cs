using Microsoft.AspNetCore.Mvc;
using System.Globalization;
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
        //new study session for a specific user (logged in user)
        public async Task<IActionResult> AddSession(string userId, [FromBody] StudySession body)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "The userId is required" }); //user must be logged in

            var start = body.StartTime == default ? DateTime.UtcNow : body.StartTime; //start time defaults to NOW
            DateTime? end = body.EndTime; //when the user stops the timer thats the end time

            var duration = body.Duration;
            if (duration <= 0 && end.HasValue)
            {
                //Calculating minutes from start to finish
                duration = (int)Math.Max(0, (end.Value - start).TotalMinutes);
            }

            var session = new StudySession //object being sent to firebase
            {
                UserId = userId,
                StartTime = start,
                EndTime = end,
                Duration = duration
            };

            var response = await _db.Client.PostAsJsonAsync($"/studySession/{userId}.json", session); //posting to firebase
            var fbText = await response.Content.ReadAsStringAsync(); //reading the response from firebase

            if (response.IsSuccessStatusCode) 
            {
                return Ok(new
                {
                    message = "Study session saved successfully", //message to return on success
                    data = fbText
                });
            }

            return BadRequest(new
            {
                message = "Your study session did not save...", //message to return on failure
                status = (int)response.StatusCode, //status code from firebase just for debugging for now in android
                firebase = fbText
            });
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

            Dictionary<string, StudySession>? map; //getting all study sessions for logged in user, stored in dictionary
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
                return Ok(new List<StudySession>()); //just returns an empty list so the app wont crash
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

