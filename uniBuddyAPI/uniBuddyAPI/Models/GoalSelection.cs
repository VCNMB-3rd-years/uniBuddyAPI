namespace uniBuddyAPI.Models
{
    public class GoalSelection
    {
        public int Id { get; set; }           
        public string UserId { get; set; } = "";
        public string GoalId { get; set; } = "";
        public int Points { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
