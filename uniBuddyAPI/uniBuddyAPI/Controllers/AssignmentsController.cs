using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
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

        //Initialize Firebase app
        private static bool _firebaseInitialized = false;
        public AssignmentsController(RealTimeDbService db)
        {
            _db = db;
        }

        [HttpPost("{assignmentId}/{userId}")]
        public async Task<IActionResult> Create(string assignmentId, string userId, [FromBody] AssignmentuploadRequest body)
        {
            //error messages for missing data so there are no crashes of api
            if (string.IsNullOrWhiteSpace(assignmentId))
                return BadRequest(new { message = "assignmentId is required" });
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "userId is required" });
            if (body is null)
                return BadRequest(new { message = "Request body is required" });
            if (string.IsNullOrWhiteSpace(body.FileName) || string.IsNullOrWhiteSpace(body.FileUrl))
                return BadRequest(new { message = "fileName and fileUrl are required" });

            var upload = new AssignmentUpload
            //assignment object saving to firebase
            {
                AssignmentId = assignmentId,
                UserId = userId,
                FileName = body.FileName,
                FileUrl = body.FileUrl,
                UploadedAt = DateTime.UtcNow
            };

            var response = await _db.Client.PostAsJsonAsync($"/assignmentUploads/{assignmentId}/{userId}.json", upload); //(Microsoft, 2025).
            var fbText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest(new
                {
                    //details on what the issue is if upload fails
                    message = "Upload save failed",
                    status = (int)response.StatusCode,
                    firebase = fbText
                });
            }

            return Ok(new
            {
                //success message
                message = "Upload saved",
                data = fbText
            });
        }

        [HttpGet("{assignmentId}/{userId}")]
        public async Task<IActionResult> GetForUser(string assignmentId, string userId)
        {
            if (string.IsNullOrWhiteSpace(assignmentId))
                return BadRequest(new { message = "assignmentId is required" });
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "userId is required" });

            //getting the uploads from firebase for the user
            var response = await _db.Client.GetAsync($"/assignmentUploads/{assignmentId}/{userId}.json"); //(Microsoft, 2025).
            if (!response.IsSuccessStatusCode) return BadRequest(new { message = "Could not load uploads" }); //error message if uploads cant be loaded

            var json = await response.Content.ReadAsStringAsync(); //reading the response content as a string from fb
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return Ok(new List<AssignmentUpload>());

            Dictionary<string, AssignmentUpload>? map;
            try
            {
                //Code Attribution
                //The PropertyNameCaseInsensitive option has been created with the help of StackOverflow
                //https://stackoverflow.com/questions/45782127/json-net-case-insensitive-deserialization-not-working
                //Ziaullah Khan
                //https://stackoverflow.com/users/3312570/ziaullah-khan
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
//Microsoft. 2025. HttpClientJsonExtensions Class. [Online]. Microsoft Learn. Available at: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.json.httpclientjsonextensions?view=net-10.0 [Accessed 20 October 2025].

