using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using uniBuddyAPI.Models;
using uniBuddyAPI.Services;

namespace uniBuddyAPI.Controllers
{
    [ApiController]
    [Route("vouchers")]
    [Produces("application/json")]
    public class VouchersController : ControllerBase
    {
        private readonly RealTimeDbService _db;

        public VouchersController(RealTimeDbService db)
        {
            _db = db;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetEarned(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "userId is required" });

            var response = await _db.Client.GetAsync($"/vouchers/{userId}.json"); //get from firebase
            if (!response.IsSuccessStatusCode) return Ok(new List<EarnedVoucher>());

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body) || body == "null")
                return Ok(new List<EarnedVoucher>());

            try
            {
                //Code Attribution
                //The PropertyNameCaseInsensitive option has been created with the help of StackOverflow
                //https://stackoverflow.com/questions/45782127/json-net-case-insensitive-deserialization-not-working
                //Ziaullah Khan
                //https://stackoverflow.com/users/3312570/ziaullah-khan
                var earned = JsonSerializer.Deserialize<Dictionary<string, EarnedVoucher>>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (earned == null) return Ok(new List<EarnedVoucher>());

                foreach (var (key, value) in earned)
                    if (string.IsNullOrWhiteSpace(value.VoucherId)) value.VoucherId = key;

                return Ok(earned.Values.OrderByDescending(v => v.AwardedAt).ToList());
            }
            catch
            {
                return Ok(new List<EarnedVoucher>());
            }
        }

        [HttpPost("evaluate/{userId}")]
        public async Task<IActionResult> Evaluate(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "userId is required" });

            var studyMinutes = await SumStudyMinutes(userId); //adding study minutes for user
            var notesCount = await CountNotes(userId); //adding notes count for user

            var metrics = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["studyMinutes"] = studyMinutes, //storing study minutes
                ["notesCount"] = notesCount //storing notes count
            };

            var rules = new (string id, string title, string metric, int threshold)[]
            {
                //study timer vouchers
                ("redbull_1", "Study 1 minute to earn a free Red Bull", "studyMinutes", 1), //1 min for testing in presentation
                ("cappuccino_30", "Study 30 minutes to earn a Cappuccino voucher", "studyMinutes", 30),
                ("bagel_120", "Study 2 hours to earn a breakfast bagel voucher", "studyMinutes", 120),

                //notes vouchers
                ("notes_5", "Write 5 notes to earn a Beef Samoosa", "notesCount", 5),
                ("notes_20", "Write 20 notes to earn a Toastie of your choice", "notesCount", 20)
            };

            var earned = await LoadEarned(userId);
            var earnedIds = new HashSet<string>(earned.Select(v => v.VoucherId), StringComparer.OrdinalIgnoreCase);

            var newly = new List<EarnedVoucher>();
            var progress = new List<VoucherProgress>();

            foreach (var (id, title, metric, threshold) in rules)
            {
                var current = metrics.TryGetValue(metric, out var val) ? val : 0;
                var achieved = current >= threshold;  //when amount is over threshold
                var percent = threshold > 0 ? Math.Min(100, (int)Math.Round(current * 100.0 / threshold)) : 0; //capped at 100%

                progress.Add(new VoucherProgress
                {
                    VoucherId = id,
                    Title = title,
                    MetricType = metric,
                    Threshold = threshold,
                    Current = current,
                    Percent = percent,
                    Achieved = achieved
                });

                if (achieved && !earnedIds.Contains(id))
                {
                    var award = new EarnedVoucher 
                    {
                        VoucherId = id,
                        Title = title,
                        MetricType = metric,
                        Threshold = threshold,
                        ValueAtAward = current,
                        AwardedAt = DateTime.UtcNow,
                        RedeemCode = GenerateCode()
                    };

                    var put = await _db.Client.PutAsJsonAsync($"/vouchers/{userId}/{id}.json", award);
                    if (put.IsSuccessStatusCode)
                        newly.Add(award);
                }
            }

            return Ok(new EvaluateResponse { Progress = progress, NewlyAwarded = newly });
        }

        private async Task<List<EarnedVoucher>> LoadEarned(string userId)
        {
            var response = await _db.Client.GetAsync($"/vouchers/{userId}.json");
            if (!response.IsSuccessStatusCode) return new List<EarnedVoucher>();

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body) || body == "null") return new List<EarnedVoucher>();

            try
            {
                var earned = JsonSerializer.Deserialize<Dictionary<string, EarnedVoucher>>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (earned == null) return new List<EarnedVoucher>();

                foreach (var (key, value) in earned)
                    if (string.IsNullOrWhiteSpace(value.VoucherId)) value.VoucherId = key;

                return earned.Values.ToList();
            }
            catch
            {
                return new List<EarnedVoucher>();
            }
        }

        private async Task<int> SumStudyMinutes(string userId)
        {
            var response = await _db.Client.GetAsync($"/studySession/{userId}.json");
            if (!response.IsSuccessStatusCode) return 0;

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body) || body == "null") return 0;

            try
            {
                //Code Attribution
                //The PropertyNameCaseInsensitive option has been created with the help of StackOverflow
                //https://stackoverflow.com/questions/45782127/json-net-case-insensitive-deserialization-not-working
                //Ziaullah Khan
                //https://stackoverflow.com/users/3312570/ziaullah-khan
                var sessions = JsonSerializer.Deserialize<Dictionary<string, StudySession>>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return sessions?.Values.Sum(s => Math.Max(0, s.Duration)) ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<int> CountNotes(string userId)
        {
            var response = await _db.Client.GetAsync($"/notes/{userId}.json");
            if (!response.IsSuccessStatusCode) return 0;

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body) || body == "null") return 0;

            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    return doc.RootElement.GetArrayLength();
                }
                else if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    int count = 0;
                    foreach (var _ in doc.RootElement.EnumerateObject()) count++;
                    return count;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private static string GenerateCode()
        //Code Attribution
        //This code generator was created with the help of StackOverflow
        //https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings
        //Wai Ha Lee
        //https://stackoverflow.com/users/1364007/wai-ha-lee
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
            var rng = new Random();
            return new string(Enumerable.Range(0, 6).Select(_ => chars[rng.Next(chars.Length)]).ToArray());
        }
    }
}
//Reference List
//Lee, W.H. 2021. How can I generate random alphanumeric strings? [Online]. StackOverflow. Available at: https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings [Accessed 3 November 2025].
