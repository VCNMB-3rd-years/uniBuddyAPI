using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using uniBuddyAPI.Models;
using uniBuddyAPI.Services;

namespace uniBuddyAPI.Controllers
{
    [ApiController]
    [Route("notes")]
    [Produces("application/json")]
    public class NotesController : Controller
    {
        private readonly RealTimeDbService _db;
        public NotesController(RealTimeDbService db)
        {
            _db = db;
        }
        [HttpPost("{userId}")] //adding a note
        public async Task<IActionResult> AddNote(string userId, [FromBody] Note body)
        {
            if (string.IsNullOrWhiteSpace(userId)) //checking for userId in firebase
                return BadRequest(new { message = "The userId is required" });

            if (body == null || string.IsNullOrWhiteSpace(body.ModuleId) || string.IsNullOrWhiteSpace(body.Text)) //checking if the note has text and a selected module code
                return BadRequest(new { message = "ModuleId and Text are required" }); 

            var note = new Note //saving eveerything to firebase from the note
            {
                UserId = userId,
                ModuleId = body.ModuleId,
                Text = body.Text,
                CreatedAt = DateTime.UtcNow
            };

            var response = await _db.Client.PostAsJsonAsync($"/notes/{userId}.json", note); //saving the note under note in firebase

            if (response.IsSuccessStatusCode) //success message if note is saved
            {
                var resultContent = await response.Content.ReadAsStringAsync();
                return Ok(new { message = "Note saved successfully", data = resultContent });
            }
            return BadRequest(new { message = "Failed to save your note" }); //error message if note doesnt save, app wont crash
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotes(string userId, [FromQuery] string? moduleId = null)
        {
            //error checks so app wont crash and note will load
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "The userId is required" });

            var response = await _db.Client.GetAsync($"/notes/{userId}.json");
            if (!response.IsSuccessStatusCode)
                return BadRequest(new { message = "Could not load notes" });

            var json = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return NotFound(new { message = "you have no notes saved yet" }); //errir message if user has no notes

            Dictionary<string, Note>? usersNotes;
            try
            //Code Attribution
            //The PropertyNameCaseInsensitive option has been created with the help of StackOverflow
            //https://stackoverflow.com/questions/45782127/json-net-case-insensitive-deserialization-not-working
            //Ziaullah Khan
            //https://stackoverflow.com/users/3312570/ziaullah-khan
            {
                usersNotes = JsonSerializer.Deserialize<Dictionary<string, Note>>(json, new JsonSerializerOptions //deserialize the json from firebase into the dictionary
                {
                    PropertyNameCaseInsensitive = true //just making sure the moduleId is not case sensitive as its moduleId in firebase
                });
            }
            catch
            {
                return Ok(new List<Note>()); //empty list if notes isnt deserialized
            }

            var list = new List<Note>();
            if (usersNotes != null)
            {
                foreach (var entry in usersNotes)
                {
                    var note = entry.Value;
                    note.NoteId = string.IsNullOrWhiteSpace(note.NoteId) ? entry.Key : note.NoteId; //use Firebase key if note id is null
                    list.Add(note);
                }
            }


            if (!string.IsNullOrWhiteSpace(moduleId)) //module filter
            {
                list = list
                    .Where(n => string.Equals(n.ModuleId, moduleId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return Ok(list.OrderByDescending(n => n.CreatedAt).ToList());
        }

        [HttpDelete("{userId}/{noteId}")] //deleting a specific note
        public async Task<IActionResult> DeleteNote(string userId, string noteId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(noteId)) //input validation
                return BadRequest(new { message = "userId and noteId are required" });

            var response = await _db.Client.DeleteAsync($"/notes/{userId}/{noteId}.json"); //deleting the note from firebase

            if (response.IsSuccessStatusCode) //message if note is deleted
                return Ok(new { message = "Note deleted" });

            return BadRequest(new {message = "Could not delete note" });
        }
    }
}
