using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using uniBuddyAPI.Models;
using uniBuddyAPI.Services;

namespace uniBuddyAPI.Controllers
{
    [ApiController]
    [Route("goals")]
    [Produces("application/json")]
    public class GoalsController : ControllerBase
    {
        private readonly RealTimeDbService _db;
        public GoalsController(RealTimeDbService db)
        {
            _db = db;
        }

        // GET returns the goals for the student
        [HttpGet]
        public async Task<IActionResult> GetGoals()
        {
            var res = await _db.Client.GetAsync("/goals.json");
            if (!res.IsSuccessStatusCode) return Ok(new List<Goal>());

            var json = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return Ok(new List<Goal>());

            try
            {
                var list = JsonSerializer.Deserialize<List<Goal>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (list != null) return Ok(list);
            }
            catch
            {
               
            }

            var map = JsonSerializer.Deserialize<Dictionary<string, Goal>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return Ok(map?.Values?.ToList() ?? new List<Goal>());
        }
    }
}