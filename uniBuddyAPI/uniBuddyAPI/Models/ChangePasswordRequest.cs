namespace uniBuddyAPI.Models
{
    public class ChangePasswordRequest
    {
        public string? UserId { get; set; }
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}
