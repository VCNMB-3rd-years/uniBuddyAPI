using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net.Http.Json;
using uniBuddyAPI.Models;
using uniBuddyAPI.Services;

namespace uniBuddyAPI.Controllers
{
    [ApiController]
    [Route("auth")]
    [Produces("application/json")]
    public class AuthController : Controller
    {
        private readonly RealTimeDbService _db;
        public AuthController(RealTimeDbService db)
        {
            _db = db;
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and Password are required." });

            //creating a new user id for a new user object to be stored in fb
            var userId = Guid.NewGuid().ToString();
            var user = new
            {
                userId,
                name = request.Name,
                surname = request.Surname,
                email = request.Email.Trim().ToLower(), //email always in lower
                passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), //hashed pw
                createdAt = DateTime.UtcNow
            };

            //code attribution
            //the PUT request has been done with the help of Firebase documentation
            //https://firebase.google.com/docs/database/rest/save-data#section-put

            var put = await _db.Client.PutAsJsonAsync($"/users/{userId}.json", user);
            put.EnsureSuccessStatusCode();
            //return the user that was created
            return Ok(new { message = "User registered successfully", userId });
        }



        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {

            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and Password are required." });

            var res = await _db.Client.GetAsync("/users.json"); //reading all users in fb
            if (!res.IsSuccessStatusCode) return Unauthorized(new { message = "Login failed." }); //if no user exists with those credentials then return this

            var body = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body) || body == "null")
                return Unauthorized(new { message = "No users found." });

            //Code Attribution
            //The PropertyNameCaseInsensitive option has been created with the help of StackOverflow
            //https://stackoverflow.com/questions/45782127/json-net-case-insensitive-deserialization-not-working
            //Ziaullah Khan
            //https://stackoverflow.com/users/3312570/ziaullah-khan
            var users = JsonSerializer.Deserialize<Dictionary<string, User>>(body, new JsonSerializerOptions //storing in a custom dictionary inspired from PROG7312 poe
            //string is the key and will be the id in firebase
            //User is the value and will be the user model which takes in user info
            {
                PropertyNameCaseInsensitive = true
            });

            if (users == null) return Unauthorized(new { message = "No users found." });

            var email = request.Email.Trim().ToLower();
            //scan users for matching email and pw
            foreach (var kv in users)
            {
                var u = kv.Value;
                if ((u.Email?.Trim().ToLower()) == email &&
                    !string.IsNullOrEmpty(u.PasswordHash) &&
                    BCrypt.Net.BCrypt.Verify(request.Password, u.PasswordHash)) //verify hashed pw after checking it exists
                {
                    var resolvedUserId = string.IsNullOrWhiteSpace(u.UserId) ? kv.Key : u.UserId; //use stored user id or fb key
                    //return user info (name surname and email)
                    return Ok(new LoginResponse { Message = "Login successful", UserId = resolvedUserId!, name = u.Name ?? "", surname = u.Surname ?? "", email = u.Email ?? "" });
                }
            }

            return Unauthorized(new { message = "Invalid email or password." }); //error message
        }

        //checking if user exists in fb by checking email
        [HttpPost("check-user")]
        public async Task<IActionResult> CheckUser([FromBody] CheckUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new { message = "Email is required." });

            var res = await _db.Client.GetAsync("/users.json");
            if (!res.IsSuccessStatusCode) return Unauthorized(new { message = "Failed to access users." });

            var body = await res.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body) || body == "null")
                return NotFound(new { message = "No users found." });

            //Code Attribution
            //The PropertyNameCaseInsensitive option has been created with the help of StackOverflow
            //https://stackoverflow.com/questions/45782127/json-net-case-insensitive-deserialization-not-working
            //Ziaullah Khan
            //https://stackoverflow.com/users/3312570/ziaullah-khan
            var users = JsonSerializer.Deserialize<Dictionary<string, User>>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var email = request.Email.Trim().ToLower();
            foreach (var kv in users)
            {
                var u = kv.Value;
                if ((u.Email?.Trim().ToLower()) == email)
                    return Ok(new { exists = true, userId = u.UserId ?? kv.Key });  //return user id and fb key if found
            }

            return Ok(new { exists = false }); //if no user is found with the provided email
        }

        [HttpPost("password/change")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            //input validation
            if (req == null || string.IsNullOrWhiteSpace(req.UserId) || string.IsNullOrWhiteSpace(req.CurrentPassword) || string.IsNullOrWhiteSpace(req.NewPassword))
                return BadRequest(new { message = "userId, currentPassword and newPassword are required." });

            var get = await _db.Client.GetAsync($"/users/{req.UserId}.json");
            if (!get.IsSuccessStatusCode) return BadRequest(new { message = "User not found." });

            var json = await get.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json) || json == "null") return BadRequest(new { message = "User not found." });

            //Code Attribution
            //The PropertyNameCaseInsensitive option has been created with the help of StackOverflow
            //https://stackoverflow.com/questions/45782127/json-net-case-insensitive-deserialization-not-working
            //Ziaullah Khan
            //https://stackoverflow.com/users/3312570/ziaullah-khan
            var user = JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions 
            {
                PropertyNameCaseInsensitive = true
            });
            if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash))
                return BadRequest(new { message = "Invalid user record." });

            if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash)) //checking that current pw is same as pw in fb
                return BadRequest(new { message = "Current password is incorrect." });

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword); //hash new password and save to db

            var put = await _db.Client.PutAsJsonAsync($"/users/{req.UserId}.json", user);
            if (!put.IsSuccessStatusCode) return BadRequest(new { message = "Failed to update password." }); //error message just incase something goes wrong with the PUT

            return Ok(new { message = "Password updated." });
        }
    }
}