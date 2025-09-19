using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using uniBuddyAPI.Models;
using uniBuddyAPI.Services;

namespace uniBuddyAPI.Controllers
{
    [ApiController]
    [Route("timetable")]
    [Produces("application/json")]
    public class TimetableController : ControllerBase
    {
        private readonly RealTimeDbService _db;
        public TimetableController(RealTimeDbService db) => _db = db;

        // GET will return the timetable for the student
        [HttpGet]
        public async Task<IActionResult> GetTimetable()
        {
            var res = await _db.Client.GetAsync("/timetable.json");
            if (!res.IsSuccessStatusCode) return Ok(new List<Timetable>());

            var json = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return Ok(new List<Timetable>());

            var list = JsonSerializer.Deserialize<List<Timetable>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Timetable>();

            return Ok(list);
        }
    }
}