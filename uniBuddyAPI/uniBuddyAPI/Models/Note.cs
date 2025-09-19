namespace uniBuddyAPI.Models
{
    public class Note
    {
        public string? NoteId { get; set; }
        public string? UserId { get; set; }
        public string? ModuleId { get; set; }
        public string? Text { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
