using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using uniBuddyAPI.Services;

namespace uniBuddyAPI.Controllers
{
    [ApiController]
    [Route("courses")]
    public class CoursesController : ControllerBase
    {
        private readonly RealTimeDbService _db;

        public CoursesController(RealTimeDbService db)
        {
            _db = db;
        }

        [HttpGet("files/{courseCode}/{type}")]
        public async Task<IActionResult> GetCourseFile(string courseCode, string type)
        {
            try
            {
                //endpoint for android buttons
                var endpoint = $"/files/{courseCode}/{type}Url.json";

                //fetching url from db
                var response = await _db.Client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    return NotFound(new { message = "File not found in Firebase" });
                }

                var json = await response.Content.ReadAsStringAsync();
                var downloadUrl = JsonSerializer.Deserialize<string>(json);

                if (string.IsNullOrEmpty(downloadUrl))
                    return NotFound(new { message = "No download link found" });

                //returns url as JSON object
                return Ok(new { url = downloadUrl });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching file link", error = ex.Message });
            }
        }
    }
}
