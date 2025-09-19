namespace uniBuddyAPI.Models
{
    public class CalendarEvent
    {
        public string EventId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; } 
        public string Time { get; set; }         
        public string Description { get; set; }
    }
}
