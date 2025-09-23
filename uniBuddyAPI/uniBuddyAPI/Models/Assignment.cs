namespace uniBuddyAPI.Models
{
    public class Assignment
    {
        public string AssignmentId { get; set; }
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public string Title { get; set; }
        public string Notes { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Still to do";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime CompletedAt { get; set; }

    }
}
