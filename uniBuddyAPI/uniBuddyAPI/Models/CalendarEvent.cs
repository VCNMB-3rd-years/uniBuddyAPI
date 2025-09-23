namespace uniBuddyAPI.Models
{
    public class CalendarEvent
    {
        public string? EventId { get; set; }
        public string? UserId { get; set; }
        public string Title { get; set; } = "";
        public string? Location { get; set; }
        public string? CourseId { get; set; }
        public string? Notes { get; set; }
        public string? EventDate { get; set; }
    }
}
