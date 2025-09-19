namespace uniBuddyAPI.Models
{
    public class Timetable
    {
        public int Id { get; set; }
        public string TimeTableId { get; set; }
        public string Module { get; set; }
        public string DayOfWeek { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Venue { get; set; }
        public string Lecturer { get; set; } 
    }
}
