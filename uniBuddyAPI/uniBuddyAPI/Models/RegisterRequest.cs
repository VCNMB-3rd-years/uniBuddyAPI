namespace uniBuddyAPI.Models
{
    public class RegisterRequest
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
