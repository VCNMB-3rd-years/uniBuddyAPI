using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using uniBuddyAPI.Models;
using uniBuddyAPI.Services;

namespace uniBuddyAPI.Controllers
{
    [ApiController]
    [Route("assignments")]
    [Produces("application/json")]
    public class AssignmentsController : Controller
    {
        private readonly RealTimeDbService _db;
        public AssignmentsController(RealTimeDbService db)
        {
            _db = db;
        }


        [HttpPost("{assignmentId}/{userId}")]
        public async Task<IActionResult> Create(string assignmentId, string userId, [FromBody] AssignmentuploadRequest body)
        {
            if (string.IsNullOrWhiteSpace(assignmentId))
                return BadRequest(new { message = "assignmentId is required" });
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "userId is required" });
            if (body is null)
                return BadRequest(new { message = "Request body is required" });
            if (string.IsNullOrWhiteSpace(body.FileName) || string.IsNullOrWhiteSpace(body.FileUrl))
                return BadRequest(new { message = "fileName and fileUrl are required" });


            var upload = new AssignmentUpload
            {
                AssignmentId = assignmentId,
                UserId = userId,
                FileName = body.FileName,
                FileUrl = body.FileUrl,
                UploadedAt = DateTime.UtcNow
            };


            var response = await _db.Client.PostAsJsonAsync($"/assignmentUploads/{assignmentId}/{userId}.json", upload);
            var fbText = await response.Content.ReadAsStringAsync();


            if (response.IsSuccessStatusCode)
            {
                return Ok(new
                {
                    message = "Upload saved",
                    data = fbText
                });
            }


            return BadRequest(new
            {
                message = "Upload save failed",
                status = (int)response.StatusCode,
                firebase = fbText
            });
        }


        [HttpGet("{assignmentId}/{userId}")]
        public async Task<IActionResult> GetForUser(string assignmentId, string userId)
        {
            if (string.IsNullOrWhiteSpace(assignmentId))
                return BadRequest(new { message = "assignmentId is required" });
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "userId is required" });

            var response = await _db.Client.GetAsync($"/assignmentUploads/{assignmentId}/{userId}.json");
            if (!response.IsSuccessStatusCode) return BadRequest(new { message = "Could not load uploads" });

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return Ok(new List<AssignmentUpload>());

            Dictionary<string, AssignmentUpload>? map;
            try
            {
                map = JsonSerializer.Deserialize<Dictionary<string, AssignmentUpload>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return Ok(new List<AssignmentUpload>());
            }

            var list = new List<AssignmentUpload>();
            if (map != null)
            {
                foreach (var (key, value) in map)
                {
                    value.UploadId = string.IsNullOrWhiteSpace(value.UploadId) ? key : value.UploadId;
                    value.AssignmentId = string.IsNullOrWhiteSpace(value.AssignmentId) ? assignmentId : value.AssignmentId;
                    value.UserId = string.IsNullOrWhiteSpace(value.UserId) ? userId : value.UserId;
                    list.Add(value);
                }
            }

            return Ok(list.OrderByDescending(u => u.UploadedAt).ToList());
        }

    }
}

//Reference List:
//Khan, Z. 2020. JSON.NET Case Insensitive Deserialization not working. [Online]. StackOverflow. Available at: https://stackoverflow.com/questions/45782127/json-net-case-insensitive-deserialization-not-working [Accessed 20 September 2025].

